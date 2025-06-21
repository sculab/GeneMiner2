import os
import re
import argparse
import subprocess
import shutil

def main():
    parser = argparse.ArgumentParser(description="基于Blast筛选Novoplasty产生的几个option中哪个是最佳序列")
    parser.add_argument("-i", "--input", required=False, default=r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\results\6_Bupleurum_hamiltonii_HQ_R\NOVOPlasty", help="选项文件夹的路径")
    parser.add_argument("-r", "--ref", required=False, default=r"Bupleurum_scorzonerifolium#MT534601.fasta", help="参考序列的路径")
    parser.add_argument("-o", "--output", required=False, default=r"best.fasta", help="结果文件的路径")

    args = parser.parse_args()

    folder_path = args.input
    pattern = re.compile(r'^Option.*\.fasta$')
    fasta_files = [file for file in os.listdir(folder_path) if pattern.match(file)]
    subject_file = args.ref
    output_db = "blast_db"  # 指定数据库名称
    output_file = "blast_output.txt"

    env = os.environ.copy()
    env['BLAST_USAGE_REPORT'] = '0'
    env['DO_NOT_TRACK'] = '1'

    # 如果数据库文件不存在，就创建
    if not os.path.exists(output_db + ".nhr"):
        makeblastdb_cmd = [r"..\analysis\makeblastdb.exe", "-in", f'"{subject_file}"', "-dbtype", "nucl", "-out", output_db]
        print(" ".join(makeblastdb_cmd))
        subprocess.run(makeblastdb_cmd, check=True, env=env)

    longest_total_length = 0
    longest_query_file = ""

    for query_file in fasta_files:
        query_file = os.path.join(folder_path, query_file)
        
        blastn_cmd = [r"..\analysis\blastn.exe", "-query", query_file, "-db", output_db, "-out", output_file, "-outfmt", "6", "-evalue", "10"]
        subprocess.run(blastn_cmd, check=True, env=env)
        # print(query_file)
        
        # 解析并获取10个最长片段的起始位置和总长度
        top_fragments = get_top_fragments(output_file, 1000)
        total_length = 0

        for fragment in top_fragments:
            fields = fragment.split("\t")
            query_start = int(fields[6])
            query_end = int(fields[7])
            fragment_length = query_end - query_start
            total_length += fragment_length
            print(f"片段起始位置: {query_start}, 片段结束位置: {query_end}, 片段长度: {fragment_length}")

        # 如果当前文件的总长度最长，则更新最长文件和总长度
        if total_length > longest_total_length:
            longest_total_length = total_length
            longest_query_file = query_file

    # 复制总长度最长的文件到输出文件
    if longest_query_file:
        print(longest_query_file)
        shutil.copy(longest_query_file, os.path.join(folder_path,args.output))

def get_top_fragments(blast_output_file, num_top):
    fragments = []
    with open(blast_output_file, "r") as blast_output:
        lines = blast_output.readlines()
        lines.sort(key=lambda x: int(x.strip().split("\t")[3]), reverse=True)  # 根据片段长度降序排序
        top_lines = lines[:num_top]
        for line in top_lines:
            fragments.append(line.strip())
    return fragments

if __name__ == "__main__":
    main()
