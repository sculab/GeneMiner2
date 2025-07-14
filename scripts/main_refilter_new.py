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

FWD_TRANS = collections.defaultdict(lambda: None, str.maketrans("ACGTUacgtu", "0123301233"))
REV_TRANS = collections.defaultdict(lambda: None, str.maketrans("ACGTUacgtu", "3210032100"))

def print_log(log_path, *args, **kwargs):
    if log_path:
        with open(log_path, 'a') as out:
            print(*args, file=out, **kwargs)

    print(*args, **kwargs)

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

    with os.scandir(ref_dir) as entries:
        for ent in entries:
            basename, extname = os.path.splitext(ent.name)

            if extname not in FILE_TYPES:
                continue

            if basename in ref_dict:
                raise ValueError(f'Duplicate reference sequence name {basename}.')

            ref_dict[basename] = ent.path

    return ref_dict

def load_reference(ref_path, kmer_size):
    with open(ref_path, 'r') as f:
        ref_set = {seq for _, seq in SimpleFastaParser(f) if len(seq) >= kmer_size}

    if not ref_set:
        return ref_set, 0

    length_list = list(map(len, ref_set))
    effective_len = int(max(length_list) * (math.log10(len(length_list)) + 1))
    return ref_set, effective_len

def build_kmer_dict(ref_set, kmer_size, trans=FWD_TRANS, rtrans=REV_TRANS):
    # Values: 0=unused; 1=forward; 2=reverse; 3=both
    kmer_dict = collections.defaultdict(lambda: 0)
    mask_bin  = (1 << (kmer_size << 1)) - 1

    for seq in ref_set:
        seq_str   = seq.translate(trans)
        seq_str_r = seq.translate(rtrans)[::-1]

        if not seq_str:
            continue

        seq_int   = int(seq_str, 4)
        seq_int_r = int(seq_str_r, 4)

        for _ in range(0, len(seq_str) - kmer_size + 1):
            kmer_dict[seq_int   & mask_bin] |= 1
            kmer_dict[seq_int_r & mask_bin] |= 2
            seq_int   >>= 2
            seq_int_r >>= 2

    return kmer_dict

def collect_runs_stats(reads, kmer_dict, kmer_size, trans=FWD_TRANS, zero_stats=(0, ) * 12):
    # Return values: [0:4]=best length; [4:8]=run count; [8:12]=hit count; [12]=k-mer count
    mask_bin = (1 << (kmer_size << 1)) - 1

    for tp in reads:
        read_str = tp[1].translate(trans)
        kmer_cnt = len(read_str) - kmer_size + 1
        results  = [*zero_stats, kmer_cnt]

        if kmer_cnt > 0:
            curr_dir    = 0
            curr_len    = 0
            read_int    = int(read_str, 4)

            for _ in range(0, kmer_cnt):
                kmer = read_int & mask_bin
                orient = 0
                read_int >>= 2

                if kmer in kmer_dict:
                    orient = kmer_dict[kmer]

                if orient != curr_dir:
                    if curr_len > results[curr_dir]:
                        results[curr_dir] = curr_len

                    results[curr_dir + 4] += 1
                    curr_dir = orient
                    curr_len = 0

                if curr_dir != 0:
                    curr_len += 1
                    results[curr_dir + 8] += 1

            if curr_len > results[curr_dir]:
                results[curr_dir] = curr_len

            results[curr_dir + 4] += 1
        else:
            results[12] = 0

        yield results

def run_length_filter(name, out_dir, ref_set, ref_length, read_info, file_type, kmer_size, keep_temporaries):
    RUN_LEN_CONST = 0.5772156649 / math.log(2) - 1.5
    THR_P95_2T = 1.96
    THR_1e5_1T = 3.74
    TOLERANCE = 1e-5

    output_ext  = FILE_EXTENSION[file_type]
    output_path = os.path.join(out_dir, 'large_files', name + output_ext)
    format_func = FORMAT_FUNCTIONS[file_type]
    read_iter   = READ_ITERATORS[file_type]
    open_flags  = os.O_WRONLY | os.O_CREAT | os.O_TRUNC

    if os.name == 'nt' and not keep_temporaries:
        open_flags |= os.O_SHORT_LIVED

    kmer_dict = build_kmer_dict(ref_set, kmer_size)

    with contextlib.ExitStack() as stack:
        read_iters  = [read_iter(stack.enter_context(open(path, 'r'))) for path in read_info]
        output_file = stack.enter_context(os.fdopen(os.open(output_path, open_flags), 'w'))

        for linked_reads in zip(*read_iters):
            orient = [0] * len(linked_reads)

            for i, (_, fwd_l, rev_l, _,
                    _, fwd_r, rev_r, _,
                    _, fwd_n, rev_n, amb_n, tot_n) in enumerate(collect_runs_stats(linked_reads, kmer_dict, kmer_size)):

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

                # Note that we count the runs for all four states
                # (mismatch, forward, reverse, ambiguous)
                # but we calculate the expected number of runs with two states
                # This is equivalent to spliting a run into two whenever
                # a mismatch is encountered
                # In principle, E(R) = 2*n1*n2/(n1+n2)+mutation_rate*(n1+n2)+1
                # However, we choose to ignore the mutation term and tolerate
                # a bounded multiplier on E(R), implying much higher stringency
                npr = 2 * fwd_n * rev_n
                nht = fwd_n + rev_n

                # E(R) in standard runs test
                # 2*n1*n2/(n1+n2)+1
                erc = npr / nht + 1

                # Var(R) in standard runs test
                # 2*n1*n2*(2*n1*n2-n1-n2)/(n1+n2)^2*(n1+n2-1)
                vrn = npr * (npr - nht) / (nht * nht * (nht - 1))

                # The direction of runs does not autocorrelate enough
                # i.e. the runs are either random or somehow periodic
                if (fwd_r + rev_r - erc) / math.sqrt(vrn) > -THR_1e5_1T:

                    # Next, we assume some mismatches caused the difference of
                    # the numbers of runs
                    # Let pf be the error rate in the forward matching region
                    # pf = (fwd_r - true_fwd_r) / (fwd_n / (1 - pf))
                    if fwd_r > rev_r:
                        ntt = fwd_n + fwd_r - rev_r
                        rex = fwd_n / ntt
                    else:
                        ntt = fwd_n + rev_r - fwd_r
                        rex = fwd_n / ntt

                    # If we cannot infer that the direction with more runs has
                    # a significant proportion of mismatches against reference
                    # then call a chimera
                    if math.isclose(rex, 1.0, abs_tol=TOLERANCE) or (1 - rex) / math.sqrt(rex * (1 - rex) / ntt) < THR_P95_2T:
                        orient[i] = 0
                        continue

                erl = max(math.log2(tot_n) + RUN_LEN_CONST, 0) + 4
                orient[i] = (fwd_l > erl) + (rev_l > erl) * 2

                # The orientation is unambiguous
                if orient[i] != 3:
                    continue

                # If the longest forward and reverse runs are both much longer
                # than the expected run-length, assume all forward and reverse
                # matches are contiguous and calculate an approximate mutation
                # rate (https://math.stackexchange.com/a/5027331)
                # If the mutation rates are too similar, we consider matches in
                # both directions 'similarly good', thereby calling a chimera
                # As a rule of thumb, we reject reads if the forward region and
                # the reverse region share the same distribution, because their
                # chimeric appearance impedes reference-guided assembly
                lpf = math.exp(math.log(1 / (1 - fwd_l + fwd_n)) / fwd_l)
                lpr = math.exp(math.log(1 / (1 - rev_l + rev_n)) / rev_l)
                fpz = math.isclose(lpf, 0.0, abs_tol=TOLERANCE)
                rpz = math.isclose(lpr, 0.0, abs_tol=TOLERANCE)

                if fpz:
                    orient[i] = 2 - 2 * rpz
                elif rpz:
                    orient[i] = 1
                elif math.isclose(lpf, 1.0, abs_tol=TOLERANCE) and math.isclose(lpr, 1.0, abs_tol=TOLERANCE):
                    orient[i] = 0
                elif abs(lpf - lpr) / math.sqrt(lpf ** 2 * (1 - lpf) / fwd_n + lpr ** 2 * (1 - lpr) / rev_n) < THR_P95_2T:
                    orient[i] = 0

            # Discordant paired reads
            if len(orient) == 2 and 1 <= orient[0] <= 2 and orient[0] == orient[1]:
                continue

            output_file.writelines(format_func(tp) for i, tp in enumerate(linked_reads) if orient[i])

    return output_path

def filter_read(read, kmer_dict, kmer_size, trans=FWD_TRANS):
    mask_bin = (1 << (kmer_size << 1)) - 1
    read_str = read.translate(trans)

    if len(read_str) < kmer_size:
        return False

    read_int = int(read_str, 4)

    return any(True
               for i in range(0, len(read_str) - kmer_size + 1)
               if ((read_int >> (2 * i)) & mask_bin) in kmer_dict)

def kmer_filter(name, out_dir, log_path, ref_set, ref_length, temp_path, file_type, kmer_size, min_depth, max_depth, max_size, keep_temporaries):
    output_ext  = FILE_EXTENSION[file_type]
    output_path = os.path.join(out_dir, name + output_ext)
    format_func = FORMAT_FUNCTIONS[file_type]
    read_iter   = READ_ITERATORS[file_type]

    with open(temp_path, 'r') as f:
        total_length = sum(len(tp[1]) for tp in read_iter(f))

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

        print_log(log_path, f'K-mer size for {name}: {kmer_size}')

        kmer_dict = build_kmer_dict(ref_set, kmer_size)

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

    kmer_dict = build_kmer_dict(ref_set, kmer_size)
    interval = max(int(total_length / 1e6 / max_size), 2)
    i = 0

    with open(temp_path, 'r') as f, open(output_path, 'w') as fo:
        for tp in read_iter(f):
            if filter_read(tp[1], kmer_dict, kmer_size):
                i += 1

                if too_large and i % interval != 0:
                    continue

                fo.write(format_func(tp))

def filter_gene(task):
    file_ext = os.path.splitext(task.read_path[0])[1]

    if file_ext not in FILE_TYPES:
        print_log(task.log_path, f"File '{task.read_path[0]}' has invalid file type.")
        return

    print_log(task.log_path, f'Filtering gene {task.name}.')

    file_type = FILE_TYPES[file_ext]
    ref_set, effective_len = load_reference(task.ref_path, task.kmer_size)

    if not effective_len:
        print_log(task.log_path, f'Gene {task.name} has no valid reference.')
        return

    # On the weird choice of k-mer size
    # Assume the sequencing error rate to be 0.95
    # 1 - 0.95^13 = 0.4867 < 0.5
    # On average, one of two clusters of biological k-mer matches is error-free
    tmp_path = run_length_filter(task.name, task.out_dir, ref_set, effective_len,
                                 task.read_path, file_type, max(task.kmer_size // 2, task.kmer_size - 13) | 1,
                                 task.keep_temporaries)

    kmer_filter(task.name, task.out_dir, task.log_path,
                ref_set, effective_len, tmp_path, file_type,
                task.kmer_size, task.min_depth, task.max_depth,
                task.max_size, task.keep_temporaries)

    if not task.keep_temporaries:
        os.unlink(tmp_path)

Task = collections.namedtuple('Task', ('name', 'out_dir', 'ref_path', 'read_path',
                                       'log_path', 'min_depth', 'max_depth',
                                       'max_size', 'keep_temporaries', 'kmer_size'))

def run(args):
    tasks = []

    for name, ref_path in ref_dict.items():
        if name not in read_dict:
            print_log(args.log_file, f"No reads for gene {name}.")
            continue

        tasks.append(Task(name, out_dir, ref_path, read_dict[name], args.log_file,
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

    parser.add_argument('--log-file', default=None, help='Log file')
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
