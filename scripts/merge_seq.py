from Bio.SeqIO.FastaIO import SimpleFastaParser
import argparse
import os

def merge_sequences(input_folder, output_file, exts, missingchar):
    file_list = [name
                 for name in os.listdir(input_folder)
                 if os.path.splitext(name)[-1].lower() in exts.split(",")]

    file_list.sort()

    gene_names = []
    gene_sequences = []
    species = set()

    for name in file_list:
        with open(os.path.join(input_folder, name), 'r') as f:
            alignment = {k: v for k, v in SimpleFastaParser(f)}

        gene_names.append(os.path.splitext(name)[0])
        gene_sequences.append(alignment)
        species.update(alignment.keys())

    # Fill in missing sequences
    for alignment in gene_sequences:
        aln_len = len(next(iter(alignment.values())))

        for sp in species:
            if sp not in alignment:
                alignment[sp] = missingchar * aln_len

    def species_sort_key(name):
        try:
            nr, sp = name.split('_', 1)
            return (int(nr), sp)
        except ValueError:
            return (0, name)

    species_list = sorted(species, key=species_sort_key)

    # Write merged sequences to a new FASTA file
    with open(output_file, "w") as f:
        for sp in species_list:
            f.write(f">{sp}\n{''.join(alignment[sp] for alignment in gene_sequences)}\n")

    with open(create_new_filename(output_file), 'w') as f:
        f.write('#nexus\nbegin sets;\n')
        start_pos, end_pos = 0, 0
        for i, alignment in enumerate(gene_sequences):
            aln_len = len(next(iter(alignment.values())))

            if aln_len > 0:
                start_pos = end_pos + 1
                end_pos += aln_len
                f.write(f'charset part{i + 1}_{gene_names[i]} = {start_pos}-{end_pos};\n')

        f.write('end;')


def create_new_filename(original_file_path, new_suffix="_partition.txt"):
    # 分割文件名和扩展名
    file_name_without_extension, _ = os.path.splitext(original_file_path)

    # 创建新文件名
    new_file_name = file_name_without_extension + new_suffix

    return new_file_name
if __name__ == "__main__":
    pars = argparse.ArgumentParser(formatter_class=argparse.RawDescriptionHelpFormatter, description=''' 按照序列名合并多个fasta文件中的所有序列 ''')
    pars.add_argument('-input', metavar='<str>', type=str, help='''input folder.''', required=False, default=r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\results\combined_trimed")
    pars.add_argument('-output', metavar='<str>', type=str, help='''output folder''', required=False, default='merge.fasta')
    pars.add_argument('-exts', metavar='<str>', type=str, help='''file extensions''', required=False, default='.fasta,.fas,.fa')
    pars.add_argument('-missing', metavar='<str>', type=str, help='''character to fill missing sequences''', required=False, default='N')
    args = pars.parse_args()
    input_folder = args.input  # Replace with the folder path containing FASTA files
    output_file = args.output
    exts = args.exts
    missingchar = args.missing
    merge_sequences(input_folder, output_file, exts, missingchar)
    print("Merging completed.")
