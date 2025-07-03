import argparse
import os
import sys
# import multiprocessing
from Bio import SeqIO
from Bio.SeqRecord import SeqRecord
from Bio.Seq import Seq
import random
import shutil
# from multiprocessing import Manager, Pool
import subprocess

def generate_bootstrap_sample(records, sequence_length):
    num_sequences = len(records)
    random_indices = random.sample(range(sequence_length), sequence_length)
    bootstrap_sample = []

    for _ in range(num_sequences):
        bootstrap_seq = ''.join([records[_].seq[i] for i in random_indices])
        bootstrap_record = SeqRecord(Seq(bootstrap_seq), id=records[_].id, description=records[_].description)
        bootstrap_sample.append(bootstrap_record)

    return bootstrap_sample

def save_bootstrap_sample(bootstrap_sample, output_file):
    with open(output_file, "w") as output_handle:
        SeqIO.write(bootstrap_sample, output_handle, "fasta")

def process_bootstrap_sample(input_file, bootstrap_output_dir, sequence_length, i, bootstrap_count, num_bootstraps, model, lock):
    with lock:
        bootstrap_count.value += 1
        print(f"Bootstrapping: {bootstrap_count.value}/{num_bootstraps}", end='\r')
    records = list(SeqIO.parse(input_file, "fasta"))
    bootstrap_sample = generate_bootstrap_sample(records, sequence_length)
    output_file = os.path.join(bootstrap_output_dir, f"bootstrap_{i + 1}.fas")
    save_bootstrap_sample(bootstrap_sample, output_file)
    fasttree_warpper(output_file,  os.path.join(bootstrap_output_dir, f"bootstrap_{i + 1}.tree"), model)
    os.remove(output_file)

def main(input_file, output_file, bootstrap_output_dir, model, outgroup, num_bootstraps, num_processes, program):
    records = list(SeqIO.parse(input_file, "fasta"))
    # os.makedirs(bootstrap_output_dir, exist_ok=True)
    sequence_length = len(records[0].seq)
    print("Building Tree...")
    if os.path.exists(output_file):
        os.remove(output_file)
    if program == 'iqtree':
        cmd = [r"..\analysis\iqtree3.exe", "-s", input_file, '-redo']

        if num_bootstraps:
            cmd.extend(['-B', str(max(num_bootstraps, 1000))])

        if num_processes > 1:
            cmd.extend(['-T', 'AUTO', '-ntmax', str(num_processes)])

        subprocess.run(cmd, check=True)
        shutil.move(input_file + ".treefile", output_file)
    else:
        subprocess.run([r"..\analysis\FastTree.exe", "-out", output_file, "-" + model, "-boot", str(num_bootstraps), "-nt", input_file], check=True)
    if os.path.exists(outgroup):
        cmd = r'..\analysis\newick.exe -rootbyoutgroup "'+output_file+ '" -labels "'+outgroup+'" -output "' + output_file +'"'
        subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    cmd = r'..\analysis\newick.exe -ladderize "' + output_file +'" -output "' + output_file +'"'
    subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    # exit(0)
    # manager = Manager()
    # bootstrap_count = manager.Value('i', 0)
    # lock = manager.Lock()

    # with Pool(num_processes) as pool:
    #     tasks = [(input_file, bootstrap_output_dir, sequence_length, i, bootstrap_count, num_bootstraps, model, lock) for i in range(num_bootstraps)]
    #     pool.starmap(process_bootstrap_sample, tasks)

    # bs_trees = os.path.join(bootstrap_output_dir, "bootstrap.trees")
    # tree_files = [os.path.join(bootstrap_output_dir, f"bootstrap_{i + 1}.tree") for i in range(num_bootstraps)]
    # with open(bs_trees, 'w') as outfile:
    #     for tree_file in tree_files:
    #         with open(tree_file, 'r') as infile:
    #             outfile.write(infile.read() + '\n')
    #         os.remove(tree_file)
    # print("Calculating Bootstrap Values")
    # if os.path.exists(outgroup):
    #     cmd = r'..\analysis\newick.exe -rootbyoutgroup "'+os.path.join(bootstrap_output_dir, "bootstrap.trees")+ '" -labels "'+outgroup+'" -output "' + os.path.join(bootstrap_output_dir, "bootstrap.trees") +'"'
    #     subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    # cmd = r'..\analysis\newick.exe -conf "'+os.path.join(bootstrap_output_dir, "input.tree")+ '" -trees "'+os.path.join(bootstrap_output_dir, "bootstrap.trees")+'" -output "' + output_file +'"'
    # subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    # cmd = r'..\analysis\newick.exe -ladderize "' + output_file +'" -output "' + output_file +'"'
    # subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    # os.remove(os.path.join(bootstrap_output_dir, "input.tree"))
    # os.remove(os.path.join(bootstrap_output_dir, "bootstrap.trees"))
    # shutil.rmtree(bootstrap_output_dir)
    print("DONE!")

if __name__ == "__main__":
    # if sys.platform.startswith('win'):
        # multiprocessing.freeze_support()
    parser = argparse.ArgumentParser(description="Generate Bootstrap Samples from a FASTA file and Build Tree.")
    parser.add_argument("-input", metavar='<str>', type=str, help="Input FASTA file", default=r'..\temp\input.fas')
    parser.add_argument("-output", metavar='<str>', type=str, help="Output TREE file", default=r'..\temp\output.tree')
    parser.add_argument("-bootstrap_output_dir", metavar='<str>', type=str, help="Output directory for Bootstrap samples", default=r'..\temp\output')
    parser.add_argument("-model", metavar='<str>', type=str, help="model of fasttree", default='gtr')
    parser.add_argument("-outgroup", metavar='<str>', type=str, help="outgroup", default=r'..\temp\og.txt')
    parser.add_argument("-num_bootstraps", metavar='<int>', type=int, help="Number of Bootstrap samples to generate", default=1000)
    parser.add_argument("-num_processes", metavar='<int>', type=int, help="Number of processes to use", default=1)
    parser.add_argument("-program", choices=('fasttree', 'iqtree'), default='fasttree')
    args = parser.parse_args()
    main(args.input, args.output, args.bootstrap_output_dir, args.model, args.outgroup, args.num_bootstraps, args.num_processes, args.program)
