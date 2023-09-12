import warnings
warnings.filterwarnings("ignore")
import os
import argparse
from Bio import pairwise2
from Bio.Seq import Seq
from Bio import SeqIO

def generate_consensus(seq_record, UseN = True):
    # 将记录中的序列拆分成一个列表
    sequences = [str(record.seq) for record in seq_record]
    # 生成一致序列
    consensus_sequence = ''
    for i in range(len(sequences[0])):
        bases = [seq[i] for seq in sequences]
        consensus_base = max(set(bases), key=bases.count)
        if UseN:
            if len(set(bases)) > 1: consensus_base = "N"
            if "-" in bases: consensus_base = "" # 不写入gap
        consensus_sequence += consensus_base
    return consensus_sequence

def cut_seq(result_file, consensus_seq):
    result_name = ""
    result_seq = ""
    with open(result_file, "r") as rf:
        for record in SeqIO.parse(rf, "fasta"):
            result_seq = record.seq
            result_name = record.id
            break  # 仅获取第一个序列
    # 执行比对
    alignments = pairwise2.align.localms(result_seq, consensus_seq, 1, -1, -3, -2) #.globalxx(result_seq, consensus_seq, one_alignment_only=True, gap_char="-")
    if alignments:
        alignment = alignments[0]
        aligned_result_seq = alignment[0]
        aligned_consensus_seq = alignment[1]
        # 寻找前导和尾随的gap，然后切齐序列
        leading_gap = len(aligned_consensus_seq) - len(aligned_consensus_seq.lstrip("-"))
        trailing_gap = len(aligned_consensus_seq.rstrip("-"))
        result_seq = aligned_result_seq[leading_gap:trailing_gap]
    return result_name, result_seq


if __name__ == '__main__':
    pars = argparse.ArgumentParser(formatter_class=argparse.RawDescriptionHelpFormatter, description=''' CUTSEQ YY ''')
    pars.add_argument('-i', metavar='<str>', type=str, help='''input files.''', required=False,
                    default=r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\results\aligned\ITS.fasta")
    pars.add_argument('-o', metavar='<str>', type=str,
                    help='''out dir.''', required=False, default=r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\results")
    args = pars.parse_args()
    aln_file = args.i
    data_path = args.o
    if not os.path.isdir(os.path.join(data_path,"trimed")): os.makedirs(os.path.join(data_path,"trimed"))
    alignment_records = list(SeqIO.parse(aln_file, 'fasta'))
    # 生成一致序列
    consensus_seq = generate_consensus(alignment_records)
    # 将一致序列保存到文件
    result_file = os.path.join(data_path,"results",os.path.basename(aln_file))
    result_name, result_seq = cut_seq(result_file, consensus_seq)
    with open(os.path.join(data_path,"trimed",os.path.basename(aln_file)), 'w') as output_handle:
        output_handle.write(">" + result_name + "\n")
        output_handle.write(result_seq.replace("-","") + "\n")