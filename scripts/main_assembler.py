from Bio.SeqIO.FastaIO import SimpleFastaParser
from collections import Counter, deque
from itertools import chain
from operator import itemgetter
import argparse
import gc
import multiprocessing
import os
import sys
import time

D_BASE_DICT = {'AG':'R','CT':'Y', 'GT':'K', 'GC':'S','AC':'M', 'AT':'W','GA':'R','TC':'Y','TG':'K', 'CG':'S','CA':'M', 'TA':'W',}
ACGT_DICT = {0: 'A', 1: 'C', 2: 'G', 3: 'T'}
ACGT_REV   = str.maketrans('ACGT', 'TGCA')
FWD_TRANS  = str.maketrans("ACGTU", "01233", "RYMKSWHBVDN\n\r")
REV_TRANS  = str.maketrans("ACGTU", "32100", "RYMKSWHBVDN\n\r")
BIN_DICT   = {'00': 'A', '01': 'C', '10': 'G', '11': 'T'}

ref_path_dict = {}  # 序列路径字典
ref_count_dict = {} # 参考序列条数字典
kmer_dict = {}  # kmer字典
ref_reads_count_dict = {}  # reads计数的字典

def Write_Print(log_path, *log_str, sep = " "):
    """
    记录日志并打印
    """
    line = sep.join(map(str,log_str)).strip()
    with open(log_path, 'a') as out:
        out.write(line + '\n')
    print(line)

def Seq_To_Int(dna_str, trans=FWD_TRANS, rtrans=REV_TRANS):
    """
    将基因转换为整数
    """
    dna_fw_str = dna_str.translate(trans)
    dna_rc_str = dna_str.translate(rtrans)[::-1]

    if not dna_fw_str:
        return (), 0

    return (int(dna_fw_str, 4), int(dna_rc_str, 4)), len(dna_fw_str)

def Int_To_Seq(seq_bin, seq_length, seq_dict=BIN_DICT):
    """
    将整数转换为基因
    :param seq_bin: 序列的整数形式
    :param seq_length: 序列的长度，用来补0
    :return: 返回整数序列的基因形式
    """
    seq_bin_str = bin(seq_bin)[2:].rjust(seq_length << 1, '0')
    return ''.join(seq_dict[seq_bin_str[j << 1:(j << 1) + 2]] for j in range(seq_length))

def Reverse_Int(dna_int, dna_length):
    """
    将基因的整数型转换为反向互补的基因的整数型
    :param dna_int: 序列的整数形式
    :param dna_length: 序列的长度，用来补0
    :return: 返回整数序列的基因形式
    """
    bin_str = bin(dna_int ^ ((1 << (dna_length << 1)) - 1))[2:].rjust(dna_length << 1, '0')
    new_list = [bin_str[i:i + 2] for i in range(0, dna_length << 1, 2)]
    return int(''.join(reversed(new_list)), 2)

def Reverse_Complement_ACGT(seq, table=ACGT_REV):
    """
    简化版反向互补
    """ 
    return seq.translate(table)[::-1]

def Judge_Type(path):
    """
    返回不同文件的类型
    :param infile: 文件路径
    :return: 返回文件类型
    """
    suffix_dict = {'.gz': 0, '.fq': 1, '.fastq': 1,
                   'fa': 2, '.fas': 2, '.fasta': 2}
    return suffix_dict.get(os.path.splitext(path)[-1].lower(), 3)

def Make_Kmer_Dict(_kmer_dict, file_path, kmer_size):
    """
    制作整数型Kmer字典
    :param _kmer_dict: 用来保存kmer的字典变量
    :param file_path: 参考序列的文件名
    :param kmer_size: kmer的长度
    """
    MASK_BIN = (1 << (kmer_size << 1)) - 1  # kmer的掩码
    DEPTH_BIN = 1 << 10  # kmer深度的递增

    with open(file_path, 'r') as f:
        # 1-10位为位置千分比，11-30为深度，31-35为预留符号位，36及以后为文件
        file_id = 1 << 35  # 设置当前文件的符号位

        for _, seq in SimpleFastaParser(f):
            # 序列转整数，获取长度
            intseqs, ref_len = Seq_To_Int(''.join(filter(str.isalpha, seq)).upper())
            ref_kmer_count = ref_len - kmer_size + 1

            for x, y in enumerate(intseqs):
                # 初始化符号位和文件位，反向互补序列的31位符号位为1
                SIGN_BIN = (1 << 30) + (1 << 10) + file_id if x else (1 << 10) + file_id

                for j in range(0, ref_kmer_count):
                    temp_int = SIGN_BIN  # 初始化文件位和深度
                    kmer_int = y >> (j << 1) & MASK_BIN  # 获取kmer的整数形式

                    if kmer_int in _kmer_dict:
                        temp_int = _kmer_dict[kmer_int]
                        temp_int += DEPTH_BIN  # 深度加1，深度大于2**20会溢出,不太可能这么深的kmer
                        temp_int |= file_id  # 赋值文件位
                    else:
                        temp_int += int((j + 1) / ref_kmer_count * 1000)

                    _kmer_dict[kmer_int] = temp_int

def Get_Ref_Info(ref_path, _ref_path_dict, _ref_count_dict):
    """
    制作保存参考序列路径和长度的字典
    :param ref_path: 参考序列路径
    :param _ref_path_dict: 参考序列路径字典
    :param _ref_count_dict: 参考序列条数字典

    """
    if os.path.isdir(ref_path):
        ref_path_list = [os.path.join(ref_path, ent.name) for ent in os.scandir(ref_path) if ent.is_file()]
    elif os.path.isfile(ref_path):
        ref_path_list = [ref_path]
    else:
        ref_path_list = []

    for file in ref_path_list:
        if Judge_Type(file) != 2:
            continue

        file_name = os.path.splitext(os.path.basename(file))[0]

        with open(file) as f:
            ref_seq_count = sum(1 for _ in SimpleFastaParser(f))

        _ref_count_dict[file_name] = ref_seq_count
        _ref_path_dict[file_name] = file

def Make_Assemble_Dict(file_list, kmer_size, _kmer_dict, _ref_dict, Filted_File_Ext = '.fq'):
    """
    构建拼接用的字典
    :param file_list: 文件列表
    :param kmer_size: kmer的长度
    :param _kmer_dict: 待生成的字典value的格式为[深度，位置（1000以内的整数）]
    :param _ref_dict: 参考序列的字典
    :return: 返回kmer的总数量
    """
    MASK_BIN = (1 << (kmer_size << 1)) - 1 # kmer的掩码
    fasta_file = Filted_File_Ext == '.fasta'
    for file in file_list:
        infile = open(file, 'r', encoding='utf-8', errors='ignore')
        infile.readline()
        for line in infile:
            if fasta_file:
                temp_str = []
                while line and line[0] != '>':
                    temp_str.append(line)
                    line = infile.readline()
                read_seq = ''.join(filter(str.isalpha, ''.join(temp_str).upper()))
            else:
                read_seq = ''.join(filter(str.isalpha, line)).upper()
                infile.readline()
                infile.readline()
                infile.readline()
            intseqs, read_len = Seq_To_Int(read_seq) # 序列转整数，获取长度
            kmer_set = {x >> (j << 1) & MASK_BIN for x in intseqs for j in range(0, read_len - kmer_size)}
            for kmer in kmer_set:
                if kmer in _kmer_dict:
                    _kmer_dict[kmer][0] += 1
                elif kmer in _ref_dict: # kmer的位置
                    temp_int = int(_ref_dict[kmer])
                    temp_depth = (temp_int >> 10) & ((1<<20) -1) #在参考序列中的深度
                    temp_pos = temp_int & 1023
                    is_reverse = bool(temp_int & 1073741824) # 判断是否为反向互补的序列
                    if is_reverse:
                        # 标记为反向的的kmer
                        temp_pos = 1000 - temp_pos
                    _kmer_dict[kmer] = [1, temp_pos, is_reverse, temp_depth]
                else:
                    _kmer_dict[kmer] = [1, 1023, 1, 0]
        infile.close()

def Get_Middle_Fragment(text, slice_len):
    """
    截取reads中间的高质量片段
    """
    start = (len(text) - slice_len) >> 1
    end = start + slice_len
    return text[start:end]

def Make_Reads_Dict(file_list, _reads_dict, Filted_File_Ext = '.fq'):
    """
    截取reads中间的片段，构建高质量的reads字典
    :param file_list: 文件列表
    :param _reads_dict: 待生成的字典value的格式为seq
    :return: 返回切片的长度
    """
    read_len = 0
    slice_len = 0
    for file in file_list:
        infile = open(file, 'r', encoding='utf-8', errors='ignore')
        infile.readline()
        paried_count = True
        for line in infile:
            temp_str = []
            if Filted_File_Ext == '.fasta':
                while line and line[0] != '>':
                    temp_str.append(line)
                    line = infile.readline()
            else:
                temp_str.append(line.strip())
                infile.readline()
                infile.readline()
                infile.readline()
            read_seq = ''.join(filter(str.isalpha, ''.join(temp_str))).upper()
            if not read_len:
                read_len = len(read_seq)
                slice_len = int(read_len * 0.9)
            intseqs = [read_seq, Reverse_Complement_ACGT(read_seq)] # 加入反向互补序列
            for x in intseqs:
                slice_reads = Get_Middle_Fragment(x, slice_len)
                if slice_reads in _reads_dict:
                    _reads_dict[slice_reads] += 1
                else:
                    _reads_dict[slice_reads] = 1
        infile.close()
    return slice_len

def Median(x):
    """
    使用中位数分割列表
    :return: 左半边列表，右半边列表，中位数
    """ 
    x = sorted(x)
    length = len(x)
    mid, rem = divmod(length, 2)    # divmod函数返回商和余数
    if rem:
        return x[:mid], x[mid+1:], x[mid]
    else:
        return x[:mid], x[mid:], (x[mid-1]+x[mid])/2

def Quartile(x):
    """
    获取列表的四分位数
    :return: 左四分位数，中位数，右四分位数，最大值加1
    """ 
    lHalf, rHalf, q2 = Median(x)
    return Median(lHalf)[2], q2, Median(rHalf)[2], max(x) + 1

def Get_Weight(_pos, new_pos, weight = 4):
    """
    距离和权重的关系模型，默认权重值为16，最高权重值为256，最低为0
    :param _pos: 当前kmer在参考序列的位置
    :param new_pos: 新的kmer位置
    :param weight: 默认权重
    :return: 返回计算后的权重
    """ 
    return int.bit_length((1024 - abs(_pos - new_pos)) >> 2) if (_pos and new_pos) else weight

def Get_Forward_Contig_v6(_dict, seed, kmer_size, iteration = 1024):
    """
    带权重的DBG贪婪拼接
    :param _pos: 当前kmer在参考序列的位置
    :param seed: 新的kmer位置
    :param kmer_size: kmer的大小
    :param iteration: 最大循环数量
    :param weight: 默认权重
    :return: best_seq, kmer_set, best_kmc, best_pos, best_snp
    """ 
    temp_list, kmer_set, stack_list, pos_list = [seed], set([seed]), [], []
    temp_dict = Counter(temp_list)
    cur_kmc, cur_seq, contigs = deque(), deque(), []
    _pos, node_distance, best_kmc_sum = 0, 0, 0
    MASK = (1 << ((kmer_size << 1) - 2)) - 1
    while iteration:
        next_kmer = (temp_list[-1] & MASK) << 2
        node = [(i, _dict[i][1], _dict[i][0] + _dict[i][3])
                for i in range(next_kmer, next_kmer + 4)
                if i in _dict and not temp_dict[i]]
        node.sort(key=itemgetter(2), reverse=True)
        if not node: 
            iteration -= 1
            cur_kmc_sum = sum(cur_kmc)
            contigs.append((cur_kmc.copy(), cur_seq.copy()))
            if cur_kmc_sum > best_kmc_sum:
                best_kmc_sum = cur_kmc_sum
            for _ in range(node_distance):
                temp_dict[temp_list.pop()] -= 1
                cur_kmc.pop()
                cur_seq.pop()
            if not stack_list:
                break
            node, node_distance, _pos = stack_list.pop()
        if len(node) >= 2:
            stack_list.append((node[1:], node_distance, _pos))
            node_distance = 0
        if node[0][1] > 0: _pos = node[0][1]
        temp_list.append(node[0][0])
        temp_dict[node[0][0]] += 1
        kmer_set.add(node[0][0])
        pos_list.append(node[0][1])
        cur_kmc.append(node[0][2])
        cur_seq.append(node[0][0] & 3)
        node_distance += 1
    return contigs, kmer_set, pos_list, int(best_kmc_sum)

def find_position(dq, n):
    for i in range(len(dq) - 1, -1, -1):
        if dq[i] >= n:
            return i
    return -1

def Process_Contigs(contigs, max_weight, slice_len, reads_dict, soft_boundary = 0):
    """
    通过将contigs与reads进行map，来检测contig的可靠性
    :param contigs: 拼接过程获取的contigs
    :param max_weight: 最大的权重，只考虑大于最大权重一半的contigs
    :param slice_len: reads的高质量切片的长度
    :param reads_dict: reads的高质量切片的词典
    :return: 按照map上的reads的数量倒序排序过后的contigs
    """ 
    # 基于soft_boundary和四分位点切割序列两端
    for i, contig in enumerate(contigs):
        if len(contig[0]) > 2:
            cut_value = Quartile(contig[0])[0]
            cut_pos = find_position(contig[0], cut_value)
            if cut_pos != -1 and cut_pos + soft_boundary + 1 < len(contig[0]):
                while len(contig[0]) > cut_pos + soft_boundary + 1:
                    contig[0].pop()
                    contig[1].pop()

    processed_contigs = sorted([[''.join(ACGT_DICT[k] for k in x[1]), sum(x[0]), 0] for x in contigs if sum(x[0]) > max_weight >> 1], key=itemgetter(1), reverse=True)
    for x in processed_contigs:
        contig_len = len(x[0])
        for j in range(contig_len - slice_len):
            if contig_len - slice_len - j >= 0:
                slice_str = x[0][contig_len - slice_len - j:contig_len - j]
                if slice_str in reads_dict:
                    x[2] += reads_dict[slice_str]
    processed_contigs.sort(key=itemgetter(2), reverse=True)
    return processed_contigs


def Get_Contig_v6(_reads_dict, slice_len, _dict, seed, kmer_size, iteration = 1024, soft_boundary = 0):
    """
    获取最优的contig
    :param _reads_dict: reads的高质量切片的词典
    :param slice_len: reads的高质量切片的长度
    :param _dict: 用于拼接的kmer字典
    :param seed: 拼接种子
    :param kmer_size: kmer的长度
    :param iteration: 构建contig时允许的最大路径分支数
    :param weight: 没有ref时的默认权重
    :return: contigs的集合，用到所有的kmer的集合，contig的大概位置
    """ 
    contigs_1, kmer_set_1, pos_list_1, weight_1 = Get_Forward_Contig_v6(_dict, seed, kmer_size, iteration)
    contigs_2, kmer_set_2, pos_list_2, weight_2 = Get_Forward_Contig_v6(_dict, Reverse_Int(seed, kmer_size), kmer_size, iteration)
    # 清理位置列表
    pos_list = [x for x in chain(pos_list_1, pos_list_2) if x > 0 and x < 1000]
    # 获取位置中位数
    contig_pos = int(Quartile(pos_list)[1] if len(pos_list)>1 else -1)
    # 获取最可能的两侧的contig
    contigs_1_16 = Process_Contigs(contigs_1, weight_1, slice_len, _reads_dict, soft_boundary)
    contigs_2_16 = Process_Contigs(contigs_2, weight_2, slice_len, _reads_dict, soft_boundary)
    processed_contigs = []
    if not contigs_1_16: contigs_1_16.append(['',0,0])
    if not contigs_2_16: contigs_2_16.append(['',0,0])
    # 对最多前9种组合计算数量
    for l in contigs_2_16[:3]:
        for r in contigs_1_16[:3]:
            c = Reverse_Complement_ACGT(l[0]) + Int_To_Seq(seed, kmer_size) + r[0]
            c_weight = l[1] + r[1]
            contig_len = len(c)
            r_count = 0
            for j in range(contig_len - slice_len):
                if contig_len - slice_len - j >= 0:
                    slice_str = c[contig_len - slice_len - j:contig_len - j]
                    if slice_str in _reads_dict:
                        r_count += _reads_dict[slice_str]
            # 序列，序列的拼接权重，切片数
            processed_contigs.append([c, c_weight, r_count])
    return processed_contigs, kmer_set_1 | kmer_set_2, contig_pos

def Calculate_Kmer_Size(ref_path, reads, slice_len, k_min, k_max, error_limit):
    mask_bin  = (1 << (k_min << 1)) - 1
    kmer_dict = Counter()
    trans = FWD_TRANS
    rtrans = REV_TRANS

    if slice_len <= k_min:
        return k_min

    if k_min % 2 == 0:
        k_min += 1

    for seq in reads:
        seq_str   = seq.translate(trans)
        seq_str_r = seq.translate(rtrans)[::-1]

        if not seq_str:
            continue

        seq_int   = int(seq_str, 4)
        seq_int_r = int(seq_str_r, 4)

        for _ in range(0, len(seq_str) - k_min + 1):
            kmer_dict[seq_int   & mask_bin] += 1
            kmer_dict[seq_int_r & mask_bin] += 1
            seq_int   >>= 2
            seq_int_r >>= 2

    kmer_dict = {k for k, v in kmer_dict.items() if v > error_limit}
    run_length_stats = [0] * (k_max - k_min + 1)
    run_maximum = k_max - k_min + 1

    with open(ref_path, 'r') as f:
        for _, seq in SimpleFastaParser(f):
            seq       = ''.join(filter(str.isalpha, seq)).upper()
            seq_str   = seq.translate(trans)

            if not seq_str:
                continue

            seq_int   = int(seq_str, 4)

            run_length_list = [0]

            for _ in range(0, len(seq_str) - k_min + 1):
                if (seq_int & mask_bin) in kmer_dict:
                    run_length_list[-1] += 1
                    if run_length_list[-1] >= run_maximum:
                        run_length_list.append(run_maximum // 2)
                elif run_length_list[-1] != 0:
                    run_length_list.append(0)
                seq_int >>= 2

            for k, v in Counter(run_length_list).items():
                if k == 0:
                    continue

                kp = k - 1
                odd = kp % 2
                kp = kp - odd

                run_length_stats[kp] += v

                for i in range(2, kp + 1, 2):
                    run_length_stats[kp - i] += v

    for k, n in reversed(tuple(enumerate(run_length_stats, k_min))):
        if n > 0:
            upper_bound = k
            break
    else:
        return k_min

    lower_bound = (upper_bound + 1) // 2

    k_list = [(k, n * k / (slice_len - k + 1) if slice_len > k else 0)
              for k, n in enumerate(run_length_stats, k_min)
              if lower_bound < k <= upper_bound]
    cutoff = max(map(itemgetter(1), k_list)) / 2

    for k, cov in reversed(k_list):
        if cov > cutoff:
            return k
    else:
        return k_min

def Write_Dict(_dict, file_name):
    """
    写入16进制kmer字典
    :param _dict: kmer字典
    :param file_name: 文件名，
    :return: 成功返回1
    """
    with open(file_name,'w') as f:
        for key, value in _dict.items():
            if type(value) == list:
                f.writelines([str(key), ",", ",".join(map(str,value)), ',', '\n'])
            else:
                f.writelines([str(key), ",", str(value), ",", '\n'])

def process_key_value(args, key, ref_path, ref_count, iteration, soft_boundary, loop_count, total_count):
    contig_best_path = os.path.join(args.o, "results", key + ".fasta")
    contig_all_path = os.path.join(args.o, "contigs_all", key + ".fasta")
    current_ka = args.ka
    limit = args.limit_count

    if os.path.isfile(contig_best_path):
        return False, key, {"status": "skipped"}

    open(contig_best_path, 'w').close()

    # 检查是哪种扩展名
    file_extensions = ['.fasta', '.fq']
    Filted_File_Ext = '.fq'
    filtered_file_path = None
    for ext in file_extensions:
        file_path = os.path.join(args.o, 'filtered', key + ext)
        if os.path.exists(file_path):
            filtered_file_path = file_path
            Filted_File_Ext = ext
            break

    # 清理文件
    if not os.path.isfile(os.path.join(args.o, 'filtered', key + Filted_File_Ext)):
        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
        return False, key, {"status": "no filtered file", "value": 0}

    # 获取种子列表
    ref_dict, filtered_dict, reads_dict = {}, {}, {}

    # 获取最大切片长度，建立reads切片字典
    slice_len = Make_Reads_Dict([filtered_file_path], reads_dict)

    if not reads_dict:
        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
        Write_Print(os.path.join(args.o,  "log.txt"), "No reads were obtained for gene", key)
        return False, key, {"status": "no reads", "value": 0}

    # 自动调整soft_boundary
    if soft_boundary == -1:
        soft_boundary = slice_len // 2

    # 如果不指定ka, 估算最大ka，执行动态高精度拼接
    if not current_ka:
        current_ka = Calculate_Kmer_Size(ref_path, reads_dict.keys(), slice_len, args.k_min, args.k_max, limit)

    Write_Print(os.path.join(args.o,  "log.txt"), "Use k=", current_ka, " for assembling gene ", key ,".", sep='')
    Write_Print(os.path.join(args.o,  "log.txt"), 'Assembling', key, loop_count, '/', total_count)

    # 制作参考序列的kmer字典
    Make_Kmer_Dict(ref_dict, ref_path, current_ka)
    # 制作用于拼接的kmer字典
    Make_Assemble_Dict([filtered_file_path], current_ka, filtered_dict, ref_dict)
    # 缩减filtered_dict，保留大于limit和有深度信息的
    if limit > 0:
        filtered_dict = {k: v for k, v in filtered_dict.items() if v[0] > limit or v[3] > 0}

    if len(filtered_dict) < 3:
        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
        Write_Print(os.path.join(args.o,  "log.txt"), 'Could not get enough reads from filter.')
        return False, key, {"status": "insufficient genomic kmers", "value": 0}

    # 纠正深度上限, 获取参考序列的深度修正权重
    # filtered_dict[0排除了上限的过滤深度，1位置，2方向，3修正参考序列深度]
    read_quar = Quartile([v[0] for v in filtered_dict.values()])
    depth_upper = int((read_quar[2] - read_quar[0]) * 1.5 + read_quar[2])
    for k, v in filtered_dict.items():
        if v[3] != 0:
            v[3] = (v[0] > limit) * int(ref_count / (abs(v[3] - ref_count) + 1) * depth_upper) + 1
        v[0] = min(v[0], depth_upper)

    # 在每个参考序列中出现且只出现一次的kmer优先作为种子
    # 长度位置在1~1000之间，与参考序列方向一致v[2] == 0
    seed_list = [(k, v[0], v[1], v[3]) for k, v in filtered_dict.items() if v[1] > 1 and v[1] < 1000 and not v[2]]
    seed_list.sort(key=lambda x: (x[3], x[1]), reverse=True)

    # 必须有seed_list, 否则意味着跟参考序列差别过大
    if not seed_list:
        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
        Write_Print(os.path.join(args.o,  "log.txt"), 'Could not get enough seeds.')
        return False, key, {"status": "no seed", "value": 0}

    # 获取seed集合，用来加速集合操作
    seed_list_len = len(seed_list)
    seed_set = set(i[0] for i in seed_list)

    # 获取contigs
    contigs_all = []
    contigs_all_low = []
    contigs_best = []

    while len(seed_list) > seed_list_len * 0.5: # 已经耗费了大于一半的seed就没必要再做了 
        # org_contigs: 0序列 1序列的拼接权重 2切片数 3配对的切片数
        org_contigs, kmer_set, contig_pos = Get_Contig_v6(reads_dict, slice_len, filtered_dict, seed_list[0][0], current_ka, iteration=iteration, soft_boundary=soft_boundary)
        seed_list = [item for item in seed_list if (item[0] not in kmer_set) and (Reverse_Int(item[0], current_ka) not in kmer_set)]
        for contig in org_contigs:
            if contig[2] * slice_len > len(contig[0]): # 起码要有reads高质量切片能够覆盖contig，否则就是错误的拼接
                # contigs_all: 0序列 1使用的种子数量 2序列位置 3序列的拼接权重 4切片数
                contigs_all.append([contig[0], len(seed_set & kmer_set), contig_pos, contig[1], contig[2]])
            else:
                contigs_all_low.append([contig[0], len(seed_set & kmer_set), contig_pos, contig[1], contig[2]])

    low_qual = not contigs_all
    if low_qual:
        contigs_all = contigs_all_low

    # 设计规则重新排序，此处使用切片数排序
    # 排序第一位作为 best contig
    if contigs_all:
        contigs_all.sort(key=lambda x: (x[4], x[3]), reverse=True)
        contigs_best.append(contigs_all[0])
    else:
        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
        Write_Print(os.path.join(args.o, "log.txt"), "Insufficient reads coverage, unable to build contigs.")
        return False, key, {"status": "no contigs", "value": 0}

    with open(contig_best_path, 'w') as out:
        for x in contigs_best:
            out.write(f'>contig_{len(x[0])}_{x[1]}_{x[2]}_{x[3]}_{x[4]}\n')
            out.write(x[0] + '\n')
    with open(contig_all_path, 'w') as out:
        for x in contigs_all:
            out.write(f'>contig_{len(x[0])}_{x[1]}_{x[2]}_{x[3]}_{x[4]}\n')
            out.write(x[0] + '\n')

    ref_dict, filtered_dict = None, None
    gc.collect()
    return True, key, {"status": "low quality" if low_qual else "success", "value": contigs_best[0][4]}

if __name__ == '__main__':
    if sys.platform.startswith('win'):
        multiprocessing.freeze_support()

    pars = argparse.ArgumentParser(formatter_class=argparse.RawDescriptionHelpFormatter, description='''Assembler by YY 20230314''')
    pars.add_argument('-r', metavar='<str>', type=str, help='''input ref file or dir''')
    pars.add_argument('-o', metavar='<str>', type=str, help='''out dir''')
    pars.add_argument('-ka', metavar='<int>', type=int, help='''kmer of assemble''',  default=39)
    pars.add_argument('-k_max', metavar='<int>', type=int, help='''max kmer of assemble''',  default=39)
    pars.add_argument('-k_min', metavar='<int>', type=int, help='''max kmer of assemble''',  default=21)
    pars.add_argument('-limit_count', metavar='<int>', type=int, help='''limit of kmer count''', required=False, default=2)
    pars.add_argument('-iteration', metavar='<int>', type=int, help='''iteration''', required=False, default=8192)
    pars.add_argument('-sb', '--soft_boundary', metavar='<int>', type=int, help='''soft boundary，default = [0], -1时为切片长度的一半''', required=False, default=0)
    pars.add_argument('-p', '--processes', metavar='<int>', type=int, help='Number of processes for multiprocessing', default= 1)#max(multiprocessing.cpu_count()-1,2))
    args = pars.parse_args()

    try:
        # 初始化文件夹
        if not os.path.isdir(os.path.join(args.o, 'results')):
            os.mkdir(os.path.join(args.o, 'results'))
        if not os.path.isdir(os.path.join(args.o, 'contigs_all')):
            os.mkdir(os.path.join(args.o, 'contigs_all'))
        print("Do not close this window manually, please!")
        # 载入参考序列信息
        Get_Ref_Info(args.r, ref_path_dict, ref_count_dict)
        t0 = time.time()
    except Exception as e:
        Write_Print(os.path.join(args.o,  "log.txt"), "error:" , e)

    try:
        Write_Print(os.path.join(args.o,  "log.txt"), '======================== Assemble =========================')

        results = []
        if args.processes > 1:
            pool = multiprocessing.Pool(args.processes)
            for loop_count, (key, ref_path) in enumerate(ref_path_dict.items(), start=1):
                results.append(pool.apply_async(process_key_value, (args, key, ref_path, ref_count_dict[key], args.iteration, args.soft_boundary, loop_count, len(ref_path_dict))))
            pool.close()
            pool.join()
        else:
            for loop_count, (key, ref_path) in enumerate(ref_path_dict.items(), start=1):
                results.append(process_key_value(args, key, ref_path, ref_count_dict[key], args.iteration, args.soft_boundary, loop_count, len(ref_path_dict)))

        result_dict = {}
        for result in results:
            success, key_update, result_dict_entry = result if type(result) == tuple else result.get()
            if result_dict_entry.get("status") != "skipped":
                result_dict[key_update] = [result_dict_entry["status"], result_dict_entry["value"]]

        Write_Dict(result_dict, os.path.join(args.o, "result_dict.txt"))
        t1 = time.time()
        Write_Print(os.path.join(args.o,  "log.txt"), '\nTime cost:', t1 - t0, '\n') # 拼接所用的时间
    except Exception as e:
        Write_Print(os.path.join(args.o,  "log.txt"), "error:" , e)
