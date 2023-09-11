import sys
import os
import time
import argparse
from collections import defaultdict, OrderedDict
import gzip
from Bio import SeqIO
pars = argparse.ArgumentParser(
    formatter_class=argparse.RawDescriptionHelpFormatter, description=''' Filter by YY 20230314''')


def STR2BOOL(v):  # 在参数中使用逻辑值
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Unsupported value encountered.')

pars.add_argument('-r', metavar='<str>', type=str, help='''input ref file or dir.''',
                      required=False, default=r"D:\working\Develop\EasyMiner Develop\DEMOS\DEMO\A_lyrata")
pars.add_argument('-q1', metavar='<str>', type=str, help='''input fq/gz -1 files.''', required=False,
                    default=[r"D:\working\Develop\EasyMiner Develop\DEMOS\DEMO\Arabidopsis_thaliana.1.fq.gz"], nargs="+")
pars.add_argument('-q2', metavar='<str>', type=str, help='''input fq/gz -2 files.''', required=False,
                    default=[r"D:\working\Develop\EasyMiner Develop\DEMOS\DEMO\Arabidopsis_thaliana.2.fq.gz"], nargs="+")
pars.add_argument('-kf', metavar='<int>', type=int,
                    help='''kmer of filter''', default=31)
pars.add_argument('-s', metavar='<int>', type=int, help='''length of the sliding window on the reads''', default=4)
pars.add_argument('-m_reads', metavar='<int>', type=int, help='''Maximum number of reads used''', default=1e9)
pars.add_argument('-o', metavar='<str>', type=str,
                    help='''out dir.''', required=False, default=r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\results")
pars.add_argument('-lkd', metavar='<str>', type=str,
                    help='''load kmer dict''', required=False, default='')
pars.add_argument('-gr', type=STR2BOOL, nargs='?', const=True,
                    help='''get reverse (fast speed) or not (low memory)''', default=True)

# 预定义一些常用的掩码
MASK_11_30 = ((1 << 20) - 1) << 10
MASK_20 = (1 << 20) - 1
MASK_30 = (1 << 30) - 1

def Write_Log(log_path, *log_str):
    """
    记录日志
    """
    with open(log_path, 'a') as out:
        for i in log_str[:-1]:
            out.write(str(i) + ',')
        out.write(str(log_str[-1]) + '\n')

def Write_Print(log_path, *log_str, sep = " "):
    """
    记录日志并打印
    """
    line = sep.join(map(str,log_str)).strip()
    with open(log_path, 'a') as out:
        out.write(line)
        out.write('\n')
    print(line)

def Seq_To_Int(dna_str, reverse=False):
    """
    将基因转换为整数
    :param dna_str: 序列，字符串类型，包含AGCTU
    :param reverse: 是否返回反向互补的整数
    :return: 返回[序列的整数形式]和清理后长度，或者反向互补序列的整数形式
    """
    if reverse:
        dna_str = dna_str.translate(str.maketrans(
            "ACGTU", "32100", "RYMKSWHBVDN\n\r"))[::-1]
        return int(dna_str, 4)
    else:
        dna_str = dna_str.translate(str.maketrans(
            "ACGTU", "01233", "RYMKSWHBVDN\n\r"))
        return [int(dna_str, 4)], len(dna_str)


def Int_To_Seq(seq_bin, seq_length):
    """
    将整数转换为基因
    :param seq_bin: 序列的整数形式
    :param seq_length: 序列的长度，用来补0
    :return: 返回整数序列的基因形式
    """
    seq_dict = {'00': 'A', '01': 'C', '10': 'G', '11': 'T'}
    seq_list = []
    seq_bin_str = bin(seq_bin)[2:].rjust(seq_length << 1, '0')
    for j in range(seq_length):
        seq_list.append(seq_dict[seq_bin_str[j << 1:(j << 1)+2]])
    return ''.join(seq_list)


def Reverse_Int(dna_int, dna_length):
    """
    将基因的整数型转换为反向互补的基因的整数型
    :param dna_int: 序列的整数形式
    :param dna_length: 序列的长度，用来补0
    :return: 返回整数序列的基因形式
    """
    bin_str = bin(dna_int ^ ((1 << (dna_length << 1)) - 1)
                  )[2:].rjust(dna_length << 1, '0')
    new_list = []
    for i in range(0, dna_length << 1, 2):
        new_list.insert(0, bin_str[i+1])
        new_list.insert(0, bin_str[i])
    return int(''.join(new_list), 2)


def Get_Dict_Memory_Size(dictionary):
    """
    计算字典的实际内存占用
    :param dictionary: 字典数据类型
    :return: 返回内存占用，单位为字节
    """
    size = sys.getsizeof(dictionary)
    for key, value in dictionary.items():
        if isinstance(value, list) or isinstance(value, tuple):
            size += sys.getsizeof(key) + Get_List_Memory_Size(value)
        elif isinstance(value, dict):
            size += sys.getsizeof(key) + Get_Dict_Memory_Size(value)
        else:
            size += sys.getsizeof(key) + sys.getsizeof(key)
    return size


def Get_List_Memory_Size(lst):
    """
    计算列表的实际内存占用
    :param lst: 列表数据类型
    :return: 返回内存占用，单位为字节
    """
    size = sys.getsizeof(lst)
    for item in lst:
        if isinstance(item, list) or isinstance(item, tuple):
            size += sys.getsizeof(item) + Get_List_Memory_Size(item)
        elif isinstance(item, dict):
            size += sys.getsizeof(item) + Get_Dict_Memory_Size(item)
        else:
            size += sys.getsizeof(item) + sys.getsizeof(item)
    return size


def Judge_Type(path):
    """
    返回不同文件的类型
    :param infile: 文件路径
    :return: 返回文件类型
    """
    suffix_dict = {'.gz': 0, '.fq': 1, '.fastq': 1,
                   'fa': 2, '.fas': 2, '.fasta': 2}
    return suffix_dict.get(os.path.splitext(path)[-1].lower(), 3)


def Read_Seq_File(infile):
    """
    返回不同文件的指针
    :param infile: 文件路径
    :return: 返回文件指针
    """
    if Judge_Type(infile) == 0:
        # 读取二代测序数据的每一条reads
        return SeqIO.parse(gzip.open(infile, "rt"), "fastq")
    elif Judge_Type(infile) == 2:
        return SeqIO.parse(open(infile), "fasta")  # fasta文件的每一条序列
    elif Judge_Type(infile) == 1:
        return SeqIO.parse(open(infile), "fastq")  # 读取二代测序数据的每一条reads
    else:
        Write_Print(os.path.join(args.o,  "log.txt"), "Error: your input may invalid, please check your input.")
        sys.exit(1)


def Bytes_Str(input, is_bytes_type):
    return input.decode('utf-8').strip() + '\n' if is_bytes_type else input.strip() + '\n'


def Make_Kmer_Dict_v6(_kmer_dict, file_list, kmer_size, get_reverse=False, get_pos=False, show_info=True):
    """
    制作整数型Kmer字典
    :param _kmer_dict: 用来保存kmer的字典变量
    :param file_list: 参考序列的文件列表
    :param kmer_size: kmer的长度
    :param get_reverse: 是否考虑反向互补序列
    :param get_pos: 是否记录位置
    :param show_info: 是否动态显示基因数量
    :return: 正常结束返回1
    """
    gene_count = 0
    MASK_BIN = (1 << (kmer_size << 1)) - 1  # kmer的掩码
    DEPTH_BIN = 1 << 10  # kmer深度的递增
    for i in range(len(file_list)):
        if os.path.isfile(file_list[i]) == False:
            continue  # 如果是个文件夹则跳过
        infile = open(file_list[i], 'r', encoding='utf-8', errors='ignore')
        # 1-10位为位置千分比，11-30为深度，31-35为预留符号位，36及以后为文件
        file_id = 1 << (i+35)  # 设置档期文件的符号位
        infile.readline()
        for line in infile:
            # 读取一条序列，合并换行
            temp_str = []
            while line and line[0] != '>':
                temp_str.append(line)
                line = infile.readline()
            gene_count += 1  # 基因序列计数加1
            refseq = ''.join(
                filter(str.isalpha, ''.join(temp_str).upper()))  # 只保留字母
            intseqs, ref_len = Seq_To_Int(refseq)  # 序列转整数，获取长度
            if get_reverse:
                intseqs.append(Seq_To_Int(refseq, True))  # 加入反向互补序列
            for x, y in enumerate(intseqs):
                # 初始化符号位和文件位，反向互补序列的31位符号位为1
                SIGN_BIN = (1 << 30) + (1 << 10) + \
                    file_id if x else (1 << 10) + file_id
                for j in range(0, ref_len-kmer_size+1):
                    temp_int = SIGN_BIN  # 初始化文件位和深度
                    kmer_int = y >> (j << 1) & MASK_BIN  # 获取kmer的整数形式
                    if kmer_int in _kmer_dict:
                        temp_int = _kmer_dict[kmer_int]
                        temp_int += DEPTH_BIN  # 深度加1，深度大于2**20会溢出,不太可能这么深的kmer
                        temp_int |= file_id  # 赋值文件位
                        _kmer_dict[kmer_int] = temp_int
                    else:
                        if get_pos:
                            # 记录kmer的千分比位置
                            temp_int += int((j+1)/len(refseq)*1000)
                        _kmer_dict[kmer_int] = temp_int
        # if show_info:
        #     Write_Print(os.path.join(args.o,  "log.txt"), 'Num. of Ref. Seq.:', gene_count)
        infile.close()
    return 1


def Get_Ref_Info(ref_path, _ref_path_list, _ref_path_dict, _ref_length_dict):
    """
    制作保存参考序列路径和长度的字典
    :param ref_path: 参考序列路径
    :param _ref_path_list: 参考序列的文件列表
    :param _ref_length_dict: 参考序列长度字典
    :param _ref_path_dict: 参考序列路径字典
    :return: 正常结束返回1
    """
    if os.path.isdir(ref_path):
        for i in os.listdir(ref_path):
            if Judge_Type(i) == 2:
                _ref_path_list.append(os.path.join(ref_path, i))
    elif os.path.isfile(ref_path):
        _ref_path_list = [ref_path]
    else:
        return 0
    for file in _ref_path_list:
        ref_seq_count = 0
        if Judge_Type(file) == 2:
            file_name = os.path.splitext(os.path.basename(file))[0]
            _ref_path_dict[file_name] = file
            for rec in Read_Seq_File(file):
                refseq = ''.join(filter(str.isalnum, str(rec.seq).upper()))
                ref_seq_count += 1
                _ref_length_dict[file_name] += len(refseq)
            _ref_length_dict[file_name] = int(
                _ref_length_dict[file_name] / max(ref_seq_count, 1))
    return 1


def Filter_Reads_v1(_kmer_dict, kmer_size, step_size, read_seq, mask, get_reverse=False, single_ref_file=False):
    """
    获取当前reads对应的基因的文件位置
    :param _kmer_dict: 用来保存kmer的字典变量
    :param kmer_size: kmer的长度
    :param setp_size: 生成kmer的步长
    :param mask: kmer的掩码
    :param get_reverse: 是否考虑反向互补序列
    :param single_ref_file: 是否只有一个参考基因文件
    :return: 返回当前reads对应的基因的文件位置的字符串
    """
    if not read_seq:
        return ""
    result_int = 0
    read_int, ref_len = Seq_To_Int(read_seq)  # 序列转整数，获取长度
    if get_reverse:
        read_int.append(Seq_To_Int(read_seq, True))
    for s in read_int:
        for j in range(0, ref_len - kmer_size - step_size, step_size):
            kmer = s >> (j << 1) & mask  # 构建kmer的整数形式
            if kmer in _kmer_dict:
                result_int |= _kmer_dict[kmer] >> 35  # 获取所有可能的文件位置
                if single_ref_file:
                    break
    return bin(result_int)[:1:-1]  # 返回二进制形式


def Reads_Filter_v5(_kmer_dict, _ref_reads_count_dict, file_list, kmer_size, setp_size, file_1, file_2, out_path, get_reverse = True, max_reads = 1e9):
    """
    过滤Paired Reads
    :param _kmer_dict: 用来保存kmer的字典变量
    :param _ref_reads_count_dict: 参考序列的reads计数
    :param file_list: 参考序列文件列表
    :param kmer_size: kmer的长度
    :param setp_size: 生成kmer的步长
    :param file_1: 一端的输入文件
    :param file_2: 另一端的输入文件
    :param t_id: 线程的id
    :param t_count: 线程的数量
    :param get_reverse: 是否考虑反向互补序列
    :return: 正常结束返回1
    """
    t1, t2, reads_count = time.time(), 0, 0 # 初始化时间和计数
    MASK_BIN = (1<< (kmer_size<<1)) - 1 # 设定kmer掩码
    bytes_type = file_1[-3:].lower() == ".gz" # 判断是否为gz格式
    single_ref_file = len(file_list) == 1 # 判断是否只有一个参考序列文件
    paired_reads_file = not file_1 == file_2 # 判断是否为单端测序
    isfasta = True if Judge_Type(file_1) == 2 else False
    Filted_File_Ext = '.fasta' if isfasta else '.fq'
    # 打开输入文件
    infile_1 = gzip.open(file_1, 'r') if bytes_type else open(file_1, 'r')
    if paired_reads_file: infile_2 = gzip.open(file_2, 'r') if bytes_type else open(file_2, 'r') 
    # 构建文件指针列表，打开并清空所有文件
    file_dict, file_name_dict = {}, {}
    for x, y in enumerate(file_list):
        file_name = os.path.basename(y).split('.')[1] if os.path.basename(y).split('.')[0] == 'contig' else os.path.basename(y).split('.')[0]
        file_name_dict[x] = file_name
        file_dict[x] = open(os.path.join(out_path, 'filtered', file_name + Filted_File_Ext), "w")
    # 读取输入文件
    for _ in infile_1:
        reads_count += 1
        # 构建reads列表
        temp_rec1 = [_, infile_1.readline()] if isfasta else [_, infile_1.readline(),infile_1.readline(),infile_1.readline()] 
        if paired_reads_file: temp_rec2 = [infile_2.readline(),infile_2.readline(),infile_2.readline(),infile_2.readline()]
        # 获取文件位置的二进制字符串
        file_line = Filter_Reads_v1(_kmer_dict, kmer_size, setp_size, 
                                    Bytes_Str(temp_rec1[1], bytes_type), 
                                    MASK_BIN, get_reverse, single_ref_file)
        for x, y in enumerate(file_line):
            if y == '1':
                # 构建参考序列reads计数字典
                if file_name_dict[x] in _ref_reads_count_dict:
                    _ref_reads_count_dict[file_name_dict[x]] += int(paired_reads_file) + 1
                else:
                    _ref_reads_count_dict[file_name_dict[x]] = int(paired_reads_file) + 1
                # 写入输出文件
                if paired_reads_file: 
                    file_dict[x].writelines([
                                            Bytes_Str(temp_rec1[0], bytes_type), 
                                            Bytes_Str(temp_rec1[1],bytes_type),
                                            Bytes_Str(temp_rec1[2],bytes_type),
                                            Bytes_Str(temp_rec1[3],bytes_type),
                                            Bytes_Str(temp_rec2[0], bytes_type), 
                                            Bytes_Str(temp_rec2[1],bytes_type),
                                            Bytes_Str(temp_rec2[2],bytes_type),
                                            Bytes_Str(temp_rec2[3],bytes_type)
                                            ])
                else:
                    if isfasta:
                        file_dict[x].writelines(['>',
                                            Bytes_Str(temp_rec1[0], bytes_type), 
                                            Bytes_Str(temp_rec1[1],bytes_type)])
                    else:
                        file_dict[x].writelines([
                                            Bytes_Str(temp_rec1[0], bytes_type), 
                                            Bytes_Str(temp_rec1[1],bytes_type),
                                            Bytes_Str(temp_rec1[2],bytes_type),
                                            Bytes_Str(temp_rec1[3],bytes_type)])
        # 每m reads输出估算速度
        if reads_count & 0b11111111111111111111 == 0:
            t2 = time.time()
            t1, t2 = t2, t2 - t1
            Write_Print(os.path.join(args.o,  "log.txt"), 'handled\t',reads_count >> 20, 'm reads,',round(t2,2),'s/m reads'," "*4)
            if (reads_count >> 20) >= max_reads: break
    # 关闭输出和输入文件
    for v in file_dict.values(): v.close()
    infile_1.close()
    if paired_reads_file: infile_2.close()
    return 1

def Write_Dict(_dict, file_name, is_int = True, append = False):
    """
    写入16进制kmer字典
    :param _dict: kmer字典
    :param file_name: 文件名，
    :return: 成功返回1
    """
    wfd = open(file_name,'a') if append else open(file_name,'w') 
    for key, value in _dict.items():
        if is_int:
            wfd.write(",".join([hex(key), hex(value),'\n']))
        else:
            if type(value) == list:
                wfd.write(",".join([str(key), ",".join(map(str,value)),'\n']))
            else:
                wfd.write(",".join([str(key), str(value),'\n']))
    return 1

def Read_Dict(_dict, file_name, is_int = True):
    """
    读取16进制kmer字典
    :param _dict: kmer字典
    :param file_name: 文件名，
    :return: 成功返回1
    """
    with open(file_name,'r') as rfd:
        for line in rfd:
            line_list = line.split(",") 
            if is_int:
                _dict[int(line_list[0],16)] = int(line_list[1],16)
            else:
                _dict[line_list[0]] = int(line_list[1])
    return 1

ACGT_DICT = {0: 'A', 1: 'C', 2: 'G', 3: 'T'}
ref_path_list = []  # 参考序列路径列表
ref_path_dict = defaultdict(str)  # 序列路径字典
ref_length_dict = defaultdict(int)  # 序列长度字典
kmer_dict = OrderedDict()  # kmer字典
ref_reads_count_dict = {}  # reads计数的字典

if __name__ == '__main__':
    args = pars.parse_args()
    try:
        if not args.r:
            Write_Print(os.path.join(args.o,  "log.txt"), 'Must have reference!')
            sys.exit(0)
        print("Do not close this window manually, please!")
        # 初始化输出文件夹
        if not os.path.isdir(args.o):
            os.mkdir(args.o)
        if not os.path.isdir(os.path.join(args.o, 'filtered')):
            os.mkdir(os.path.join(args.o, 'filtered'))
        Write_Print(os.path.join(args.o,  "log.txt"), "Getting information from references ...")
        # 初始化参考序列信息
        if not Get_Ref_Info(args.r, ref_path_list, ref_path_dict, ref_length_dict):
            Write_Print(os.path.join(args.o,  "log.txt"), 'Invaild reference!')
            sys.exit(0)
        # 开始计时
        t0 = time.time()
        Write_Print(os.path.join(args.o,  "log.txt"), '======================== Filter =========================')
        if args.lkd:
            # 载入保存的字典
            if os.path.exists(os.path.join(args.lkd)):
                Read_Dict(kmer_dict, os.path.join(args.lkd))
                Write_Print(os.path.join(args.o,  "log.txt"), "The dictionary file of length", len(
                    kmer_dict), "has been successfully loaded.")
            elif os.path.exists(os.path.join(args.o, args.lkd)):
                Read_Dict(kmer_dict, os.path.join(args.o, args.lkd))
                Write_Print(os.path.join(args.o,  "log.txt"), "The dictionary file of length", len(
                    kmer_dict), "has been successfully loaded.")
            else:
                # 生成kmer字典
                Make_Kmer_Dict_v6(kmer_dict, ref_path_list, args.kf, args.gr, False)
                # 保存kmer字典
                Write_Dict(kmer_dict, os.path.join(args.o, args.lkd))
        else:
            Make_Kmer_Dict_v6(kmer_dict, ref_path_list, args.kf, args.gr, False)
        t1 = time.time()
        Write_Print(os.path.join(args.o,  "log.txt"), '\nSetp1 cost:', t1 - t0, " "*32)  # 建立kmer字典的时间
        # 判断输入测序文件的类型
        if not args.q1:
            Write_Print(os.path.join(args.o,  "log.txt"), "At least have one input file")
            sys.exit(0)
        if args.q1 and not args.q2:  # 单端测序文件
            args.q2 = args.q1
        Filted_File_Ext = '.fasta' if Judge_Type(args.q1[0]) == 2 else '.fq'

        for i in range(len(args.q1)):
            Reads_Filter_v5(kmer_dict, ref_reads_count_dict, ref_path_list,
                            args.kf, args.s, args.q1[i], args.q2[i], args.o,
                            not args.gr, args.m_reads)
        Write_Dict(ref_reads_count_dict, os.path.join(
            args.o, "ref_reads_count_dict.txt"), False, True)
        t2 = time.time()
        Write_Print(os.path.join(args.o,  "log.txt"), 'Setp2 cost:', t2 - t1, " "*32)  # 过滤所用的时间
        Write_Print(os.path.join(args.o,  "log.txt"), 'Total cost:', t2 - t0, " "*32)  # 全部过程所用的时间
    except Exception as e:
        Write_Print(os.path.join(args.o,  "log.txt"), e)



