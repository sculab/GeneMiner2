import os
import sys
from sys import platform
import re
import argparse
import subprocess
import shutil
from collections import Counter
from Bio import SeqIO
from Bio.Seq import Seq

degenerate_bases = {
    'R': ['A', 'G'],
    'Y': ['C', 'T'],
    'S': ['G', 'C'],
    'W': ['A', 'T'],
    'K': ['G', 'T'],
    'M': ['A', 'C'],
    'B': ['C', 'G', 'T'],
    'D': ['A', 'G', 'T'],
    'H': ['A', 'C', 'T'],
    'V': ['A', 'C', 'G'],
    'N': ['A', 'C', 'G', 'T'],
    'AG': 'R',
    'CT': 'Y',
    'CG': 'S',
    'GT': 'K',
    'AT': 'W',
    'AC': 'M',
    'CGT': 'B',
    'AGT': 'D',
    'ACT': 'H',
    'ACG': 'V',
    'ACGT': 'N'
}

def sort_varieties(variety_dict):
    sorted_varieties = sorted(variety_dict.items(), key=lambda x: x[1], reverse=True)
    return sorted_varieties

def Write_Print(log_path, *log_str, sep = " "):
    """
    记录日志并打印
    """
    line = sep.join(map(str,log_str)).strip()
    with open(log_path, 'a') as out:
        out.write(line)
        out.write('\n')
    print(line)

def check_muti_copy(query_file, word_size, folder_name, out_folder, con_thr, level, keep_best = True, mmap = "map-pb"):
    print("Mapping to reference ...")
    try:
        minimap2_cmd = [r"..\analysis\minimap2.exe", "-ax", mmap, '"' + os.path.join(out_folder, folder_name + "_ref.fasta") + '"', query_file if query_file[0] == '"' else '"' + query_file + '"', "-o", '"' + os.path.join(out_folder, folder_name + ".sam") + '"']
        print(" ".join(minimap2_cmd))
        subprocess.run(" ".join(minimap2_cmd), check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        if os.path.exists(os.path.join(out_folder, folder_name + ".sam")) == False:
            if os.path.exists(os.path.join(out_folder, folder_name + "_ref.fasta")): os.remove(os.path.join(out_folder, folder_name + "_ref.fasta"))
            exit(0)
        build_consensus_cmd = [r"..\analysis\build_consensus.exe", "-i", '"' + os.path.join(out_folder, folder_name + ".sam") + '"', "-c", "0.25","-m 2", "-o", '"' + out_folder + '"', "-p", folder_name + '_tmp']
        print(" ".join(build_consensus_cmd))
        subprocess.run(" ".join(build_consensus_cmd), check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

        minimap2_cmd = [r"..\analysis\minimap2.exe", "-ax", mmap, '"' + os.path.join(out_folder, folder_name + "_tmp.fasta") + '"', query_file if query_file[0] == '"' else '"' + query_file + '"', "-o", '"' + os.path.join(out_folder, folder_name + ".sam") + '"']
        print(" ".join(minimap2_cmd))
        subprocess.run(" ".join(minimap2_cmd), check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

        build_consensus_cmd = [r"..\analysis\build_consensus.exe", "-i", '"' + os.path.join(out_folder, folder_name + ".sam") + '"', "-c", con_thr,"-m 2", "-o", '"' + out_folder + '"', "-p", folder_name + '_tmp', "-s 1"]
        print(" ".join(build_consensus_cmd))
        subprocess.run(" ".join(build_consensus_cmd), check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

        print("Making alignment ...")
        #build_mafft_cmd = [r"..\analysis\mafft-win\mafft.bat --adjustdirection --auto", f'"{os.path.join(out_folder, folder_name + "_tmp.fasta")}">"{os.path.join(out_folder, folder_name + ".fasta")}"']
        build_muscle_cmd = [r"..\analysis\muscle_wrapper.exe", "-i", f'"{os.path.join(out_folder, folder_name + "_tmp.fasta")}"', f'-o "{os.path.join(out_folder, folder_name + ".fasta")}"']
        subprocess.run(" ".join(build_muscle_cmd), shell=True, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except:
        if os.path.exists(os.path.join(out_folder, folder_name + "_tmp.fasta")): os.remove(os.path.join(out_folder, folder_name + "_tmp.fasta"))
        if os.path.exists(os.path.join(out_folder, folder_name + ".sam")): os.remove(os.path.join(out_folder, folder_name + ".sam"))
        if os.path.exists(os.path.join(out_folder, folder_name + "_ref.fasta")): os.remove(os.path.join(out_folder, folder_name + "_ref.fasta"))
        if os.path.exists(os.path.join(out_folder, folder_name + ".png")): os.remove(os.path.join(out_folder, folder_name + ".png"))
        Write_Print(os.path.join(out_folder, "log.txt"), 'Error: Could not complete map: ', folder_name)
        exit(0)
    os.remove(os.path.join(out_folder, folder_name + "_tmp.fasta"))
    os.remove(os.path.join(out_folder, folder_name + ".sam"))
    os.remove(os.path.join(out_folder, folder_name + "_ref.fasta"))
    print("Making final results")
    try:
        check_degenerate(os.path.join(out_folder, folder_name + ".fasta"), level, "", keep_best)
    except:
        Write_Print(os.path.join(out_folder, "log.txt"), 'Warning: Could not get best sequence: ', folder_name)



def check_degenerate(input_aligment, level, output_aligment = "", keep_best = True):
    processed_sequences = []
    if output_aligment == "":
        output_aligment = input_aligment
    with open(input_aligment, "r") as fasta_file:
        for record in SeqIO.parse(fasta_file, "fasta"):
            processed_sequences.append(record)
    get_degenerate = False
    for x in processed_sequences[5].seq:
        if str(x).upper() in degenerate_bases:
            get_degenerate = True
            break
    if get_degenerate and processed_sequences[5].seq != processed_sequences[0].seq:
        processed_sequences[6+level].seq = list(processed_sequences[6+level].seq)
        for i, x in enumerate(processed_sequences[6+level].seq):
            if str(x).upper() in degenerate_bases:
                for j in range(1, 11):
                    if processed_sequences[j].seq[i].upper() != processed_sequences[0].seq[i].upper():
                        temp_nuc = processed_sequences[6+level].seq[i]
                        if processed_sequences[j].seq[i].upper() in degenerate_bases:
                            if processed_sequences[0].seq[i].upper() in list(degenerate_bases[processed_sequences[j].seq[i].upper()]):
                                temp_list = list(degenerate_bases[processed_sequences[j].seq[i].upper()])
                                temp_list.remove(processed_sequences[0].seq[i].upper())
                                temp_nuc = temp_list[0] if len(temp_list) == 1 else degenerate_bases[''.join(temp_list)]
                        processed_sequences[6+level].seq[i] = temp_nuc.lower()
                        break
        processed_sequences[0].id = processed_sequences[0].id.split("|")[0][:-4] + "_option_best"
        processed_sequences[0].description = processed_sequences[0].id
        processed_sequences[0].seq = Seq(processed_sequences[0].seq.replace("-","").upper())
        processed_sequences[6+level].seq = Seq(''.join(processed_sequences[6+level].seq).replace("-","").upper())
        processed_sequences[6+level].id = processed_sequences[6+level].id.split("|")[0][:-4] + "_option_alt"
        processed_sequences[6+level].description = processed_sequences[6+level].id
        with open(output_aligment, "w") as output_fasta:
            SeqIO.write([processed_sequences[0], processed_sequences[6+level]], output_fasta, "fasta")       
    elif keep_best:
        with open(output_aligment, "w") as output_fasta:
            processed_sequences[0].id = processed_sequences[0].id.split("|")[0][:-4] + "_option_best"
            processed_sequences[0].description = processed_sequences[0].id
            processed_sequences[0].seq = Seq(processed_sequences[0].seq.replace("-","").upper())
            SeqIO.write(processed_sequences[0], output_fasta, "fasta")
    else:
        os.remove(output_aligment)
        if os.path.exists(output_aligment.replace(".fasta", ".png")): os.remove(output_aligment.replace(".fasta", ".png"))

def main():
    parser = argparse.ArgumentParser(description="基于参考序列对二代或者三代的测序数据构建一致性序列，并且检测多拷贝序列。")
    parser.add_argument("-i", "--input", required=False, default=r"E:\Workshop\alldata\clean_data\Barcode_adapter_BA2_clean.fasta", help="测序数据路径，文件或者文件夹")
    parser.add_argument("-r", "--ref", required=False, default=r"E:\测试数据\HIVTEST\HIV_ref.fasta", help="参考序列的路径")
    parser.add_argument("-o", "--output", required=False, default=r"E:\测试数据\HIV", help="结果文件夹的路径")
    parser.add_argument('-w', "--word_size", required=False, default = "7", help='''执行Blast时的word size''')
    parser.add_argument("-c", "--con_thr", required=False, default="0.25,0.3,0.35,0.4,0.45,0.5,0.55,0.6,0.65,0.7,0.75", help="List of consensus thresold(s) separated by commas, no spaces, example: -c 0.25,0.75,0.50, default=0.25")
    parser.add_argument("-l", "--level", required=False, type=int, default=4, help='对多拷贝的敏感度 1~4 分别对应0.6,0.65,0.7,0.75级别的consensus，越高简并碱基越多')
    parser.add_argument('-p', "--processes", required=False, type=int, default=16, help='Number of processes')
    parser.add_argument('-m', "--mode", required=False, type=int, default=0, help='0: 不进行操作 1: 对二代序列检测多拷贝')
    args = parser.parse_args()
    subject_file = args.ref 
    con_thr = args.con_thr
    level =  args.level
    mode = args.mode
    word_size =  args.word_size
    out_folder = args.output

    if mode == 1:
        check_muti_copy(args.input, word_size, os.path.basename(os.path.splitext(args.input)[0]), out_folder, con_thr, level, False, "sr")

if __name__ == "__main__":
    main()
