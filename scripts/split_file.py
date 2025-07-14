import argparse
from Bio import SeqIO
import os

def parse_args():
    parser = argparse.ArgumentParser(description="Process GenBank or fasta file and save sequences as individual fasta files.")
    parser.add_argument("-i", "--input", required=False, default='sequence_mito.gb', help="Input GenBank or fasta file.")
    parser.add_argument("-o", "--output_dir", required=False, default= './test', help="Output directory.")
    parser.add_argument("-f", "--format", required=False, default='genbank', choices=['genbank', 'fasta'], help="Input file format: 'genbank' or 'fasta'.")
    return parser.parse_args()

def save_fasta(record, output_dir):
    tmp_name = str(record.id).replace('.','_').replace(' ','_').replace('__','_').replace('-', '_').replace(':', '_').replace('|', '_')
    record.description = record.id
    filename = os.path.join(output_dir, f"{tmp_name}.fasta")
    record.seq = record.seq.replace("-", "")  # Remove gaps if present
    with open(filename, "w") as output_handle:
        SeqIO.write(record, output_handle, "fasta-2line")
        
def process_file(input_file, output_dir, file_format):
    with open(input_file, "r") as handle:
        for record in SeqIO.parse(handle, file_format):
            save_fasta(record, output_dir)

def main():
    args = parse_args()
    if not os.path.exists(args.output_dir):
        os.makedirs(args.output_dir)
    process_file(args.input, args.output_dir, args.format)

if __name__ == "__main__":
    main()
