from Bio.SeqIO.FastaIO import SimpleFastaParser
from concurrent.futures import FIRST_COMPLETED, ThreadPoolExecutor, wait
import argparse
import csv
import math
import os
import shlex
import shutil
import statistics
import subprocess
import sys

import build_trimed

COMMAND_HELP = '''
filter    Reference-based filtering of raw reads
refilter  Refinement of filtered reads
assemble  Gene assembly using wDBG
consensus Consensus generation on heterozygous sites
trim      Flank sequence removal
combine   Gene alignment, concatenation and cleanup
'''

SCRIPT_ROOT = os.path.join(sys._MEIPASS, os.pardir) if hasattr(sys, '_MEIPASS') else os.path.dirname(__file__)

def find_executable(prog, internal=False):
    bin_path = os.path.join(SCRIPT_ROOT, prog)

    if not shutil.which(bin_path):
        if internal:
            raise RuntimeError(f"A GeneMiner component is missing from '{bin_path}'")
        else:
            bin_path = shutil.which(prog)

    if not bin_path:
        raise RuntimeError(f"Unable to find {prog} executable")

    return bin_path

def get_sample_ext(data_path):
    data_name, data_ext = os.path.splitext(data_path)

    if data_ext == '.gz':
        data_name, data_ext = os.path.splitext(data_name)

    if data_ext == '.fq' or data_ext == '.fastq':
        return '.fq'
    else:
        return '.fasta'

def prepare_workdir(args):
    samples = {}
    tsv_loc = args.f

    try:
        sp_id = 0
        tsv_loc = os.path.realpath(tsv_loc, strict=True)

        with open(tsv_loc, 'r') as f:
            for row in csv.reader(f, delimiter="\t", quotechar='"'):
                if not row:
                    continue

                sp_id += 1
                sp_name = "".join('_' if c in ' -' else c for c in row[0].strip() if c.isalnum() or c in " -_.").capitalize()

                if not sp_name:
                    print(f"Invalid sample name '{row[0]}'")
                    return {}

                if len(row) == 1:
                    print(f"Sample '{row[0]}' has no data files")
                    return {}

                samples[f'{sp_id}_{sp_name}'] = (row[1], row[1] if len(row) == 2 else row[2])

    except OSError as e:
        print(f"Unable to read sample list '{tsv_loc}': {e}")
        return {}

    out_loc = args.o.strip()

    print(f"Preparing working directory '{out_loc}'")

    try:
        os.makedirs(out_loc, exist_ok=True)
    except OSError as e:
        print(f"Unable to create working directory '{out_loc}': {e}")
        return {}

    for name in samples.keys():
        sp_path = os.path.join(out_loc, name)

        try:
            os.makedirs(sp_path, exist_ok=True)
        except OSError as e:
            print(f"Unable to create directory '{sp_path}': {e}")
            return {}

    return samples

def do_filter_assemble(args, samples, do_filter, do_refilter, do_assemble, ignore_hook=lambda *_, **__: None):
    out_loc = args.o.strip()
    kmer_dict_path = os.path.join(out_loc, f'kmer_dict_k{args.kf}.dict')

    if do_filter:
        filter_bin = find_executable('MainFilterNew', internal=True)

        try:
            subprocess.run([filter_bin, '-r', args.r, '-o', out_loc, '-kf', str(args.kf), '-s', str(args.step_size),
                            '-gr', '-lkd', kmer_dict_path, '-m', '2'], check=True)
        except subprocess.SubprocessError as e:
            raise RuntimeError(f"Unable to build k-mer dictionary: {e}")

        def run_filter(name):
            q1, q2 = samples[name]
            read_count_path = os.path.join(out_loc, name, 'ref_reads_count_dict.txt')
            out_dir = os.path.join(out_loc, name, 'filtered_pe')

            if os.path.isfile(read_count_path):
                os.remove(read_count_path)

            params = [filter_bin, '-r', args.r, '-q1', q1, '-q2', q2, '-o', os.path.join(out_loc, name),
                      '-kf', str(args.kf), '-s', str(args.step_size), '-gr', '-subdir', 'filtered_pe',
                      '-m', '4', '-lb', '-lkd', kmer_dict_path]

            if args.max_reads > 0:
                params.extend(['-m_reads', str(args.max_reads)])

            subprocess.run(params)

            if not os.path.isfile(read_count_path):
                raise RuntimeError('Filter failed')

            if not do_refilter and os.path.isdir(out_dir):
                merge_dir = os.path.join(out_loc, name, 'filtered')
                sample_ext = get_sample_ext(q1)

                os.makedirs(merge_dir, exist_ok=True)

                genes = set()

                with open(read_count_path, 'r') as f:
                    for line in f:
                        line = line.strip()

                        if not line:
                            continue

                        genes.add(line.split(',')[0])

                for gene in genes:
                    read_1 = os.path.join(out_dir, f'{gene}_1{sample_ext}')
                    read_2 = os.path.join(out_dir, f'{gene}_2{sample_ext}')

                    if not os.path.isfile(read_1):
                        continue

                    with open(os.path.join(merge_dir, gene + sample_ext), 'wb') as f:
                        with open(read_1, 'rb') as r:
                            shutil.copyfileobj(r, f)

                        if not os.path.isfile(read_2):
                            continue

                        with open(read_2, 'rb') as r:
                            shutil.copyfileobj(r, f)

    else:
        run_filter = ignore_hook

    if do_refilter:
        refilter_bin = find_executable('main_refilter_new', internal=True)

        def run_refilter(name, thr=1):
            in_dir  = os.path.join(out_loc, name, 'filtered_pe')
            out_dir = os.path.join(out_loc, name, 'filtered')

            if not os.path.isdir(in_dir):
                raise RuntimeError('No successful filter run, cannot re-filter')

            params = [refilter_bin, '-r', args.r, '-qd', in_dir, '-o', out_dir, '-kf', str(args.kf),
                      '-p', str(thr), '--log-file', os.path.join(out_loc, name, 'log.txt'),
                      '--min-depth', str(args.min_depth), '--max-depth', str(args.max_depth),
                      '--max-size', str(args.max_size)]

            subprocess.run(params)

            if do_filter and os.path.isdir(in_dir) and os.path.isdir(out_dir):
                shutil.rmtree(in_dir, ignore_errors=True)

    else:
        run_refilter = ignore_hook

    if do_assemble:
        assembler_bin = find_executable('main_assembler', internal=True)

        def run_assembler(name, thr=1):
            in_dir = os.path.join(out_loc, name, 'filtered')
            out_dir = os.path.join(out_loc, name, 'results')
            result_path = os.path.join(out_loc, name, 'result_dict.txt')

            if not os.path.isdir(in_dir):
                raise RuntimeError('No successful filter run, cannot assemble')

            soft_boundary = '0'

            if args.soft_boundary == 'auto':
                soft_boundary = '-1'
            elif args.soft_boundary == 'unlimited':
                soft_boundary = '10000'

            if os.path.isdir(out_dir):
                shutil.rmtree(out_dir, ignore_errors=True)

            params = [assembler_bin, '-r', args.r, '-o', os.path.join(out_loc, name), '-ka', str(args.ka),
                      '-k_min', str(args.min_ka), '-k_max', str(args.max_ka), '-limit_count', str(args.error_threshold),
                      '-iteration', str(args.iteration), '-sb', soft_boundary, '-p', str(thr)]

            subprocess.run(params)

            if not os.path.isfile(result_path):
                raise RuntimeError('Assembly failed')

    else:
        run_assembler = ignore_hook

    if args.p > 1:
        avail_cpu = args.p
        asm_thr   = max(min(args.p // 2, 6), 2)
        filt_thr  = 1 if args.p < 4 else 2

        def calc_task_thr():
            min_thr = min(asm_thr, filt_thr) if filter_list else asm_thr
            return avail_cpu if avail_cpu - asm_thr < min_thr else asm_thr

        filter_list   = []
        refilter_list = []
        assemble_list = []

        executor      = ThreadPoolExecutor(max_workers=math.ceil(avail_cpu / filt_thr))
        running_tasks = {}
        task_metadata = {} # (stage, threads)

        if do_filter:
            filter_list.extend(reversed(samples.keys()))
        elif do_refilter:
            refilter_list.extend(reversed(samples.keys()))
        elif do_assemble:
            assemble_list.extend(reversed(samples.keys()))

        while True:
            while refilter_list and avail_cpu >= asm_thr:
                sample = refilter_list.pop()
                task_thr = calc_task_thr()
                avail_cpu -= task_thr
                running_tasks[sample] = executor.submit(run_refilter, sample, thr=task_thr)
                task_metadata[sample] = (2, task_thr)

            while assemble_list and avail_cpu >= asm_thr:
                sample = assemble_list.pop()
                task_thr = calc_task_thr()
                avail_cpu -= task_thr
                running_tasks[sample] = executor.submit(run_assembler, sample, thr=task_thr)
                task_metadata[sample] = (3, task_thr)

            while filter_list and avail_cpu >= filt_thr:
                sample = filter_list.pop()
                avail_cpu -= filt_thr
                running_tasks[sample] = executor.submit(run_filter, sample)
                task_metadata[sample] = (1, filt_thr)

            if not running_tasks:
                break

            wait(running_tasks.values(), return_when=FIRST_COMPLETED)

            processed_samples = set()

            for sample, task in running_tasks.items():
                if not task.done():
                    continue

                processed_samples.add(sample)

                stage, thr_cnt = task_metadata[sample]
                avail_cpu += thr_cnt

                try:
                    task.result()
                except Exception as e:
                    print(f'An error occurred while processing {sample}: {e}')
                    continue

                if stage == 1:
                    if do_refilter:
                        refilter_list.append(sample)
                    elif do_assemble:
                        assemble_list.append(sample)
                elif stage == 2 and do_assemble:
                    assemble_list.append(sample)

            for sample in processed_samples:
                del running_tasks[sample]
                del task_metadata[sample]

    else:
        for name in samples.keys():
            try:
                if do_filter:
                    run_filter(name)
                if do_refilter:
                    run_refilter(name)
                if do_assemble:
                    run_assembler(name)
            except Exception as e:
                print(f'An error occurred while processing {sample}: {e}')
                continue

def generate_consensus(args, samples):
    out_loc = args.o.strip()

    consensus_bin = find_executable('build_consensus', internal=True)
    minimap2_bin = find_executable('minimap2')

    if args.consensus_threshold <= 0 or args.consensus_threshold > 1:
        raise RuntimeError(f"Invalid consensus threshold {args.consensus_threshold} (must be between 0.0 and 1.0)")

    genes = set()

    with os.scandir(args.r) as it:
        for entry in it:
            if not entry.is_file():
                continue

            name, ext = os.path.splitext(entry.name)

            if ext == '.fasta':
                genes.add(name)

    def iterate_gene(sample):
        in_dir = os.path.join(out_loc, sample, 'results')

        if not os.path.isdir(in_dir):
            print(f'Error: Sample {sample} has no assembled genes, cannot generate consensus')
            return

        cns_dir  = os.path.join(out_loc, sample, 'consensus')
        filt_dir = os.path.join(out_loc, sample, 'filtered')

        os.makedirs(cns_dir, exist_ok=True)

        for name in genes:
            asm_path = os.path.join(in_dir, name + '.fasta')
            read_path = os.path.join(filt_dir, name + get_sample_ext(samples[sample][0]))

            if os.path.isfile(asm_path) and os.path.isfile(read_path):
                sam_path = os.path.join(cns_dir, name + '.sam')
                yield (name, asm_path, read_path, sam_path)

    def process_gene(task):
        gene, asm_path, read_path, sam_path = task

        subprocess.run([minimap2_bin, '-ax', 'sr', '-t', '1', '--sam-hit-only',
                        '-o', sam_path, asm_path, read_path])

        if os.path.isfile(sam_path):
            subprocess.run([consensus_bin, '-i', sam_path, '-c', str(args.consensus_threshold),
                            '-o', os.path.dirname(sam_path), '-s', '0'])

            os.remove(sam_path)

    if args.p > 1:
        executor = ThreadPoolExecutor(max_workers=args.p)

        for _ in executor.map(process_gene, (task
                                             for sample in samples.keys()
                                             for task in iterate_gene(sample))):
            pass

    else:
        for sample in samples.keys():
            for task in iterate_gene(sample):
                process_gene(task)

def blast_trim(args, samples):
    out_loc = args.o.strip()

    blastn_bin = find_executable('blastn')
    makeblastdb_bin = find_executable('makeblastdb')

    if args.trim_retention < 0 or args.trim_retention > 1:
        raise RuntimeError(f"Invalid trim retention threshold {args.trim_retention} (must be between 0.0 and 1.0)")

    if args.trim_mode == 'all':
        mode = 0
    elif args.trim_mode == 'longest':
        mode = 1
    else:
        mode = 2

    gene_len = {}

    with os.scandir(args.r) as it:
        for entry in it:
            if not entry.is_file():
                continue

            name, ext = os.path.splitext(entry.name)

            if ext != '.fasta':
                continue

            with open(os.path.join(args.r, entry.name), 'r') as f:
                sequence_lengths = [len(seq) for _, seq in SimpleFastaParser(f)]

            if not sequence_lengths:
                continue

            gene_len[name] = statistics.median(sequence_lengths)

    def build_blast_db(gene):
        ref_path = os.path.join(args.r, gene + '.fasta')
        db_path = os.path.join(out_loc, 'blast_db', gene)
        subprocess.run([makeblastdb_bin, "-in", ref_path, "-dbtype", "nucl", "-out", db_path])

    def iterate_gene(sample):
        if args.trim_source == 'consensus':
            in_dir = os.path.join(out_loc, sample, 'consensus')
        else:
            in_dir = os.path.join(out_loc, sample, 'results')

        if not os.path.isdir(in_dir):
            print(f'Error: Sample {sample} has no {args.trim_source} sequences, cannot trim')
            return

        blast_dir = os.path.join(out_loc, sample, 'blast')

        os.makedirs(blast_dir, exist_ok=True)

        for name, length in gene_len.items():
            asm_path = os.path.join(in_dir, name + '.fasta')

            if os.path.isfile(asm_path):
                yield (name, length, asm_path, os.path.join(blast_dir, name + '.fasta'))

    def process_gene(task):
        name, length, asm_path, out_path = task

        with open(asm_path, 'r') as f:
            header = next(f)
            sequence = f.read().replace('\n', '')

        blast_proc = subprocess.Popen([blastn_bin, "-query", asm_path, "-db", os.path.join(out_loc, 'blast_db', name),
                                       "-outfmt", "6", "-evalue", "10"],
                                       bufsize=1, errors='replace', stdout=subprocess.PIPE, text=True)

        fragments = []

        for line in blast_proc.stdout:
            parts = line.split("\t")

            if len(parts) < 12:
                continue

            if int(parts[6]) < int(parts[7]):
                fragments.append([int(parts[6]), int(parts[7])])

        blast_proc.wait()

        if not fragments:
            return

        fragments = sorted(fragments, key=lambda fragment: abs(fragment[0] - fragment[1]), reverse=True)

        if mode != 1:
            fragments = build_trimed.merge_fragments(fragments)

        if os.path.exists(out_path):
            os.remove(out_path)

        if mode == 1:
            fragments = fragments[:1]
        elif mode == 2:
            fragments = [[min(t[0] for t in fragments), max(t[1] for t in fragments)]]

        combined_sequence = build_trimed.extract_and_combine_fragments(sequence, fragments)

        if len(combined_sequence) / length > args.trim_retention:
            with open(out_path, 'w') as f:
                f.writelines([header, combined_sequence + '\n'])

    if args.p > 1:
        executor = ThreadPoolExecutor(max_workers=args.p)

        for _ in executor.map(build_blast_db, gene_len.keys()):
            pass

        for _ in executor.map(process_gene, (task
                                             for sample in samples.keys()
                                             for task in iterate_gene(sample))):
            pass

    else:
        for gene in gene_len.keys():
            build_blast_db(gene)
        for sample in samples.keys():
            for task in iterate_gene(sample):
                process_gene(task)

def combine_genes(args, samples):
    out_loc = args.o.strip()

    if not args.no_alignment:
        if args.msa_program == 'clustalo':
            msa_bin = find_executable('clustalo')
        elif args.msa_program == 'muscle':
            msa_bin = find_executable('muscle')
        else:
            msa_bin = find_executable('mafft')

        trimal_bin = find_executable('trimal')
        merge_seq_bin = find_executable('merge_seq', internal=True)

        if args.clean_difference < 0 or args.clean_difference > 1:
            raise RuntimeError(f"Invalid maximum difference {args.clean_difference} (must be between 0.0 and 1.0)")

        if args.clean_sequences < 0 or args.clean_sequences > len(samples):
            raise RuntimeError(f"Invalid required number of sequences {args.clean_sequences} (must be between 0 and {len(samples)})")

    genes = set()

    with os.scandir(args.r) as it:
        for entry in it:
            if not entry.is_file():
                continue

            name, ext = os.path.splitext(entry.name)

            if ext == '.fasta':
                genes.add(name)

    combine_dir = os.path.join(out_loc, 'combined_results')

    os.makedirs(combine_dir, exist_ok=True)

    if not args.no_alignment:
        alignment_dir = os.path.join(out_loc, 'combined_results', 'aligned')
        trim_dir = os.path.join(out_loc, 'combined_trimed')

        os.makedirs(alignment_dir, exist_ok=True)
        os.makedirs(trim_dir, exist_ok=True)

    if args.combine_source == 'trimmed':
        in_name = 'blast'
    elif args.combine_source == 'consensus':
        in_name = 'consensus'
    else:
        in_name = 'results'

    def merge_gene(gene):
        with open(os.path.join(combine_dir, gene + '.fasta'), 'wb') as f:
            for name in sorted(samples.keys()):
                in_path = os.path.join(out_loc, name, in_name, gene + '.fasta')

                if not os.path.isfile(in_path):
                    continue

                with open(in_path, 'rb') as r:
                    shutil.copyfileobj(r, f)

    def align_gene(gene):
        in_path = os.path.join(combine_dir, gene + '.fasta')
        out_path = os.path.join(alignment_dir, gene + '.fasta')

        if not os.path.isfile(in_path):
            return

        if args.msa_program == 'clustalo':
            subprocess.run([msa_bin, '-i', in_path, '-o', out_path, '--auto', '--force',
                            '--seqtype=DNA', '--threads=1'])
        elif args.msa_program == 'muscle':
            subprocess.run([msa_bin, '-align', in_path, '-output', out_path,
                            '-nt', '-threads', '1'])
        else:
            subprocess.run(f'{shlex.quote(msa_bin)} --auto --quiet --nuc --thread 1 '
                           f'{shlex.quote(in_path)} > {shlex.quote(out_path)}', shell=True)

    def clean_gene(gene):
        gene_path = os.path.join(alignment_dir, gene + '.fasta')

        if not os.path.isfile(gene_path):
            return

        with open(gene_path, 'r') as f:
            seq_list = list(SimpleFastaParser(f))

        seq_count = len(seq_list)

        if seq_count <= 1:
            return

        dist_mat = [[0] * seq_count] * seq_count

        for i in range(seq_count - 1):
            for j in range(i + 1, seq_count):
                seq_i = seq_list[i][1].upper()
                seq_j = seq_list[j][1].upper()

                diff_count = sum(ci != cj for ci, cj in zip(seq_i, seq_j) if ci not in '-?' and cj not in '-?')

                nuc_i = seq_i.replace("-", "").replace("?", "")
                nuc_j = seq_j.replace("-", "").replace("?", "")

                if not nuc_i or not nuc_j:
                    dist_mat[i][j] = 1
                    dist_mat[j][i] = 1
                    continue

                diff_pct = diff_count / min(len(nuc_i), len(nuc_j))
                dist_mat[i][j] = diff_pct
                dist_mat[j][i] = diff_pct

        max_count  = 0
        max_subset = []

        for i in range(seq_count):
            subset = [i]

            for j in range(seq_count):
                if i == j:
                    continue

                for k in subset:
                    if dist_mat[j][k] > args.clean_difference:
                        break
                else:
                    subset.append(j)

            if len(subset) > max_count:
                max_count = len(subset)
                max_subset = subset

        max_subset = frozenset(max_subset)

        if max_count >= args.clean_sequences:
            with open(gene_path, 'w') as f:
                f.writelines(f'>{name}\n{seq}\n' for i, (name, seq) in enumerate(seq_list) if i in max_subset)
        else:
            os.remove(gene_path)

    def trim_gene(gene):
        in_path = os.path.join(alignment_dir, gene + '.fasta')
        out_path = os.path.join(trim_dir, gene + '.fasta')

        if not os.path.isfile(in_path):
            return

        subprocess.run([trimal_bin, '-in', in_path, '-out', out_path, '-automated1'])

    if args.p > 1:
        executor = ThreadPoolExecutor(max_workers=args.p)

        for _ in executor.map(merge_gene, genes):
            pass

        if not args.no_alignment:
            for _ in executor.map(align_gene, genes):
                pass

            for _ in executor.map(clean_gene, genes):
                pass

            for _ in executor.map(trim_gene, genes):
                pass

    else:
        for gene in genes:
            merge_gene(gene)

            if not args.no_alignment:
                align_gene(gene)
                clean_gene(gene)
                trim_gene(gene)

    if not args.no_alignment:
        subprocess.run([merge_seq_bin, '-input', alignment_dir, '-exts', '.fasta', '-missing', '-',
                        '-output', os.path.join(out_loc, 'combined_results.fasta')])

        subprocess.run([merge_seq_bin, '-input', trim_dir, '-exts', '.fasta', '-missing', '-',
                        '-output', os.path.join(out_loc, 'combined_trimed.fasta')])

def execute_tasks(args, samples):
    if not os.path.isdir(args.r):
        print(f"Reference directory '{args.r}' does not exist")
        return

    commands = frozenset(args.command)

    do_filter = 'filter' in commands
    do_refilter = 'refilter' in commands
    do_assemble = 'assemble' in commands
    do_consensus = 'consensus' in commands
    do_trim = 'trim' in commands
    do_combine = 'combine' in commands

    try:
        if do_filter or do_refilter or do_assemble:
            do_filter_assemble(args, samples, do_filter, do_refilter, do_assemble)

        if do_consensus:
            generate_consensus(args, samples)

        if do_trim:
            if not args.trim_source:
                args.trim_source = 'consensus' if do_consensus else 'assembly'

            blast_trim(args, samples)

        if do_combine:
            if not args.combine_source:
                if do_trim:
                    args.combine_source = 'trimmed'
                elif do_consensus:
                    args.combine_source = 'consensus'
                else:
                    args.combine_source = 'assembly'

            combine_genes(args, samples)

    except RuntimeError as e:
        print(f'Error: {e}')
        return

if __name__ == '__main__':
    parser = argparse.ArgumentParser(formatter_class=argparse.RawTextHelpFormatter,
                                     description='GeneMiner2 is a tool for extracting phylogenetic marker genes.')
    parser.add_argument('command',
                        choices=('filter', 'refilter', 'assemble', 'consensus', 'trim', 'combine', []),
                        help='One or several of the following actions, separated by space:' + COMMAND_HELP,
                        metavar='command',
                        nargs='*')

    parser.add_argument('-f', help='Sample list file', metavar='FILE', required=True)
    parser.add_argument('-r', help='Reference directory', metavar='DIR', required=True)
    parser.add_argument('-o', help='Output directory', metavar='DIR', required=True)
    parser.add_argument('-p', default=1, help='Number of parallel processes', metavar='INT', type=int)

    parser.add_argument('-kf', default=31, help='Filter k-mer size', metavar='INT', type=int)
    parser.add_argument('-ka', default=0, help='Assembly k-mer size (default = auto)', metavar='INT', type=int)
    parser.add_argument('-s', '--step-size', default=4, help='Filter step size', metavar='INT', type=int)
    parser.add_argument('-e', '--error-threshold', default=2, help='Error threshold', metavar='INT', type=int)
    parser.add_argument('-sb', '--soft-boundary', choices=('0', 'auto', 'unlimited'), default='auto', help='Soft boundary (default = auto)', type=str)
    parser.add_argument('-i', '--iteration', default=4096, help='Search depth', metavar='INT', type=int)
    parser.add_argument('-c', '--consensus-threshold', default='0.75', help='Consensus threshold (default = 0.75)', metavar='FLOAT', type=float)
    parser.add_argument('-m', '--trim-mode', choices=('all', 'longest', 'terminal'), default='terminal', help='Trim mode (default = terminal)', type=str)
    parser.add_argument('-n', '--trim-retention', default=0, help='Retention length threshold (default = 0.0)', metavar='FLOAT', type=float)
    parser.add_argument('-t', '--trim-source', choices=('assembly', 'consensus'), default=None, help='Whether to trim the primary assembly or the consensus sequence (default = output of last step, assembly if no other command given)')
    parser.add_argument('-x', '--combine-source', choices=('assembly', 'consensus', 'trimmed'), default=None, help='Whether to combine the primary assembly, the consensus sequences or the trimmed sequences (default = output of last step, assembly if no other command given)')
    parser.add_argument('-d', '--clean-difference', default=1, help='Maximum acceptable pairwise difference in an alignment (default = 1.0)', metavar='FLOAT', type=float)
    parser.add_argument('-q', '--clean-sequences', default=0, help='Number of sequences required in an alignment (default = 0)', metavar='INT', type=int)

    parser.add_argument('--max-reads', default=0, help='Maximum reads per file', metavar='INT', type=int)
    parser.add_argument('--min-depth', default=50, help='Minimum acceptable depth during re-filtering', metavar='INT', type=int)
    parser.add_argument('--max-depth', default=768, help='Maximum acceptable depth during re-filtering', metavar='INT', type=int)
    parser.add_argument('--max-size', default=6, help='Maximum file size during re-filtering', metavar='INT', type=int)
    parser.add_argument('--min-ka', default=21, help='Minimum auto-estimated assembly k-mer size', metavar='INT', type=int)
    parser.add_argument('--max-ka', default=51, help='Maximum auto-estimated assembly k-mer size', metavar='INT', type=int)
    parser.add_argument('--msa-program', choices=('clustalo', 'mafft', 'muscle'), default='mafft', help='Program for multiple sequence alignment)', type=str)
    parser.add_argument('--no-alignment', action='store_true', default=False, help='Do not perform multiple sequence alignment')

    args = parser.parse_args()
    args.command = args.command or ('filter', 'refilter', 'assemble', 'trim', 'combine')

    samples = prepare_workdir(args)

    if samples:
        print(f'Running tasks: {", ".join(args.command)}')
        print()
        execute_tasks(args, samples)
    else:
        print('Sample list is empty or invalid, exiting')
