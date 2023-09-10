import sys
import os, time
import argparse
import gzip
from Bio import SeqIO
pars = argparse.ArgumentParser(
    formatter_class=argparse.RawDescriptionHelpFormatter, description=''' Filter by YY 20230314''')

def Judge_Type(path):
    """
    返回不同文件的类型
    :param infile: 文件路径
    :return: 返回文件类型
    """
    suffix_dict = {'.gz': 0, '.fq': 1, '.fastq': 1,
                   'fa': 2, '.fas': 2, '.fasta': 2}
    return suffix_dict.get(os.path.splitext(path)[-1].lower(), 3)

def Bytes_Str(input, is_bytes_type):
    return input.decode('utf-8') if is_bytes_type else input

def Write_Print(log_path, *log_str, sep = " "):
    """
    记录日志并打印
    """
    line = sep.join(map(str,log_str)).strip()
    with open(log_path, 'a') as out:
        out.write(line)
        out.write('\n')
    print(line)

def Reads_Exporter(file_1, file_2, out_path, out_1, out_2,  max_reads = 1e9, skip = 0):
    """
    :param file_1: 一端的输入文件
    :param file_2: 另一端的输入文件
    :return: 正常结束返回1
    """

    bytes_type = file_1[-3:].lower() == ".gz" # 判断是否为gz格式
    paired_reads_file = not file_1 == file_2 # 判断是否为单端测序
    isfasta = True if Judge_Type(file_1) == 2 else False
    Filted_File_Ext = '.fasta' if isfasta else '.fq'
    skip_count = skip << 20
    # 打开输入文件
    infile_1 = gzip.open(file_1, 'r') if bytes_type else open(file_1, 'r')
    outfile_1 = open(os.path.join(args.o,  out_1 + Filted_File_Ext), 'w')
    outdata_1 = []
    if paired_reads_file: 
        infile_2 = gzip.open(file_2, 'r') if bytes_type else open(file_2, 'r') 
        outfile_2 = open(os.path.join(args.o,  out_2 + Filted_File_Ext), 'w')
        outdata_2 = []

    # 构建文件指针列表，打开并清空所有文件
    # 读取输入文件
    t1, t2, reads_count = time.time(), 0, 0 # 初始化时间和计数
    for _ in infile_1:
        # 构建reads列表
        temp_rec1 = [_, infile_1.readline()] if isfasta else [_, infile_1.readline(),infile_1.readline(),infile_1.readline()] 
        if paired_reads_file: temp_rec2 = [infile_2.readline(),infile_2.readline(),infile_2.readline(),infile_2.readline()]
        if skip_count > 0:
            skip_count -= 1
            if skip_count & 0b11111111111111111111 == 0:
                t2 = time.time()
                t1, t2 = t2, t2 - t1
                Write_Print(os.path.join(args.o, "log.txt"), 'Skiped', skip - (skip_count >> 20), 'm reads,',round(t2,2),'s/m reads'," "*4)
            continue
        else:
            reads_count += 1

        if paired_reads_file: 
            outdata_1.extend([
                                    Bytes_Str(temp_rec1[0], bytes_type), 
                                    Bytes_Str(temp_rec1[1],bytes_type),
                                    Bytes_Str(temp_rec1[2],bytes_type),
                                    Bytes_Str(temp_rec1[3],bytes_type)
                                    ])
            outdata_2.extend([
                                    Bytes_Str(temp_rec2[0], bytes_type), 
                                    Bytes_Str(temp_rec2[1],bytes_type),
                                    Bytes_Str(temp_rec2[2],bytes_type),
                                    Bytes_Str(temp_rec2[3],bytes_type)
                                    ])
        else:
            if isfasta:
                outdata_1.extend(['>',
                                    Bytes_Str(temp_rec1[0], bytes_type), 
                                    Bytes_Str(temp_rec1[1],bytes_type)])
            else:
                outdata_1.extend([
                                    Bytes_Str(temp_rec1[0], bytes_type), 
                                    Bytes_Str(temp_rec1[1],bytes_type),
                                    Bytes_Str(temp_rec1[2],bytes_type),
                                    Bytes_Str(temp_rec1[3],bytes_type)])
        # 每m reads输出估算速度
        if reads_count & 0b11111111111111111111 == 0:
            outfile_1.writelines(outdata_1)
            outdata_1.clear()
            if paired_reads_file: 
                outfile_2.writelines(outdata_2)
                outdata_2.clear()
            t2 = time.time()
            t1, t2 = t2, t2 - t1
            Write_Print(os.path.join(args.o, "log.txt"), 'Exported',reads_count >> 20, 'm reads,',round(t2,2),'s/m reads'," "*4)
            if (reads_count >> 20) >= max_reads: break
    # 关闭输出和输入文件
    outfile_1.writelines(outdata_1)
    outdata_1.clear()
    if paired_reads_file: 
        outfile_2.writelines(outdata_2)
        outdata_2.clear()
    infile_1.close()
    outfile_1.close()
    if paired_reads_file: 
        infile_2.close()
        outfile_2.close()
    return 1

if __name__ == '__main__':
    pars.add_argument('-i1', metavar='<str>', type=str, help='''input fq/gz -1 files.''', required=False,
                    default=['D:\working\Develop\EasyMiner Develop\DEMOS\DEMO_03/Cnidium_monnieri.1.fq.gz'], nargs="+")
    pars.add_argument('-i2', metavar='<str>', type=str, help='''input fq/gz -2 files.''', required=False,
                    default=['D:\working\Develop\EasyMiner Develop\DEMOS\DEMO_03/Cnidium_monnieri.2.fq.gz'], nargs="+")
    pars.add_argument('-o1', metavar='<str>', type=str, help='''output files.''', required=False,
                    default=['out_1'], nargs="+")
    pars.add_argument('-o2', metavar='<str>', type=str, help='''input fq/gz -2 files.''', required=False,
                    default=['out_2'], nargs="+")
    pars.add_argument('-m_reads', metavar='<int>', type=int, help='''Maximum number of reads used''', default=2)
    pars.add_argument('-skip', metavar='<int>', type=int, help='''Skip number of reads used''', default=2)
    pars.add_argument('-o', metavar='<str>', type=str,
                    help='''out dir.''', required=False, default="D:/working/Develop/EasyMiner Develop/DEMOS/DEMO_03/results")
    args = pars.parse_args()
    Write_Print(os.path.join(args.o, "log.txt"), "Start outputting reads...")
    if args.i2[0] == "":
        args.i2 = args.i1
        args.o2 = args.o1
    print("Do not close this window manually, please!")
    for i in range(len(args.i1)):
        Reads_Exporter(args.i1[i], args.i2[i], args.o, args.o1[i], args.o2[i], args.m_reads, args.skip)