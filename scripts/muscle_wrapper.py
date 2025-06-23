import argparse
import subprocess
import os
def muscle5_wrapper(input_file, output_file):
    """
    Muscle 5 Wrapper function.
    
    Args:
        input_file (str): Path to the input file.
        output_file (str): Path to the output file.
    
    Returns:
        int: Return code of the Muscle 5 process.
    """
    muscle5_command = [r"..\analysis\muscle-win64.v5.3.exe", "-align", input_file, "-output", output_file, '-nt', '-threads', '1']
    try:
        subprocess.run(muscle5_command, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        reorder_sequences(input_file, output_file)
    except Exception as e:
        print(f"An error occurred: {e}")

def reorder_sequences(org_fas_file, aln_fas_file):
    # 读取 aln.fas 文件中的序列和名称
    with open(aln_fas_file, 'r') as f:
        aln_fas_lines = f.readlines()
    # 创建一个字典，将序列名映射到序列内容
    aln_fas_dict = {}
    current_seq_name = ''
    current_seq = ''
    for line in aln_fas_lines:
        if line.startswith('>'):
            # 如果是序列名称行，则保存当前序列
            if current_seq_name:
                aln_fas_dict[current_seq_name] = current_seq
            current_seq_name = line.strip()
            current_seq = ''
        else:
            current_seq += line.strip()
    if current_seq_name:
        aln_fas_dict[current_seq_name] = current_seq
    # 读取 org.fas 文件中的序列顺序
    with open(org_fas_file, 'r') as f:
        org_fas_lines = f.readlines()
    # 根据 org.fas 中的顺序重新排列序列，并保存到 aln.fas 文件中
    with open(aln_fas_file, 'w') as f:
        for line in org_fas_lines:
            if line.startswith('>'):
                current_seq_name = line.strip()
                if current_seq_name in aln_fas_dict:
                    f.write(current_seq_name + '\n')
                    f.write(aln_fas_dict[current_seq_name] + '\n')
                current_seq_name = ''

def main():
    parser = argparse.ArgumentParser(description="Muscle 5 Wrapper")
    parser.add_argument("-i", "--input", required=False, help="Path to the input file", default=r'E:\TPS2.fasta')
    parser.add_argument("-o", "--output", required=False, help="Path to the output file", default=r'E:\aln.fasta')
    args = parser.parse_args()
    muscle5_wrapper(args.input, args.output)

if __name__ == "__main__":
    main()
