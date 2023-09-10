#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# @Time    : 2021/10/20 16:48
# @Author  : xiepulin
# @File    : build_reference_database.py
# @Software: PyCharm
import os
import sys
import csv
from Bio import SeqIO
from Bio.Align import MultipleSeqAlignment
from Bio.Seq import Seq
from Bio.SeqRecord import SeqRecord
from collections import defaultdict
from Bio.SeqRecord import SeqRecord
from concurrent.futures import ProcessPoolExecutor
import multiprocessing
import subprocess
import argparse

def str2bool(v):
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Unsupported value encountered.')

def is_exist(file):
    if os.path.isfile(file):
        if os.path.getsize(file) > 0:
            flag = 1
        else:
            flag = 0

    elif os.path.isdir(file):
        files = get_files(file)
        if files == []:
            flag = 0
        else:
            flag = 1
            for i in files:
                if os.path.getsize(i) > 0:
                    continue
                else:
                    flag = 0
                    break
    else:
        flag = 0
    return flag


def get_basename(file):
    extension = (".fasta", ".fas", ".fa", ".fna", ".ffn", ".frn", ".faa")
    if is_exist(file):
        basename = os.path.basename(file)
        stem, suffix = os.path.splitext(basename)
        if suffix:
            if suffix.lower() in extension:
                basename = stem
        else:
            basename = basename
        return basename

def get_files(ref):
    file_path_list = []
    for root, dirs, files in os.walk(ref):
        for file in files:
            file_path = os.path.join(root, file)
            file_path_list.append(file_path)
    return file_path_list

def get_file_list(path, exts):
    file_list = []
    if os.path.isdir(path):
        files = get_files(path)
        for i in files:
            if os.path.splitext(os.path.basename(i))[-1].lower() in exts:
                if os.path.getsize(i) == 0:
                    pass
                else:
                    file_list.append(i)
    elif os.path.isfile(path):
        if os.path.splitext(os.path.basename(path))[-1].lower() in exts:
            size = os.path.getsize(path)
            if size == 0:
                pass
            else:
                file_list.append(path)
    else:
        pass
    return file_list

def reverse_complement_all(seq):
    return seq.translate(str.maketrans('ACGTacgtRYMKrymkVBHDvbhd', 'TGCAtgcaYRKMyrkmBVDHbvdh'))[::-1]

# 函数：计算两个引物之间的序列
def calculate_intermediate_sequence(template_sequence, left_primer_sequence, right_primer_sequence):
    left_index = template_sequence.find(left_primer_sequence)
    right_index = template_sequence.find(reverse_complement_all(right_primer_sequence))
    if left_index == -1 or right_index == -1:
        return ""
    return template_sequence[left_index:right_index + len(right_primer_sequence)]

#
def check_good_primer(result_dict, limit):
    values = [result_dict[_] for _ in result_dict.keys()]
    for i in values:
        if i[0]-i[1] > limit/2:
            return 0
    for i in range(len(values)):
        for j in range(i, len(values)):
            if values[i][1] - values[j][0] > limit or values[j][1] - values[i][0] > limit:
                return 1
    return 0

def split_sequences(input_file, output_dir, sequence_length = 2000, step= 1800):
    with open(input_file, 'r') as f:
        lines = f.readlines()
    if not os.path.isdir(output_dir):
                os.makedirs(output_dir)
    sequences = {}  # 存储每个序列的名称和内容
    current_sequence_name = None

    for line in lines:
        if line.startswith('>'):
            current_sequence_name = line.strip()
            sequences[current_sequence_name] = ''
        else:
            sequences[current_sequence_name] += line.strip()

    num_fragments = (len(sequences[next(iter(sequences))]) - sequence_length) // step + 2

    gene_names = []
    # 分割序列并保存到文件
    
    for i in range(num_fragments):
        start = i * step
        end = start + sequence_length
        output_file = f"{output_dir}/{start+1}-{end}.fasta"
        gene_names.append(f"{start+1}-{end}")
        with open(output_file, 'w') as out_f:
            for sequence_name, sequence_content in sequences.items():
                sequence_name= sequence_name.replace(" ","_").replace(".","_")
                fragment = sequence_content[start:end]
                if fragment:
                    out_f.write(f"{sequence_name}\n{fragment}\n")
    return gene_names


class Extract_reference():
    def __init__(self, configuration_information):
        self.configuration_information = configuration_information  # 包含各级文件名字信息
        self.out_dir = configuration_information["out"]  # 最大一级的输出文件夹
        self.input = configuration_information["input"]  # 参考基因组
        self.soft_boundary = configuration_information["soft_boundary"]  # 软边界
        # 基因最大长度
        self.gene_max_length = configuration_information["max_seq_length"]
        # 基因最小长度
        self.gene_min_length = configuration_information["min_seq_length"]
        # 标记最大长度
        self.marker_max_length = configuration_information["max_marker_length"]
        # 标记最小长度
        self.marker_min_length = configuration_information["min_marker_length"]
        self.gap_length = configuration_information["gap_length"]
        self.thread_number = configuration_information["thread_number"]
        self.max_primer = configuration_information["max_primer"]
        self.usevar = configuration_information["usevar"]
        self.split_only = configuration_information["split_only"]
        self.do_alignment = configuration_information["do_alignment"]

    def add_soft_boundary(self, start, end, start_all, end_all):
        soft_boundary = self.soft_boundary
        gene_min_length = self.gene_min_length
        gene_max_length = self.gene_max_length
        # 允许软边界超界限
        # if (end - start < gene_min_length) and (end - start > gene_max_length):
        #     return (start, end)
        soft_start = start - soft_boundary
        soft_end = end + soft_boundary
        if soft_start <= start_all:  # Out of Left Boundary
            start = start
        else:
            start = soft_start
        if soft_end >= end_all:  # Out of Right Boundary
            end = end
        else:
            end = soft_end
        return (start, end)
    
    def remove_terminal_gap(self, sequence):
        start = 0
        end = len(sequence)
        # 查找序列开头的非 gap 位置
        for i, base in enumerate(sequence):
            if base != '-':
                start = i
                break
        # 查找序列末尾的非 gap 位置
        for i in range(len(sequence) - 1, -1, -1):
            if sequence[i] != '-':
                end = i + 1
                break
        return sequence[start:end]
    

    def generate_consensus(self, seq_record, UseN = True):
        # 将记录中的序列拆分成一个列表
        sequences = [str(record.seq) for record in seq_record]
        # 生成一致序列
        consensus_sequence = ''
        for i in range(len(sequences[0])):
            bases = [seq[i] for seq in sequences]
            consensus_base = max(set(bases), key=bases.count)
            if UseN:
                if len(set(bases)) > 1: consensus_base = "N"
                if "-" in bases: consensus_base = "-"
            consensus_sequence += consensus_base
        return self.remove_terminal_gap(consensus_sequence)

    def write_fasta_file(self, record, files_count, current_file, do_aln = True):
        # 去除全部相同的序列
        if len(set(str(seq.seq) for seq in record)) == 1 and self.split_only == False:
            return
        temp_file_count = {}
        for seq in record:
            seq_name_parts = seq.id.lower().split("#")
            seq_name_key = seq_name_parts[0]
            if seq_name_key in temp_file_count:
                temp_file_count[seq_name_key] += 1
            else:
                temp_file_count[seq_name_key] = 1
        # 如果某物种有某条序列，该物种的所有样本就必须全部有这条序列
        if self.split_only == False:
            for key, value in temp_file_count.items():
                if value < files_count[key]:
                    return

        if len(record) >= 2 or self.split_only:
            sequence_lengths = [len(seq.seq) for seq in record]
            absolute_path = os.path.abspath(current_file)
            data_path = os.path.dirname(absolute_path)
            gene_name = os.path.splitext(os.path.basename(absolute_path))[0]

            # 写入原始序列
            file_name_with_ext = gene_name + '.fasta'
            seq_file = os.path.join(data_path,"org_seq",file_name_with_ext)
            if not os.path.isdir(os.path.join(data_path,"org_seq")):
                os.makedirs(os.path.join(data_path,"org_seq"))
            # 保存两行模式的fasta文件
            SeqIO.write(record, seq_file, "fasta-2line")
            # 保存比对之后的序列
            aln_file = os.path.join(data_path,"alignment",file_name_with_ext)
            
            if do_aln:
                print("Aligning", gene_name, "gene sequences")
                if not os.path.isdir(os.path.join(data_path,"alignment")):
                    os.makedirs(os.path.join(data_path,"alignment"))
                if sys.platform.startswith('win'):
                    muscle_command = ["muscle5.1.win64.exe", "-align", seq_file, "-output", aln_file]
                else:
                    muscle_command = ["./muscle -in '" + seq_file + "' -out '" + aln_file + "'"]
                try:
                    # with open(os.devnull, 'w') as devnull:
                    #     subprocess.run(muscle_command, shell=True, check=True, stdout=devnull, stderr=devnull)
                    subprocess.run(muscle_command, shell=True, check=True)
                except subprocess.CalledProcessError as e:
                    print("Error:", e)
            if self.split_only: return
            alignment_records = list(SeqIO.parse(aln_file, 'fasta'))
            # 生成一致序列
            consensus_seq = self.generate_consensus(alignment_records, self.usevar)
            # 创建一致序列的 SeqRecord 对象
            taxon_count = len(set(temp_file_count.keys())) 
            gap_count = consensus_seq.count("-")
            consensus_name = "|".join(set(temp_file_count.keys()))
            # 将一致序列保存到文件
            data_path = os.path.dirname(absolute_path)
            # 写入consensus
            if not os.path.isdir(os.path.join(data_path,"consensus")):
                os.makedirs(os.path.join(data_path,"consensus"))
            con_file = os.path.join(data_path,"consensus",file_name_with_ext)
            with open(con_file, 'w') as output_handle:
                output_handle.write(">" + consensus_name + "\n")
                output_handle.write(consensus_seq + "\n")
            # 写入input
            file_name_with_ext = gene_name + '.txt'
            if not os.path.isdir(os.path.join(data_path,"primer_input")):
                os.makedirs(os.path.join(data_path,"primer_input"))
            input_file = os.path.join(data_path,"primer_input",file_name_with_ext)
            with open(input_file, 'w') as output_handle:
                output_handle.write("SEQUENCE_ID=" + consensus_name + "\n")
                output_handle.write("SEQUENCE_TEMPLATE=" + consensus_seq.replace("-","") + "\n")
                output_handle.write("=")
            # 写入setting
            if not os.path.isdir(os.path.join(data_path,"setting")):
                os.makedirs(os.path.join(data_path,"setting"))
            setting_file = os.path.join(data_path,"setting",file_name_with_ext)
            with open("setting.tmp", 'r') as input_handle:
                file_content = input_handle.read()
                with open(setting_file, 'w') as output_handle:
                    if self.marker_min_length > int(min(len(consensus_seq.replace("-","")), self.marker_max_length)):
                        return
                    length_str = str(self.marker_min_length) + "-" + str(int(min(len(consensus_seq.replace("-","")), self.marker_max_length)))
                    output_handle.write(file_content.replace("$length$", length_str).replace("$max_primer$", str(self.max_primer)))
            # 记录结果
            with open(os.path.join(data_path,"seq_results.csv"), "a", newline='') as csv_file:
                csv_writer = csv.writer(csv_file)
                csv_writer.writerow([gene_name,consensus_seq.replace("-",""),taxon_count,gap_count,consensus_name,])
            
            # 运行primer3_core
            print("Designing primers based on", gene_name)
            primer_file = os.path.join(data_path,"primer",file_name_with_ext)
            if not os.path.isdir(os.path.join(data_path,"primer")):
                os.makedirs(os.path.join(data_path,"primer"))
            primer_command = ["primer3_core.exe", "--p3_settings_file", setting_file ,"--output", primer_file , input_file]
            try:
                subprocess.run(primer_command, shell=True, check=True)
            except subprocess.CalledProcessError as e:
                print("Error:", e)

            # primer3写入csv
            file_name_with_ext = gene_name + '.csv'
            primer_csv_file = os.path.join(data_path,"primer",file_name_with_ext)
            with open(primer_file, 'r') as file:
                primer3_data = file.read()
            # 将数据按行分割
            data_lines = primer3_data.strip().split('\n')
            # 创建一个字典来保存解析后的数据
            parsed_data = {}
            current_key = ""
            for line in data_lines:
                if '=' in line:
                    key, value = line.split('=', 1)
                    parsed_data[key] = value
                    current_key = key
                else:
                    # 处理多行的数据
                    parsed_data[current_key] += '\n' + line
            # 创建 CSV 文件并写入数据
            with open(primer_csv_file, 'w', newline='') as csv_file:
                csv_writer = csv.writer(csv_file)
                # 写入 CSV 表头
                csv_writer.writerow(["ID", "SEQUENCE_ID", "PRIMER_LEFT_SEQUENCE", "PRIMER_RIGHT_SEQUENCE", "PRIMER_LEFT_TM", "PRIMER_RIGHT_TM", "LENGTH_INTERMEDIATE_SEQUENCE", "REV_RIGHT_SEQUENCE"])
                # 提取每个 PRIMER 的序列和温度，并添加到 CSV 文件中
                primer_rows = []
                num_primers = int(parsed_data["PRIMER_LEFT_NUM_RETURNED"])
                for i in range(num_primers):
                    left_key = "PRIMER_LEFT_{0}_SEQUENCE".format(i)
                    right_key = "PRIMER_RIGHT_{0}_SEQUENCE".format(i)
                    left_tm_key = "PRIMER_LEFT_{0}_TM".format(i)
                    right_tm_key = "PRIMER_RIGHT_{0}_TM".format(i)
                    left_sequence = parsed_data[left_key]
                    right_sequence = parsed_data[right_key]
                    template_sequence = parsed_data["SEQUENCE_TEMPLATE"]
                    intermediate_sequence = calculate_intermediate_sequence(template_sequence, left_sequence, right_sequence)
                    if intermediate_sequence is not None:
                        row_data = [i + 1, parsed_data["SEQUENCE_ID"], left_sequence, right_sequence, parsed_data[left_tm_key], parsed_data[right_tm_key], len(intermediate_sequence), reverse_complement_all(right_sequence)]
                        primer_rows.append(row_data)
                        csv_writer.writerow(row_data)
            # 对原始的序列进行分析
            primer_results_file = os.path.join(data_path,"primer_results", file_name_with_ext)
            if not os.path.isdir(os.path.join(data_path,"primer_results")):
                os.makedirs(os.path.join(data_path,"primer_results"))
            seq_names = []
            for seq in record:
                seq_names.append(seq.id.lower() + "#seq") 
                seq_names.append(seq.id.lower() + "#len")
            with open(primer_results_file, 'w', newline='') as csv_file:
                csv_writer = csv.writer(csv_file)
                csv_writer.writerow(["PRIMER_LEFT_SEQUENCE", "PRIMER_RIGHT_SEQUENCE", "PRIMER_LEFT_TM", "PRIMER_RIGHT_TM"] + list(files_count.keys()) + seq_names)
                for row in primer_rows:
                    intermediate_row = []
                    result_dict = dict.fromkeys(files_count.keys())
                    for _ in result_dict.keys(): result_dict[_] = [0, 1e9]
                    for seq in record:
                        intermediate_sequence = calculate_intermediate_sequence(str(seq.seq).replace("-",""), row[2], row[3])
                        intermediate_row.append(intermediate_sequence)
                        intermediate_row.append(len(intermediate_sequence))
                        tmp_name = seq.id.lower().split("#")[0]
                        result_dict[tmp_name][0] = max(len(intermediate_sequence),result_dict[tmp_name][0])
                        result_dict[tmp_name][1] = min(len(intermediate_sequence),result_dict[tmp_name][1])
                    tmp_result = check_good_primer(result_dict, self.gap_length)
                    csv_writer.writerow(row[2:6] + [result_dict[_] for _ in result_dict.keys()] + intermediate_row + [tmp_result])
                    if tmp_result:
                        for _ in result_dict.keys():
                            result_dict[_] = int(sum(result_dict[_])/2)
                            if result_dict[_] == int(1e9/2):
                                result_dict[_] = "null"
                            if result_dict[_]  == 0:
                                result_dict[_] = "weak"
                        with open(os.path.join(data_path,"primer_results.csv"), "a", newline='') as tmp_csv_file:
                            tmp_csv_writer = csv.writer(tmp_csv_file)
                            tmp_csv_writer.writerow([gene_name, template_sequence]+ row[2:6] + [result_dict[_] for _ in result_dict.keys()])
    
    def extract_reference_from_gb_parallel(self):
        out_dir = self.out_dir
        ref = self.input
        thread_number = self.thread_number
        if self.split_only == False:
            with open(os.path.join(out_dir,"seq_results.csv"), "w", newline='') as csv_file:
                csv_writer = csv.writer(csv_file)
                csv_writer.writerow(["SEQUENCE_NAME", "SEQUENCE_TEMPLATE", "TAXON_COUNT", "GAP_COUNT", "TAXON_LIST"])
        file_names =get_file_list(ref, [".gb",".genbank"])
        task_pool = []
        results = []
        executor = ProcessPoolExecutor(max_workers=thread_number)
        if thread_number > 1:
            for _ in file_names:
                task_pool.append(executor.submit(self.extract_reference_from_gb, _))
            for i in task_pool:
                results.append(i.result())
        else:
            for _ in file_names:
                results.append(self.extract_reference_from_gb(_))
            if not results:
                return 0
        my_records = defaultdict(list)  
        for i in results:
            for key, value in i.items():
                if key not in my_records:
                    my_records[key] = value
                else:
                    my_records[key].extend(value)
        uniform_name_dict = defaultdict(dict)
        Alias2Real_dict = defaultdict()
        real_name_list = []  # ['acc', 't', 'SS']

        for key, value in my_records.items():
            number = len(value)
            new_name = str(key).lower()
            if new_name not in uniform_name_dict:
                uniform_name_dict[new_name] = {key: number}
            else:
                uniform_name_dict[new_name].update({key: number})
        for key, value in uniform_name_dict.items():
            real_name = max(value, key=lambda gene_name: value[gene_name])
            Alias2Real_dict[key] = real_name
            real_name_list.append(real_name)

        my_records_ultimate = defaultdict(list)
        for key, value in my_records.items():
            if key in real_name_list:
                my_records_ultimate[key].extend(value)
            else:
                key_ultimate = Alias2Real_dict[str(key).lower()]
                my_records_ultimate[key_ultimate].extend(value)
        files_count = {}
        for i in [os.path.splitext(os.path.basename(_))[0] for _ in file_names]:
            file_name = i.lower().split("#")[0]
            if file_name in files_count:
                files_count[file_name] += 1
            else:
                files_count[file_name] = 1
        if self.split_only == False:
            with open(os.path.join(out_dir,"primer_results.csv"), "w", newline='') as csv_file:
                csv_writer = csv.writer(csv_file)
                csv_writer.writerow(["SEQUENCE_NAME", "SEQUENCE_TEMPLATE", "PRIMER_LEFT_SEQUENCE", "PRIMER_RIGHT_SEQUENCE", "PRIMER_LEFT_TM", "PRIMER_RIGHT_TM"] + list(files_count.keys()))
        task_pool2 = []
        results2 = []
        executor2 = ProcessPoolExecutor(max_workers=thread_number)
        if thread_number > 1:
            for key, value in my_records_ultimate.items():
                path = os.path.join(out_dir, key + ".fasta")
                if self.split_only:
                    task_pool2.append(executor2.submit(self.write_fasta_file, value, files_count, os.path.join(out_dir, key + ".Fasta"), self.do_alignment))
                else:
                    task_pool2.append(executor2.submit(self.write_fasta_file, value, files_count, os.path.join(out_dir, key + ".Fasta"), True))
            for i in task_pool2:
                results2.append(i.result())
        else:
            for key, value in my_records_ultimate.items():
                path = os.path.join(out_dir, key + ".fasta")
                if self.split_only:
                    results2.append(self.write_fasta_file(value, files_count, os.path.join(out_dir, key + ".Fasta"), self.do_alignment))
                else:
                    results2.append(self.write_fasta_file(value, files_count, os.path.join(out_dir, key + ".Fasta"), True))

    def extract_reference_from_fasta(self):
        out_dir = self.out_dir
        input_file = self.input
        thread_number = self.thread_number
        if not os.path.isdir(out_dir):
            os.makedirs(out_dir)
        if self.split_only == False:
            with open(os.path.join(out_dir,"seq_results.csv"), "w", newline='') as csv_file: 
                csv_writer = csv.writer(csv_file)
                csv_writer.writerow(["SEQUENCE_NAME", "SEQUENCE_TEMPLATE", "TAXON_COUNT", "GAP_COUNT", "TAXON_LIST"])
        
        name_list = []
        with open(input_file, 'r') as f:
            for line in f:
                if line.startswith('>'):
                    name_list.append(line.strip()[1:])
        files_count = {}
        for i in name_list:
            file_name = os.path.splitext(os.path.basename(i))[0].lower().split("#")[0]
            if file_name in files_count:
                files_count[file_name] += 1
            else:
                files_count[file_name] = 1
        if self.split_only == False:
            with open(os.path.join(out_dir,"primer_results.csv"), "w", newline='') as csv_file:
                csv_writer = csv.writer(csv_file)
                csv_writer.writerow(["SEQUENCE_NAME", "SEQUENCE_TEMPLATE", "PRIMER_LEFT_SEQUENCE", "PRIMER_RIGHT_SEQUENCE", "PRIMER_LEFT_TM", "PRIMER_RIGHT_TM"] + list(files_count.keys()))
                
        my_records_ultimate = defaultdict(list)

        gene_names = split_sequences(input_file, os.path.join(out_dir, "alignment"), self.gene_max_length, self.gene_max_length-self.gene_min_length)
        for gene_name in gene_names:
            tmp_path = os.path.join(out_dir, "alignment",gene_name + ".fasta")
            my_records_ultimate[gene_name] = list(SeqIO.parse(tmp_path, 'fasta'))
        task_pool2 = []
        results2 = []
        executor2 = ProcessPoolExecutor(max_workers=thread_number)
        if thread_number > 1:
            for key, value in my_records_ultimate.items():
                task_pool2.append(executor2.submit(self.write_fasta_file, value, files_count,  os.path.join(out_dir, key + ".Fasta"), False))
            for i in task_pool2:
                results2.append(i.result())
        else:
            for key, value in my_records_ultimate.items():
                results2.append(self.write_fasta_file(value, files_count, os.path.join(out_dir, key + ".Fasta"), False))

    def extract_reference_from_gb(self, file_path):
        gene_min_length = self.gene_min_length
        gene_max_length = self.gene_max_length
        All_Records = defaultdict(list)  # all rec
        for rec in SeqIO.parse(file_path, "gb"):
            Records = defaultdict(list)
            multi_fragment_complex_gene = []
            appeared_gene = []

            crossed_origin_gene = ["psbA", "trnH-GUG"]
            sequence = rec.seq
            temp = [i.strand for i in rec.features if i.type == "source"]
            strand_all = temp[0] if temp != [] else 1  # genbank默认为正义链
            start_all = 1
            end_all = len(rec.seq)
            organism = rec.annotations["organism"].replace(" ", "_")
            id = rec.id 
            file_name = os.path.splitext(os.path.basename(file_path))[0]
            identifier = file_name

            for feature in rec.features:
                if feature.type == "gene" and "gene" in feature.qualifiers.keys() and feature.qualifiers["gene"][0] in crossed_origin_gene:
                    gene_information = {}
                    seq = feature.location.extract(sequence)
                    gene_name = feature.qualifiers["gene"][0].replace(" ", "_")
                    gene_information["gene_name"] = feature.qualifiers["gene"][0]
                    gene_information["gene_sequence"] = seq
                    gene_information["identifier"] = identifier
                    gene_information["organism"] = organism
                    gene_information["id"] = id
                    gene_information["length"] = len(str(seq))
                    gene_information["start"] = feature.location.start
                    gene_information["end"] = feature.location.end
                    if gene_name not in appeared_gene:
                        appeared_gene.append(gene_name)
                    else:
                        pass

                    if gene_name not in Records:
                        Records[gene_name] = [gene_information]
                    else:
                        Records[gene_name].append(gene_information)

                elif feature.type == "gene" and "gene" in feature.qualifiers.keys():
                    gene_information = {}
                    gene_name = feature.qualifiers["gene"][0].replace(" ", "_")
                    location = feature.location
                    if "join" in str(location):
                        multi_fragment_complex_gene.append(gene_name)
                        continue

                    strand = feature.strand
                    start = feature.location.start
                    end = feature.location.end
                    start, end = self.add_soft_boundary(int(start), int(end), start_all, end_all)
                    if strand == strand_all:
                        seq = sequence[start:end]
                        gene_information["gene_sequence"] = seq
                    else:
                        seq = sequence[start:end].reverse_complement()
                        gene_information["gene_sequence"] = seq
                    gene_information["gene_name"] = feature.qualifiers["gene"][0]
                    gene_information["identifier"] = identifier
                    gene_information["organism"] = organism
                    gene_information["id"] = id
                    gene_information["length"] = len(str(seq))
                    gene_information["start"] = feature.location.start
                    gene_information["end"] = feature.location.end
                    appeared_gene.append(gene_name)
                    if gene_name not in Records:
                        Records[gene_name] = [gene_information]
                    else:
                        Records[gene_name].append(gene_information)
                else:
                    pass

            if not Records:
                continue

            for key, value in Records.items():
                number = 0
                if len(Records[key]) >= 2:
                    for i in range(len(value)):
                        number += 1
                        Records[key][i]["identifier"] = Records[key][i]["identifier"] + \
                            "|"+str(number)

            for key, value in Records.items():
                for i in range(len(value)):
                    start = int(value[i]["start"])
                    end = int(value[i]["end"])
                    if (end - start >= gene_min_length) and (end - start <= gene_max_length):  
                        temp = SeqRecord(
                            id=value[i]["identifier"], seq=value[i]["gene_sequence"], description="")
                        if key not in All_Records:
                            All_Records[key] = [temp]
                        else:
                            All_Records[key].append(temp)
        return All_Records


if __name__ == '__main__':
    if sys.platform.startswith('win'):
        multiprocessing.freeze_support()
    pars = argparse.ArgumentParser(
        formatter_class=argparse.RawDescriptionHelpFormatter, description=''' build ref YY ''')
    pars.add_argument('-input', metavar='<str>', type=str,
                      help='''input folder.''', required=False, default='./build_primer_test/Coptis.gb')
    pars.add_argument('-soft_boundary', metavar='<int>', type=int,
                      help='''soft boundary''', required=False, default=200)
    pars.add_argument('-max_marker_length', metavar='<int>', type=int,
                      help='''max  marker length''', required=False, default=2000)
    pars.add_argument('-min_marker_length', metavar='<int>', type=int,
                      help='''min marker length''', required=False, default=200)
    pars.add_argument('-max_seq_length', metavar='<int>', type=int,
                      help='''max length''', required=False, default=5000)
    pars.add_argument('-min_seq_length', metavar='<int>', type=int,
                      help='''min length''', required=False, default=200)
    pars.add_argument('-gap_length', metavar='<int>', type=int,
                      help='''min length''', required=False, default=100)
    pars.add_argument('-t', metavar='<int>', type=int,
                      help='''thread number''', required=False, default=1)
    pars.add_argument('-usevar', type=str2bool, nargs='?', const=True, help='''Designing primers using consensus region only''', default = True)
    pars.add_argument('-split_only', type=str2bool, nargs='?', const=True, help='''是否仅仅分隔序列''', default = False)
    pars.add_argument('-do_aln', type=str2bool, nargs='?', const=True, help='''是否排序''', default = False)
    pars.add_argument('-out_dir', metavar='<str>', type=str,
                      help='''output folder.''', required=False, default='./build_primer_test/out_test')
    pars.add_argument('-max_primer', metavar='<int>', type=int,
                      help='''max number of primer''', required=False, default=64)

    args = pars.parse_args()
    print("Do not close this window manually, please!")
    out = args.out_dir
    input_data = args.input
    soft_boundary = args.soft_boundary
    max_seq_length = args.max_seq_length
    min_seq_length = args.min_seq_length
    max_marker_length = args.max_marker_length
    min_marker_length = args.min_marker_length
    gap_length = args.gap_length
    max_primer = args.max_primer
    thread_number = args.t
    split_only = args.split_only
    usevar = args.usevar
    do_alignment = args.do_aln

    configuration_information = {"out": out,  "input": input_data, "soft_boundary": soft_boundary,
                                 "max_marker_length": max_marker_length, "min_marker_length": min_marker_length,
                                 "max_seq_length": max_seq_length, "min_seq_length": min_seq_length, "gap_length": gap_length,
                                 "thread_number": thread_number, "max_primer": max_primer, "usevar":usevar,
                                 "split_only": split_only, "do_alignment": do_alignment}
    
    if os.path.isdir(input_data):
        target1 = Extract_reference(configuration_information)
        target1.extract_reference_from_gb_parallel()
    elif input_data.lower()[-3:] == ".gb":
        org_gb_path = os.path.join(out, "org_gb")
        if not os.path.exists(org_gb_path):
            os.makedirs(org_gb_path)
        records = SeqIO.parse(input_data, "genbank")
        for record in records:
            organism = record.annotations.get("organism", "organism").replace(" ", "_").replace(".", "")
            accession = record.annotations.get("accessions", ["accessions"])[0]
            output_filename = os.path.join(out, "org_gb", organism + "#" + accession + ".gb")
            with open(output_filename, "w") as output_file:
                SeqIO.write(record, output_file, "genbank")
        configuration_information["input"] = org_gb_path
        target1 = Extract_reference(configuration_information)
        target1.extract_reference_from_gb_parallel()
    else:
        target1 = Extract_reference(configuration_information)
        target1.extract_reference_from_fasta()