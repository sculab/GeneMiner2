import os
from Bio import SeqIO
import statistics
import argparse
import subprocess
import shutil

def main():
    parser = argparse.ArgumentParser(description="基于Blast筛选Novoplasty产生的几个option中哪个是最佳序列")
    parser.add_argument("-i", "--input", required=False, default=r"..\temp\ITS.fasta", help="选项文件夹的路径")
    parser.add_argument("-r", "--ref", required=False, default=r"..\temp\ITSref.fasta", help="参考序列的路径")
    parser.add_argument("-o", "--output", required=False, default=r"..\temp\trimed.fasta", help="结果文件的路径")
    parser.add_argument("-b", "--output_blast", required=False, default=r"..\temp", help="结果文件的路径")
    parser.add_argument("-m", "--mode", metavar='<int>', type=int, required=False, default=2, help="0:使用匹配上的所有片段，1:只使用最长匹配片段，2:只trim两端")
    parser.add_argument("-pec", "--pec", metavar='<int>', type=int, required=False, default=50, help="只保留大于某个值的片段")

    args = parser.parse_args()
    subject_file = args.ref
    query_file = args.input
    output_file = args.output
    output_db = args.output_blast + r"\blast_db"  # 指定数据库名称
    output_blast = args.output_blast + r"\blast_output.txt"

    if os.path.exists(output_db + ".nhr"): os.remove(output_db + ".nhr")
    if os.path.exists(output_db + ".nin"): os.remove(output_db + ".nin")
    if os.path.exists(output_db + ".nsq"): os.remove(output_db + ".nsq")
    if os.path.exists(output_blast): os.remove(output_blast)

    # 用于存储所有序列的长度
    sequence_lengths = []
    # 逐个读取FASTA文件中的序列并获取其长度
    for record in SeqIO.parse(subject_file, "fasta"):
        sequence_lengths.append(len(record.seq))
    # 计算序列长度的中值
    median_length = statistics.median(sequence_lengths)

    makeblastdb_cmd = [r"..\analysis\makeblastdb.exe", "-in", subject_file, "-dbtype", "nucl", "-out", output_db]
    print(" ".join(makeblastdb_cmd))
    subprocess.run(makeblastdb_cmd, check=True)
    blastn_cmd = [r"..\analysis\blastn.exe", "-query", query_file, "-db", output_db, "-out", output_blast, "-outfmt", "6", "-evalue", "10"]
    subprocess.run(blastn_cmd, check=True)
    if os.path.exists(output_blast):
        fragments = get_fragments(output_blast) if args.mode == 1 else merge_fragments(get_fragments(output_blast))
        with open(query_file, 'r') as file:
            header = next(file)
            sequence = file.read().replace('\n', '')
        if len(fragments) >= 1:
            if args.mode == 1: fragments = [fragments[0]]
            if args.mode == 2:
                min_first = min([t[0] for t in fragments])
                max_second = max([t[1] for t in fragments])
                fragments = [[min_first, max_second]]
            combined_sequence = extract_and_combine_fragments(sequence, fragments)
            if len(combined_sequence) / median_length * 100 > args.pec:
                with open(output_file, 'w') as file:
                    file.write(header )
                    file.write(combined_sequence + '\n')
            else:
                if os.path.exists(output_file):
                    shutil.remove(output_file)
        else:
            if os.path.exists(output_file):
                shutil.remove(output_file)


def extract_and_combine_fragments(sequence, fragments):
    """
    Extract and combine fragments from a sequence based on given fragment positions.
    """
    combined_sequence = ''
    for start, end in fragments:
        # Extract the fragment (end is inclusive, hence end + 1)
        fragment = sequence[start-1:end]
        combined_sequence += fragment
    return combined_sequence

def merge_fragments(fragments):
    # Sort fragments by their starting positions
    fragments.sort(key=lambda x: x[0])
    merged_fragments = []
    current_fragment = fragments[0]
    for fragment in fragments[1:]:
        # If there is an overlap or the fragments are adjacent, merge them
        if fragment[0] <= current_fragment[1]:
            current_fragment[1] = max(current_fragment[1], fragment[1])
        else:
            # Add the current merged fragment to the list
            merged_fragments.append(current_fragment)
            current_fragment = fragment
    # Add the last merged fragment to the list
    merged_fragments.append(current_fragment)
    return merged_fragments
 

def get_fragments(blast_output_file):
    fragments = []
    with open(blast_output_file, "r") as blast_output:
        lines = blast_output.readlines()
        for line in lines:
            parts = line.split("\t")
            if int(parts[6]) < int(parts[7]):
                fragments.append([int(parts[6]), int(parts[7])])
    # 自定义排序键函数，计算两个值的差的绝对值
    def sort_key(fragment):
        return abs(fragment[0] - fragment[1])
    # 使用sorted函数进行排序，key参数指定排序的键函数
    sorted_fragments = sorted(fragments, key=sort_key, reverse=True)
    return sorted_fragments

if __name__ == "__main__":
    main()
