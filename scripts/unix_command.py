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
import fix_alignment
import muscle_wrapper

COMMAND_HELP = '''
filter    Reference-based filtering of raw reads
refilter  Refinement of filtered reads
assemble  Gene assembly using wDBG
consensus Consensus generation on heterozygous sites
trim      Flank sequence removal
combine   Gene alignment, concatenation and cleanup
tree      Phylogenetic tree reconstruction
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

def get_ref_genes(ref_dir):
    genes = set()

    with os.scandir(ref_dir) as it:
        for entry in it:
            if not entry.is_file():
                continue

            name_tup = os.path.splitext(entry.name)

            if name_tup[1] in ('.fa', '.fas', '.fasta'):
                genes.add(name_tup)

    return genes

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

        if os.path.isfile(kmer_dict_path):
            os.remove(kmer_dict_path)

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

        executor.shutdown()

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
                print(f'An error occurred while processing {name}: {e}')
                continue

def generate_consensus(args, samples):
    out_loc = args.o.strip()

    consensus_bin = find_executable('build_consensus', internal=True)
    minimap2_bin = find_executable('minimap2')

    if args.consensus_threshold <= 0 or args.consensus_threshold > 1:
        raise RuntimeError(f"Invalid consensus threshold {args.consensus_threshold} (must be between 0.0 and 1.0)")

    genes = get_ref_genes(args.r)

    def iterate_gene(sample):
        in_dir = os.path.join(out_loc, sample, 'results')

        if not os.path.isdir(in_dir):
            print(f'Error: Sample {sample} has no assembled genes, cannot generate consensus')
            return

        cns_dir  = os.path.join(out_loc, sample, 'consensus')
        filt_dir = os.path.join(out_loc, sample, 'filtered')

        if os.path.isdir(cns_dir):
            shutil.rmtree(cns_dir, ignore_errors=True)

        os.makedirs(cns_dir, exist_ok=True)

        for name, ext in genes:
            asm_path = os.path.join(in_dir, name + ext)
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
        with ThreadPoolExecutor(max_workers=args.p) as executor:
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

    makeblastdb_bin = find_executable('makeblastdb')

    if args.trim_mode == 'isoform':
        blast_bin = find_executable('magicblast')
        blast_iter = build_trimed.execute_magicblast
    else:
        blast_bin = find_executable('blastn')
        blast_iter = build_trimed.execute_blastn

    if args.trim_retention < 0 or args.trim_retention > 1:
        raise RuntimeError(f"Invalid trim retention threshold {args.trim_retention} (must be between 0.0 and 1.0)")

    if args.trim_mode == 'longest' or args.trim_mode == 'isoform':
        criterion = 'longest'
    elif args.trim_mode == 'terminal':
        criterion = 'terminal'
    else:
        criterion = 'all'

    genes = get_ref_genes(args.r)

    os.makedirs(os.path.join(out_loc, 'blast_db'), exist_ok=True)

    def build_blast_db(name_tup):
        name, ext = name_tup
        subprocess.run([makeblastdb_bin, "-in", f'"{os.path.realpath(os.path.join(args.r, name + ext))}"',
                        "-dbtype", "nucl", "-out", name],
                       cwd=os.path.join(out_loc, 'blast_db'))

    def iterate_gene(sample):
        if args.trim_source == 'consensus':
            in_dir = os.path.join(out_loc, sample, 'consensus')
        else:
            in_dir = os.path.join(out_loc, sample, 'results')

        if not os.path.isdir(in_dir):
            print(f'Error: Sample {sample} has no {args.trim_source} sequences, cannot trim')
            return

        blast_dir = os.path.join(out_loc, sample, 'blast')

        if os.path.isdir(blast_dir):
            shutil.rmtree(blast_dir, ignore_errors=True)

        os.makedirs(blast_dir, exist_ok=True)

        for name, ext in genes:
            asm_path = os.path.join(in_dir, name + '.fasta')
            ref_path = os.path.join(args.r, name + ext)

            if os.path.isfile(asm_path):
                yield (name, asm_path, ref_path, os.path.join(blast_dir, name + '.fasta'))

    def process_gene(task):
        name, asm_path, ref_path, out_path = task
        blast_output = blast_iter(asm_path, os.path.join(out_loc, 'blast_db', name), executable_path=blast_bin)
        build_trimed.process_file(asm_path, ref_path, blast_output, out_path, args.trim_retention * 100, criterion)

    gene_count = len(genes) * len(samples)
    trimmed_count = 0

    if args.p > 1:
        with ThreadPoolExecutor(max_workers=args.p) as executor:
            for _ in executor.map(build_blast_db, genes):
                pass

            for _ in executor.map(process_gene, (task
                                                for sample in samples.keys()
                                                for task in iterate_gene(sample))):
                trimmed_count += 1

                if trimmed_count >= 2:
                    print(f'{trimmed_count}/{gene_count} genes trimmed\r', end='')

    else:
        for gene_tup in genes:
            build_blast_db(gene_tup)

        for sample in samples.keys():
            for task in iterate_gene(sample):
                process_gene(task)

                trimmed_count += 1

                if trimmed_count >= 2:
                    print(f'{trimmed_count}/{gene_count} genes trimmed\r', end='')

    print('\n')

def combine_genes(args, samples):
    out_loc = args.o.strip()

    if not args.no_alignment:
        if args.msa_program == 'clustalo':
            msa_bin = find_executable('clustalo')
        elif args.msa_program == 'muscle':
            msa_bin = find_executable('muscle')
        else:
            msa_bin = find_executable('mafft')

        if not args.no_trimal:
            trimal_bin = find_executable('trimal')

        merge_seq_bin = find_executable('merge_seq', internal=True)

        if args.clean_difference < 0 or args.clean_difference > 1:
            raise RuntimeError(f"Invalid maximum difference {args.clean_difference} (must be between 0.0 and 1.0)")

        if args.clean_sequences < 0 or args.clean_sequences > len(samples):
            raise RuntimeError(f"Invalid required number of sequences {args.clean_sequences} (must be between 0 and {len(samples)})")

    combine_dir = os.path.join(out_loc, 'combined_results')

    if os.path.isdir(combine_dir):
        shutil.rmtree(combine_dir, ignore_errors=True)

    os.makedirs(combine_dir, exist_ok=True)

    if not args.no_alignment:
        alignment_dir = os.path.join(out_loc, 'combined_results', 'aligned')
        trim_dir = os.path.join(out_loc, 'combined_trimed')

        if os.path.isdir(trim_dir):
            shutil.rmtree(trim_dir, ignore_errors=True)

        os.makedirs(alignment_dir, exist_ok=True)
        os.makedirs(trim_dir, exist_ok=True)

    if args.combine_source == 'trimmed':
        in_name = 'blast'
    elif args.combine_source == 'consensus':
        in_name = 'consensus'
    else:
        in_name = 'results'

    genes = {t[0] for t in get_ref_genes(args.r)}

    def merge_gene(gene):
        out_path = os.path.join(combine_dir, gene + '.fasta')
        written  = False

        with open(out_path, 'w+') as f:
            for name in samples.keys():
                in_path = os.path.join(out_loc, name, in_name, gene + '.fasta')

                if not os.path.isfile(in_path):
                    continue

                with open(in_path, 'r') as r:
                    _, seq = next(SimpleFastaParser(r))
                    f.write(f'>{name}\n{seq}\n')

                written = True

        if not written:
            os.remove(out_path)

    def align_gene(gene):
        in_path = os.path.join(combine_dir, gene + '.fasta')
        out_path = os.path.join(alignment_dir, gene + '.fasta')

        if not os.path.isfile(in_path):
            return

        if args.msa_program == 'clustalo':
            subprocess.run([msa_bin, '-i', in_path, '-o', out_path, '--auto', '--force',
                            '--seqtype=DNA', '--threads=1'], stderr=subprocess.DEVNULL)
        elif args.msa_program == 'muscle':
            subprocess.run([msa_bin, '-align', in_path, '-output', out_path, '-quiet',
                            '-nt', '-threads', '1'], stderr=subprocess.DEVNULL)
            muscle_wrapper.reorder_sequences(in_path, out_path)
        else:
            subprocess.run(f'{shlex.quote(msa_bin)} --auto --quiet --nuc --thread 0 '
                           f'{shlex.quote(in_path)} > {shlex.quote(out_path)}',
                           shell=True, stderr=subprocess.DEVNULL)

    def clean_gene(gene):
        gene_path = os.path.join(alignment_dir, gene + '.fasta')

        if os.path.isfile(gene_path):
            fix_alignment.clean_file(gene_path, args.clean_sequences, args.clean_difference)

    def trim_gene(gene):
        in_path = os.path.join(alignment_dir, gene + '.fasta')
        out_path = os.path.join(trim_dir, gene + '.fasta')

        if os.path.isfile(in_path):
            subprocess.run([trimal_bin, '-in', in_path, '-out', out_path, '-automated1'])

    alignment_count = 0
    gene_count = len(genes)

    if args.p > 1:
        with ThreadPoolExecutor(max_workers=args.p) as executor:
            for _ in executor.map(merge_gene, genes):
                pass

            if not args.no_alignment:
                for _ in executor.map(align_gene, genes):
                    alignment_count += 1

                    if alignment_count >= 2:
                        print(f'{alignment_count}/{gene_count} genes aligned\r', end='')

                for _ in executor.map(clean_gene, genes):
                    pass

                if not args.no_trimal:
                    for _ in executor.map(trim_gene, genes):
                        pass

    else:
        for gene in genes:
            merge_gene(gene)

            if not args.no_alignment:
                align_gene(gene)

                alignment_count += 1

                if alignment_count >= 2:
                    print(f'{alignment_count}/{gene_count} genes aligned\r', end='')

                clean_gene(gene)

                if not args.no_trimal:
                    trim_gene(gene)

    print('\n')

    if not args.no_alignment:
        subprocess.run([merge_seq_bin, '-input', alignment_dir, '-exts', '.fasta', '-missing', '-',
                        '-output', os.path.join(out_loc, 'combined_results.fasta')])

        if not args.no_trimal:
            subprocess.run([merge_seq_bin, '-input', trim_dir, '-exts', '.fasta', '-missing', '-',
                            '-output', os.path.join(out_loc, 'combined_trimed.fasta')])

def build_single_tree(prog_name, prog_bin, in_path, bootstrap=0, quiet=False, threads=1):
    if prog_name == 'raxmlng':
        params = [prog_bin, '--msa', in_path, '--msa-format', 'FASTA',
                  '--model', 'GTR+G', '--redo']

        if bootstrap:
            params.extend(['--all', '--bs-trees', str(bootstrap)])
        else:
            params.append('--search')

        if threads > 1:
            params.extend(['--threads', f'auto{{{threads}}}', '--workers', 'auto'])
        else:
            params.extend(['--threads', '1'])

        subprocess.run(params, stdout=subprocess.DEVNULL if quiet else None)

        return in_path + ".raxml.bestTree"

    elif prog_name == 'iqtree':
        params = [prog_bin, '-s', in_path, '-redo']

        if bootstrap:
            params.extend(['-B', str(bootstrap)])

        if quiet:
            params.append('-quiet')

        if threads > 1:
            params.extend(['-T', 'AUTO', '-ntmax', str(threads)])
        else:
            params.extend(['-T', '1'])

        subprocess.run(params, stdout=subprocess.DEVNULL if quiet else None)

        return in_path + ".treefile"

    elif prog_name == 'veryfasttree':
        params = [prog_bin, '-out', in_path + ".veryfasttree.tre", '-gtr']

        if bootstrap:
            params.extend(['-boot', str(bootstrap)])
        else:
            params.append('-nosupport')

        if quiet:
            params.extend(['-quiet'])

        if threads > 1:
            params.extend(['-threads', str(threads)])

        params.extend(['-nt', in_path])

        subprocess.run(params, stderr=subprocess.DEVNULL if quiet else None)

        return in_path + ".veryfasttree.tre"

    else:
        params = [prog_bin, '-out', in_path + ".fasttree.tre", '-gtr']

        if bootstrap:
            params.extend(['-boot', str(bootstrap)])
        else:
            params.append('-nosupport')

        if quiet:
            params.append('-quiet')

        params.extend(['-nt', in_path])

        subprocess.run(params, stderr=subprocess.DEVNULL if quiet else None)

        return in_path + ".fasttree.tre"

def build_coalescent_tree(args):
    out_loc = args.o.strip()

    if args.phylo_program == 'raxmlng':
        phylo_bin = find_executable('raxml-ng')
    elif args.phylo_program == 'iqtree':
        phylo_bin = find_executable('iqtree')
    elif args.phylo_program == 'veryfasttree':
        phylo_bin = find_executable('VeryFastTree')
    else:
        phylo_bin = find_executable('FastTree')

    astral_bin = find_executable('astral')

    def find_genes(path):
        try:
            with os.scandir(path) as it:
                return {os.path.splitext(entry.name)[0] for entry in it if entry.is_file() and entry.name.endswith('.fasta')}
        except OSError:
            return set()

    if args.no_trimal:
        alignment_dir = os.path.join(out_loc, 'combined_results', 'aligned')
    else:
        alignment_dir = os.path.join(out_loc, 'combined_trimed')

    genes = {t[0] for t in get_ref_genes(args.r)} & find_genes(alignment_dir)
    gene_count = len(genes)

    if not genes:
        raise RuntimeError(f"No gene alignments found under '{alignment_dir}'")

    def make_gene_tree(gene):
        return build_single_tree(args.phylo_program, phylo_bin, os.path.join(alignment_dir, f'{gene}.fasta'), quiet=True)

    tree_files = set()

    if args.p > 1:
        with ThreadPoolExecutor(max_workers=args.p) as executor:
            for tree_path in executor.map(make_gene_tree, genes):
                if os.path.isfile(tree_path):
                    tree_files.add(tree_path)
                    tree_count = len(tree_files)

                    if tree_count >= 2:
                        print(f'{tree_count}/{gene_count} trees built\r', end='')

    else:
        for tree_path in map(make_gene_tree, genes):
            if os.path.isfile(tree_path):
                tree_files.add(tree_path)
                tree_count = len(tree_files)

                if tree_count >= 2:
                    print(f'{tree_count}/{gene_count} trees built\r', end='')

    print('\n')

    coal_trees_path = os.path.join(out_loc, 'combined_genes.trees')
    coal_out_path = os.path.join(out_loc, 'Coalescent.tree')
    written = False

    with open(coal_trees_path, 'w') as f:
        for path in tree_files:
            if os.path.getsize(path) <= 2: # Empty tree
                continue

            with open(path, 'r') as r:
                f.write(next(r))

            written = True

    if not written:
        raise RuntimeError(f"Unable to reconstruct coalescent trees because no gene tree is available")

    subprocess.run([astral_bin, '-i', coal_trees_path, '-o', coal_out_path, '-t', str(args.p)])

def build_concatenation_tree(args):
    out_loc = args.o.strip()

    if args.phylo_program == 'raxmlng':
        phylo_bin = find_executable('raxml-ng')
    elif args.phylo_program == 'iqtree':
        phylo_bin = find_executable('iqtree')
    elif args.phylo_program == 'veryfasttree':
        phylo_bin = find_executable('VeryFastTree')
    else:
        phylo_bin = find_executable('FastTree')

    if args.no_trimal:
        in_path = os.path.join(out_loc, 'combined_results.fasta')
    else:
        in_path = os.path.join(out_loc, 'combined_trimed.fasta')

    if not os.path.isfile(in_path):
        raise RuntimeError(f"Unable to find the concatenated alignment at '{in_path}'")

    out_path = build_single_tree(args.phylo_program, phylo_bin, in_path,
                                 bootstrap=args.bootstrap, threads=args.p)

    if not os.path.isfile(out_path):
        raise RuntimeError(f"Phylogenetic tree reconstruction failed")

    shutil.copyfile(out_path, os.path.join(out_loc, 'Concatenation.tree'))

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
    do_tree = 'tree' in commands

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

        if do_tree:
            if args.tree_method == 'coalescent':
                build_coalescent_tree(args)
            else:
                build_concatenation_tree(args)

    except RuntimeError as e:
        print(f'Error: {e}')
        return

if __name__ == '__main__':
    parser = argparse.ArgumentParser(formatter_class=argparse.RawTextHelpFormatter,
                                     description='GeneMiner2 is a tool for extracting phylogenetic marker genes.')
    parser.add_argument('command',
                        choices=('filter', 'refilter', 'assemble', 'consensus', 'trim', 'combine', 'tree', []),
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

    parser.add_argument('-ts', '--trim-source', choices=('assembly', 'consensus'), default=None, help='Whether to trim the primary assembly or the consensus sequence (default = output of last step, assembly if no other command given)')
    parser.add_argument('-tm', '--trim-mode', choices=('all', 'longest', 'terminal', 'isoform'), default='terminal', help='Trim mode (default = terminal)', type=str)
    parser.add_argument('-tr', '--trim-retention', default=0, help='Retention length threshold (default = 0.0)', metavar='FLOAT', type=float)

    parser.add_argument('-cs', '--combine-source', choices=('assembly', 'consensus', 'trimmed'), default=None, help='Whether to combine the primary assembly, the consensus sequences or the trimmed sequences (default = output of last step, assembly if no other command given)')
    parser.add_argument('-cd', '--clean-difference', default=1, help='Maximum acceptable pairwise difference in an alignment (default = 1.0)', metavar='FLOAT', type=float)
    parser.add_argument('-cn', '--clean-sequences', default=0, help='Number of sequences required in an alignment (default = 0)', metavar='INT', type=int)

    parser.add_argument('-m', '--tree-method', choices=('coalescent', 'concatenation'), default='coalescent', help='Multi-gene tree reconstruction method (default = coalescent)')
    parser.add_argument('-b', '--bootstrap', default=1000, help='Number of bootstrap replicates', metavar='INT', type=int)

    parser.add_argument('--max-reads', default=0, help='Maximum reads per file', metavar='INT', type=int)
    parser.add_argument('--min-depth', default=50, help='Minimum acceptable depth during re-filtering', metavar='INT', type=int)
    parser.add_argument('--max-depth', default=768, help='Maximum acceptable depth during re-filtering', metavar='INT', type=int)
    parser.add_argument('--max-size', default=6, help='Maximum file size during re-filtering', metavar='INT', type=int)
    parser.add_argument('--min-ka', default=21, help='Minimum auto-estimated assembly k-mer size', metavar='INT', type=int)
    parser.add_argument('--max-ka', default=51, help='Maximum auto-estimated assembly k-mer size', metavar='INT', type=int)
    parser.add_argument('--msa-program', choices=('clustalo', 'mafft', 'muscle'), default='mafft', help='Program for multiple sequence alignment', type=str)
    parser.add_argument('--no-alignment', action='store_true', default=False, help='Do not perform multiple sequence alignment')
    parser.add_argument('--no-trimal', action='store_true', default=False, help='Do not run trimAl on alignments')
    parser.add_argument('--phylo-program', choices=('raxmlng', 'iqtree', 'fasttree', 'veryfasttree'), default='fasttree', help='Program for phylogenetic tree reconstruction', type=str)

    args = parser.parse_args()
    args.command = args.command or ('filter', 'refilter', 'assemble', 'trim', 'combine', 'tree')

    samples = prepare_workdir(args)

    if samples:
        print(f'Running tasks: {", ".join(args.command)}')
        print()
        execute_tasks(args, samples)
    else:
        print('Sample list is empty or invalid, exiting')
