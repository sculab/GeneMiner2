from Bio.SeqIO.FastaIO import SimpleFastaParser
from Bio.SeqIO.QualityIO import FastqGeneralIterator
import argparse
import collections
import contextlib
import math
import os
import shutil

FILE_EXTENSION = {
    'fasta': '.fasta',
    'fastq': '.fq'
}

FILE_TYPES = {
    '.fa': 'fasta',
    '.fas': 'fasta',
    '.fasta': 'fasta',
    '.fq': 'fastq',
    '.fastq': 'fastq'
}

FORMAT_FUNCTIONS = {
    'fasta': lambda t: f'>{t[0]}\n{t[1]}\n',
    'fastq': lambda t: f'@{t[0]}\n{t[1]}\n+\n{t[2]}\n'
}

READ_ITERATORS = {
   'fasta': SimpleFastaParser,
   'fastq': FastqGeneralIterator
}

FWD_TRANS = str.maketrans("ACGTU", "01233", "RYMKSWHBVDN\n\r")
REV_TRANS = str.maketrans("ACGTU", "32100", "RYMKSWHBVDN\n\r")

def get_read_dict(se_dir, pe_dir):
    read_dict = {}
    walk_directory = lambda path: ((os.path.dirname(ent.path), *os.path.splitext(ent.name)) for ent in os.scandir(path) if ent.is_file())

    if se_dir:
        if not os.path.isdir(se_dir):
            raise ValueError('Argument --se-dir does not refer to a directory.')

        for dirname, basename, extname in walk_directory(se_dir):
            if extname not in FILE_TYPES:
                continue

            if basename in read_dict:
                raise ValueError(f'Duplicate read group name {basename}.')

            read_dict[basename] = (f'{dirname}/{basename}{extname}', )

    if pe_dir:
        if not os.path.isdir(pe_dir):
            raise ValueError('Argument --pe-dir does not refer to a directory.')

        for dirname, basename, extname in walk_directory(pe_dir):
            if extname not in FILE_TYPES or not basename.endswith('_1'):
                continue

            gene_name = basename[:-2]

            if gene_name in read_dict:
                raise ValueError(f'Duplicate read group name {gene_name}.')

            forward_path = os.path.join(dirname, f'{gene_name}_1{extname}')
            reverse_path = os.path.join(dirname, f'{gene_name}_2{extname}')

            if os.path.isfile(reverse_path):
                read_dict[gene_name] = (forward_path, reverse_path)
            else:
                read_dict[gene_name] = (forward_path, )

    return read_dict

def get_ref_dict(ref_dir):
    ref_dict = {}

    for ent in os.scandir(ref_dir):
        basename, extname = os.path.splitext(ent.name)

        if extname not in FILE_TYPES:
            continue

        if basename in ref_dict:
            raise ValueError(f'Duplicate reference sequence name {basename}.')

        ref_dict[basename] = ent.path

    return ref_dict

def build_kmer_dict(ref_path, kmer_size, trans=FWD_TRANS, rtrans=REV_TRANS):

    # Values: 0=unused; 1=forward; 2=reverse; 3=both
    kmer_dict = collections.defaultdict(lambda: 0)
    mask_bin  = (1 << (kmer_size << 1)) - 1

    with open(ref_path, 'r') as f:
        for _, seq in SimpleFastaParser(f):
            seq       = ''.join(filter(str.isalpha, seq)).upper()
            seq_str   = seq.translate(trans)
            seq_str_r = seq.translate(rtrans)[::-1]
            seq_int   = int(seq_str, 4)
            seq_int_r = int(seq_str_r, 4)

            for _ in range(0, len(seq_str) - kmer_size + 1):
                kmer_dict[seq_int   & mask_bin] |= 1
                kmer_dict[seq_int_r & mask_bin] |= 2
                seq_int   >>= 2
                seq_int_r >>= 2

    return kmer_dict

def filter_read(read, kmer_dict, kmer_size, trans=FWD_TRANS):
    if len(read) < kmer_size:
        return False

    mask_bin = (1 << (kmer_size << 1)) - 1
    read_str = read.upper().translate(trans)
    read_int = int(read_str, 4)

    for j in range(0, len(read_str) - kmer_size + 1):
        if (read_int & mask_bin) in kmer_dict:
            return True

        read_int >>= 2

    return False

def collect_runs_stats(read, kmer_dict, kmer_size, trans=FWD_TRANS):
    read_str = read.upper().translate(trans)

    if len(read_str) < kmer_size:
        return 0, 0, 0, 0, 0, 0, 0

    mask_bin = (1 << (kmer_size << 1)) - 1
    read_int = int(read_str, 4)

    curr_dir = 0
    curr_len = 0

    best_len = [0] * 4
    hit_cnt  = [0] * 4
    run_cnt  = [0] * 4

    for _ in range(0, len(read_str) - kmer_size + 1):
        kmer = read_int & mask_bin
        read_int >>= 2
        orient = int(kmer in kmer_dict and kmer_dict[kmer])

        if orient != curr_dir:
            run_cnt[curr_dir] += 1

            if curr_len > best_len[curr_dir]:
                best_len[curr_dir] = curr_len

            curr_dir = orient
            curr_len = 0

        curr_len += 1
        hit_cnt[curr_dir] += 1

    run_cnt[curr_dir] += 1

    if curr_len > best_len[curr_dir]:
        best_len[curr_dir] = curr_len

    return hit_cnt[1], hit_cnt[2], hit_cnt[3], run_cnt[1], run_cnt[2], best_len[1], best_len[2]

def run_length_filter(name, out_dir, ref_path, ref_length, read_info, file_type, kmer_size, keep_temporaries):
    # 1e-5
    THRESHOLD = 3.74

    output_ext  = FILE_EXTENSION[file_type]
    output_path = os.path.join(out_dir, 'large_files', name + output_ext)
    format_func = FORMAT_FUNCTIONS[file_type]
    read_iter   = READ_ITERATORS[file_type]
    open_flags  = os.O_WRONLY | os.O_CREAT | os.O_TRUNC

    if os.name == 'nt' and not keep_temporaries:
        open_flags |= os.O_SHORT_LIVED

    kmer_dict = build_kmer_dict(ref_path, kmer_size)

    with contextlib.ExitStack() as stack:
        read_iters  = [read_iter(stack.enter_context(open(path, 'r'))) for path in read_info]
        output_file = stack.enter_context(os.fdopen(os.open(output_path, open_flags), 'w'))

        for linked_reads in zip(*read_iters):
            orient = [0] * len(linked_reads)

            for i, (fwd_n, rev_n, amb_n,
                    fwd_r, rev_r, fwd_l, rev_l) in enumerate(collect_runs_stats(tp[1], kmer_dict, kmer_size)
                                                             for tp in linked_reads):

                # Forward hits    Reverse hits    Ambiguous hits    Verdict
                # -----------------------------------------------------------
                # <= 1            <= 1            <= 1              reject
                # <= 1            <= 1            > 1               ambiguous
                # <= 1            > 1             *                 reverse
                # > 1             <= 1            *                 forward
                # > 1             > 1             *                 continue
                if fwd_n <= 1:
                    orient[i] = (rev_n <= 1) * (1 - (amb_n <= 1) * 3) + 2
                    continue
                elif rev_n <= 1:
                    orient[i] = 1
                    continue

                dpr = 2 * fwd_n * rev_n
                ttn = fwd_n + rev_n
                dpf = ttn * ttn / 2
                rma = dpr / ttn + 1
                va1 = dpr * (dpr - ttn) / (dpr * dpr * (dpr - 1))
                va2 = dpf * (dpf - ttn) / (dpf * dpf * (dpf - 1))
                zto = abs((fwd_r + rev_r - rma) / math.sqrt(va1))
                zcf = abs((fwd_r - rev_r) / math.sqrt(va2 / 2))

                # Number of runs does not differ significantly from random
                # and the difference between the numbers of forward and reverse
                # runs is insignificant
                if zto < THRESHOLD and zcf < THRESHOLD:
                    orient[i] = 0
                    continue

                # E[max{R_n}] has a rather small variance
                # For any run, the expected extension length is
                # 1/2 * 0 + 1/4 * 1 + 1/8 * 2 + 1/16 * 3 + ... < 2
                # Taking the first 5 terms
                # 1/2 * 0 + 1/4 * 1 + 1/8 * 2 + 1/16 * 3 + 1/32 * 4 = 0.8125
                # Giving a confidence level at
                # 1/2 + 1/4 + 1/8 + 1/16 + 1/32 = 0.96875 > 0.95
                # See https://math.stackexchange.com/a/1414950
                rle = max(math.log2(ttn) + 0.5772156649 / math.log(2) - 3 / 2, 0) + 0.8125
                orient[i] = (fwd_l > rle) + (rev_l > rle) * 2

                if orient[i] == 3:
                    pfl = fwd_l / fwd_n
                    prl = rev_l / rev_n
                    epr = (fwd_l + rev_l) / ttn
                    ept = epr / 2
                    vpr = math.sqrt(epr * (1 - epr) / ttn)
                    vph = math.sqrt(ept * (1 - ept) / ttn)

                    if (epr != 1 and abs((pfl - prl) / vpr) < THRESHOLD) or abs((fwd_l - rev_l) / ttn / vph) < THRESHOLD:
                        orient[i] = 0

            # Discordant paired reads
            if len(orient) == 2 and 1 <= orient[0] <= 2 and orient[0] == orient[1]:
                continue

            output_file.writelines(format_func(t) for i, t in enumerate(linked_reads) if orient[i])

    return output_path

def kmer_filter(name, out_dir, ref_path, ref_length, temp_path, file_type, kmer_size, min_depth, max_depth, max_size, keep_temporaries):
    output_ext  = FILE_EXTENSION[file_type]
    output_path = os.path.join(out_dir, name + output_ext)
    format_func = FORMAT_FUNCTIONS[file_type]
    read_iter   = READ_ITERATORS[file_type]

    with open(temp_path, 'r') as f:
        total_length = sum(len(t[1]) for t in read_iter(f))

    coverage  = total_length / ref_length
    too_deep  = coverage > max_depth
    too_large = total_length // 1e6 > max_size

    if not too_deep and not too_large:
        return shutil.copyfile(temp_path, output_path)

    min_depth = min(min_depth, max_depth / 4)
    initial_kmer_size = kmer_size

    # NOTE: the largest possible k-mer size is 63 + 6 = 69
    while kmer_size < 64 and (too_deep or too_large):
        last_kmer_size = kmer_size
        last_length = total_length

        if coverage > 8 * max_depth or total_length // 1e6 > 6 * max_size:
            kmer_size += 6
        else:
            kmer_size += 2

        print(f'K-mer size for {name}: {kmer_size}')

        kmer_dict = build_kmer_dict(ref_path, kmer_size)

        with open(temp_path, 'r') as f:
            total_length = sum(len(tp[1])
                               for tp in read_iter(f)
                               if filter_read(tp[1], kmer_dict, kmer_size))

        coverage  = total_length / ref_length
        too_deep  = coverage > max_depth
        too_large = total_length // 1e6 > max_size

        if coverage < min_depth:
            kmer_size    = last_kmer_size
            total_length = last_length
            coverage     = total_length / ref_length
            too_large    = total_length // 1e6 > max_size
            break

    if kmer_size == initial_kmer_size and not too_large:
        return shutil.copyfile(temp_path, output_path)

    kmer_dict = build_kmer_dict(ref_path, kmer_size)
    interval = max(int(total_length / 1e6 / max_size), 2)
    i = 0

    with open(temp_path, 'r') as f, open(output_path, 'w') as fo:
        for tp in read_iter(f):
            if filter_read(tp[1], kmer_dict, kmer_size):
                i += 1

                if too_large and i % interval != 0:
                    continue

                fo.write(format_func(tp))

def calc_effective_len(ref_path):
    with open(ref_path, 'r') as f:
        length_list = [len(seq) for _, seq in SimpleFastaParser(f)]

    return max(length_list) * ((math.log2(len(length_list) + 1) + 1) // 2)

def filter_gene(task):
    file_ext = os.path.splitext(task.read_path[0])[1]

    if file_ext not in FILE_TYPES:
        print(f"File '{task.read_path[0]}' has invalid file type.")
        return

    print(f'Filtering gene {task.name}.')

    effective_len = calc_effective_len(task.ref_path)
    file_type = FILE_TYPES[file_ext]

    tmp_path = run_length_filter(task.name, task.out_dir, task.ref_path,
                                 effective_len, task.read_path, file_type,
                                 task.kmer_size - 8, task.keep_temporaries)

    kmer_filter(task.name, task.out_dir, task.ref_path,
                effective_len, tmp_path, file_type,
                task.kmer_size, task.min_depth, task.max_depth,
                task.max_size, task.keep_temporaries)

    if not task.keep_temporaries:
        os.unlink(tmp_path)

Task = collections.namedtuple('Task', ('name', 'out_dir', 'ref_path', 'read_path',
                                       'min_depth', 'max_depth', 'max_size',
                                       'keep_temporaries', 'kmer_size'))

def run(args):
    tasks = []

    for name, ref_path in ref_dict.items():
        if name not in read_dict:
            print(f"No reads for reference file '{ref_path}'.")
            continue

        tasks.append(Task(name, out_dir, ref_path, read_dict[name],
                          args.min_depth, args.max_depth, args.max_size,
                          args.keep_temporaries, args.kmer_size))

    if args.processes > 1:
        with multiprocessing.Pool(args.processes) as pool:
            for _ in pool.imap_unordered(filter_gene, tasks):
                pass
    else:
        for task in tasks:
            filter_gene(task)

    if not args.keep_temporaries:
        try:
            os.rmdir(os.path.join(out_dir, 'large_files'))
        except OSError:
            pass

if __name__ == '__main__':
    import multiprocessing

    if os.name == 'nt':
        multiprocessing.freeze_support()

    parser = argparse.ArgumentParser(description='An improved NGS filtering gadget based on k-mers.')

    input_group = parser.add_mutually_exclusive_group(required=True)
    input_group.add_argument('-qs', '--se-dir', help='Directory with single-read sequencing data')
    input_group.add_argument('-qd', '--pe-dir', help='Directory with paired-end sequencing data')

    parser.add_argument('-r', '--ref-dir', required=True, help='Directory with reference sequences')
    parser.add_argument('-o', '--out-dir', required=True, help='Output directory')

    parser.add_argument('--min-depth', default=50, help='Min allowed coverage', type=int)
    parser.add_argument('--max-depth', default=768, help='Max allowed coverage', type=int)
    parser.add_argument('--max-size', default=6, help='Max allowed size in million bases', type=int)
    parser.add_argument('--keep-temporaries', action='store_true', help='Keep temporary files')
    parser.add_argument('-kf', '--kmer-size', default=31, help='K-mer size', type=int)

    parser.add_argument('-p', '--processes', default=1, help='Number of parallel processes', type=int)

    args = parser.parse_args()

    try:
        read_dict = get_read_dict(args.se_dir, args.pe_dir)
        ref_dict = get_ref_dict(args.ref_dir)
        out_dir = args.out_dir
        os.makedirs(os.path.join(out_dir, 'large_files'), exist_ok=True)
    except (OSError, ValueError) as e:
        parser.error(str(e))
    else:
        run(args)
