from collections import deque
from operator import itemgetter
import gc
from collections import Counter
from concurrent.futures import ProcessPoolExecutor
from overlap import calculate_overlap_wrapper
import multiprocessing
import numpy as np
from Bio import SeqIO
from win_filter import * 

# pars.add_argument('-ta', metavar='<int>', type=int, help='''thread of assemble''', required=False, default= 1)
# 简并碱基字典
D_BASE_DICT = {'AG':'R','CT':'Y', 'GT':'K', 'GC':'S','AC':'M', 'AT':'W','GA':'R','TC':'Y','TG':'K', 'CG':'S','CA':'M', 'TA':'W',}


def find_max_overlap(long_dnas, short_dnas, min_overlap=20, step=1, max_overlap_limit=None):
    """
    查找最大重叠片段
    """ 
    short_dnas_array = np.array([[ord(c) for c in short_dna] for short_dna in short_dnas], dtype=np.uint32)
    max_overlap = 0
    for long_dna in long_dnas:
        long_dna_array = np.array([ord(c) for c in long_dna], dtype=np.uint32)
        if max_overlap_limit is None:
            max_overlap_limit = short_dnas_array.shape[1]
        with ProcessPoolExecutor() as executor:
            for overlap in executor.map(calculate_overlap_wrapper, [(long_dna_array, short_dnas_array, min_overlap, step, max_overlap_limit)]):
                temp_max_overlap = max(max_overlap, overlap)
                if max_overlap < temp_max_overlap:
                    max_overlap = temp_max_overlap
                    if max_overlap >= max_overlap_limit:
                        return max_overlap
                    elif max_overlap > min_overlap:
                        min_overlap = max_overlap
    return max_overlap

def consensus_sequence(multi_alignment):
    """
    根据排序后的序列生成一致序列
    """ 
    num_sequences = len(multi_alignment)
    seq_length = len(multi_alignment[0])
    consensus = []
    for column in range(seq_length):
        base_count = Counter(multi_alignment[row][column] for row in range(1, num_sequences) if multi_alignment[row][column] != '-')
        if base_count:
            consensus.append(base_count.most_common(1)[0][0])
        else:
            consensus.append('?')
    return ''.join(consensus)

def Reverse_Complement_ACGT(seq):
    """
    简化版反向互补
    """ 
    return seq.translate(str.maketrans('ACGT', 'TGCA'))[::-1]

def Make_Assemble_Dict(file_list, kmer_size, _kmer_dict, _ref_dict):
    """
    构建拼接用的字典
    :param file_list: 文件列表
    :param kmer_size: kmer的长度
    :param _kmer_dict: 待生成的字典value的格式为[深度，位置（1000以内的整数）]
    :param _ref_dict: 参考序列的字典
    :return: 返回kmer的总数量
    """
    MASK_BIN = (1<< (kmer_size<<1)) - 1 # kmer的掩码
    kmer_count = 0
    for file in file_list:
        infile = open(file, 'r', encoding='utf-8', errors='ignore')
        infile.readline()
        for line in infile:
            temp_str = [] # 为了支持多行的fasta文件作为源数据
            if Filted_File_Ext == '.fasta':
                while line and line[0] != '>':
                    temp_str.append(line)
                    line = infile.readline()
            else:
                temp_str.append(line)
                infile.readline()
                infile.readline()
                infile.readline()
            read_seq = ''.join(filter(str.isalpha, ''.join(temp_str).upper()))
            intseqs, read_len = Seq_To_Int(read_seq) # 序列转整数，获取长度
            intseqs.append(Seq_To_Int(read_seq, True)) # 加入反向互补序列
            for x in intseqs:
                kmer_count += read_len - kmer_size + 1
                for j in range(0, read_len - kmer_size):
                    temp_list, temp_pos,  kmer = [], 0, x >> (j<<1) & MASK_BIN
                    if kmer in _kmer_dict:
                        _kmer_dict[kmer][0] += 1
                    else:
                        if kmer in _ref_dict: # kmer的位置
                            if  _ref_dict[kmer] & 1073741824: # 判断是否为反向互补的序列
                                temp_pos = 1000 - (_ref_dict[kmer] & 1023)
                                temp_list = [1, temp_pos, 1] # 标记为反向的的kmer
                            else: 
                                temp_pos = _ref_dict[kmer] & 1023
                                temp_list = [1, temp_pos, 0] 
                        else:
                            temp_list = [1, 1023, 1] 
                        _kmer_dict[kmer] = temp_list
        infile.close()
    return kmer_count

def Get_Closest_Power_Of_Two(num):
    """
    获取小于正整数的最接近的2的整数次方
    """
    power = 1
    while power <= num:
        power <<= 1
    return power >> 1

def Get_Middle_Fragment(text, slice_len):
    """
    截取reads中间的高质量片段
    """
    start = (len(text) - slice_len) >> 1
    end = start + slice_len
    return text[start:end]

def Make_Reads_Dict(file_list, _reads_dict, not_Paired = False):
    """
    截取reads中间的片段，构建高质量的reads字典
    :param file_list: 文件列表
    :param _reads_dict: 待生成的字典value的格式为[seq, count]
    :return: 返回切片的长度
    """
    read_len = 0
    slice_len = 0
    for file in file_list:
        infile = open(file, 'r', encoding='utf-8', errors='ignore')
        infile.readline()
        reads_count = 0
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
            if not_Paired or paried_count:
                reads_count += 1
            paried_count = not paried_count
            read_seq = ''.join(filter(str.isalpha, ''.join(temp_str).upper()))
            if not read_len:
                read_len = len(read_seq)
                slice_len = int(read_len * 0.9) #Get_Closest_Power_Of_Two(read_len)
            intseqs = [read_seq, Reverse_Complement_ACGT(read_seq)] # 加入反向互补序列
            for x in intseqs:
                slice_reads = Get_Middle_Fragment(x, slice_len)
                if slice_reads in _reads_dict:
                    _reads_dict[slice_reads][0] += 1
                else:
                    _reads_dict[slice_reads] = [1, reads_count]
        infile.close()
    return slice_len

def Get_Ref_List(file_list):
    """
    构建参考序列字符串列表
    :param file_list: 参考序列文件列表
    :return: 返回参考序列列表
    """
    ref_seqs = []
    for file in file_list:
        infile = open(file, 'r', encoding='utf-8', errors='ignore')
        # 第一条参考序列的名字
        ref_name = infile.readline()
        for line in infile:
            # 读取每一条参考序列
            temp_str = []
            while line and line[0] != '>':
                temp_str.append(line)
                line = infile.readline()
            ref_seqs.append(''.join(filter(str.isalpha, ''.join(temp_str).upper())))
        infile.close()
    return ref_seqs


def Get_Similar_Ref(file_list, kmer_size, _reads_set_combine):
    """
    构建拼接用的字典
    :param file_list: 参考序列文件列表
    :param kmer_size: kmer的长度
    :param _reads_set_combine: reads的集合
    :return: 返回最近源的参考序列
    """
    MASK_BIN = (1<< (kmer_size<<1)) - 1 # kmer的掩码
    Similar_Ref, ref_kmer_count = ('',0,''),  0
    for file in file_list:
        infile = open(file, 'r', encoding='utf-8', errors='ignore')
        # 第一条参考序列的名字
        ref_name = infile.readline()
        for line in infile:
            # 读取每一条参考序列
            temp_str = []
            while line and line[0] != '>':
                temp_str.append(line)
                line = infile.readline()
            
            ref_seq = ''.join(filter(str.isalpha, ''.join(temp_str).upper()))
            intseqs, read_len = Seq_To_Int(ref_seq) # 序列转整数，获取长度
            intseqs.append(Seq_To_Int(ref_seq, True)) # 加入反向互补序列
            # 参考序列的kmer在集合中的计数
            kmer_count = 0
            for x in intseqs:
                for j in range(0, read_len-kmer_size):
                    kmer = x >> (j<<1) & MASK_BIN
                    if kmer in _reads_set_combine:
                        kmer_count += 1
            if ref_kmer_count < kmer_count:
                 ref_kmer_count = kmer_count
                 Similar_Ref = (intseqs[0], read_len, ref_name)
            # 参考序列的名字
            if line[0] == '>': ref_name = line
        infile.close()
    return Similar_Ref[2], Int_To_Seq(Similar_Ref[0], Similar_Ref[1])


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


def Forward_Bin(seq_int, mask):
    """
    正向的迭代器
    """ 
    for x in (0,1,2,3): yield ((seq_int & mask) << 2) + x

def Get_Weight(_pos, new_pos, weight = 4):
    """
    距离和权重的关系模型，默认权重值为16，最高权重值为256，最低为0
    :param _pos: 当前kmer在参考序列的位置
    :param new_pos: 新的kmer位置
    :param weight: 默认权重
    :return: 返回计算后的权重
    """ 
    return int.bit_length((1024 - abs(_pos - new_pos)) >> 2) if (_pos and new_pos) else weight


def Get_Forward_Contig_v6(_dict, seed, kmer_size, iteration = 1024, weight = 4):
    """
    带权重的DBG贪婪拼接
    :param _pos: 当前kmer在参考序列的位置
    :param seed: 新的kmer位置
    :param kmer_size: kmer的大小
    :param iteration: 最大循环数量
    :param weight: 默认权重
    :return: best_seq, kmer_list, best_kmc, best_pos, best_snp
    """ 
    temp_list, kmer_list, stack_list, pos_list = deque([seed]), deque([seed]), deque(), deque()
    cur_kmc, cur_seq, contigs = deque(), deque(), deque()
    _pos, node_distance, best_kmc_sum  = 0, 0, 0
    MASK = (1 << ((kmer_size << 1) - 2)) - 1
    while True and iteration:
        node = [[i, _dict[i][1], _dict[i][0] << Get_Weight(_pos, _dict[i][1], weight), ACGT_DICT[i & 3]] for i in Forward_Bin(temp_list[-1], MASK) if i in _dict]
        node.sort(key = itemgetter(2), reverse=True)
        while node and node[0][0] in temp_list: node.pop(0) 
        if not node: 
            iteration -= 1
            cur_kmc_sum = sum(cur_kmc)
            contigs.append((cur_kmc_sum, cur_seq.copy()))
            if cur_kmc_sum > best_kmc_sum:
                best_kmc_sum = cur_kmc_sum
            for _ in range(node_distance):
                temp_list.pop()
                cur_kmc.pop()
                cur_seq.pop()
            if stack_list: 
                node, node_distance, _pos = stack_list.pop()
            else: 
                break
        if len(node) >= 2:
            stack_list.append((node[1:], node_distance, _pos))
            node_distance = 0
        if node[0][1] > 0: _pos = node[0][1]
        temp_list.append(node[0][0])
        kmer_list.append(node[0][0])
        pos_list.append(node[0][1])
        cur_kmc.append(node[0][2])
        cur_seq.append(node[0][0]&3)
        node_distance += 1
    return contigs, kmer_list, pos_list, best_kmc_sum

def count_duplicates(numbers):
    count_dict = {}
    for number in numbers:
        if number in count_dict:
            count_dict[number] += 1
        else:
            count_dict[number] = 1
    
    duplicates_count = 0
    for count in count_dict.values():
        if count == 2:
            duplicates_count += 1
    
    return duplicates_count

def Process_Contigs(contigs, max_weight, slice_len, reads_dict):
    """
    通过将contigs与reads进行map，来检测contig的可靠性
    :param contigs: 拼接过程获取的contigs
    :param max_weight: 最大的权重，只考虑大于最大权重一半的contigs
    :param slice_len: reads的高质量切片的长度
    :param reads_dict: reads的高质量切片的词典
    :return: 按照map上的reads的数量倒序排序过后的contigs
    """ 
    processed_contigs = sorted([[''.join(ACGT_DICT[k] for k in x[1]), x[0], 0] for x in contigs if x[0] > max_weight >> 1], key=itemgetter(1), reverse=True)
    for x in processed_contigs:
        contig_len = len(x[0])
        for j in range(contig_len - slice_len):
            if contig_len - slice_len - j >= 0:
                slice_str = x[0][contig_len - slice_len - j:contig_len - j]
                if slice_str in reads_dict:
                    x[2] += reads_dict[slice_str][0]
    processed_contigs.sort(key=itemgetter(2), reverse=True)
    return processed_contigs

def Get_Contig_v5(_reads_dict, slice_len, _dict, seed, kmer_size, iteration = 1024, weight = 4):
    """
    获取最优的contig
    :param _reads_dict: reads的高质量切片的词典
    :param slice_len: reads的高质量切片的长度
    :param _dict: 用于拼接的kmer字典
    :param seed: 拼接种子
    :param kmer_size: kmer的长度
    :param iteration: 构建contig时允许的最大路径分支数
    :param weight: 没有ref时的默认权重
    :return: 最优contigs，用到所有的kmer的集合，contig的大概位置，contig的权重，map到contig上的reads数量
    """ 
    contigs_1, kmer_list_1, pos_list_1, weight_1 = Get_Forward_Contig_v6(_dict, seed, kmer_size, iteration, weight)
    contigs_2, kmer_list_2, pos_list_2, weight_2 = Get_Forward_Contig_v6(_dict, Reverse_Int(seed, kmer_size), kmer_size, iteration, weight)
    # 清理位置列表
    pos_list = [x for x in pos_list_1+ pos_list_2 if x > 0 and x < 1000]
    # 获取位置中位数
    contig_pos = int(Quartile(pos_list)[1] if len(pos_list)>1 else -1)
    # 获取最可能的两侧的contig
    contigs_1_16 = Process_Contigs(contigs_1, weight_1, slice_len, _reads_dict)
    contigs_2_16 = Process_Contigs(contigs_2, weight_2, slice_len, _reads_dict)
    # 组合contig
    contig = (Reverse_Complement_ACGT(contigs_2_16[0][0]) if contigs_2_16 else '') + Int_To_Seq(seed, kmer_size) + (contigs_1_16[0][0] if contigs_1_16 else '')
    contig_weight = contigs_1_16[0][1] if contigs_1_16 else 0 + contigs_2_16[0][1] if contigs_2_16 else 0
    contig_count = contigs_1_16[0][2] if contigs_1_16 else 0 + contigs_2_16[0][2] if contigs_2_16 else 0
    return contig, set(kmer_list_1 + kmer_list_2), contig_pos, contig_weight, contig_count

def Get_Contig_v6(_reads_dict, slice_len, _dict, seed, kmer_size, iteration = 1024, weight = 4):
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
    contigs_1, kmer_list_1, pos_list_1, weight_1 = Get_Forward_Contig_v6(_dict, seed, kmer_size, iteration, weight)
    contigs_2, kmer_list_2, pos_list_2, weight_2 = Get_Forward_Contig_v6(_dict, Reverse_Int(seed, kmer_size), kmer_size, iteration, weight)
    # 清理位置列表
    pos_list = [x for x in pos_list_1+ pos_list_2 if x > 0 and x < 1000]
    # 获取位置中位数
    contig_pos = int(Quartile(pos_list)[1] if len(pos_list)>1 else -1)
    # 获取最可能的两侧的contig
    contigs_1_16 = Process_Contigs(contigs_1, weight_1, slice_len, _reads_dict)
    contigs_2_16 = Process_Contigs(contigs_2, weight_2, slice_len, _reads_dict)
    processed_contigs = []
    if not contigs_1_16: contigs_1_16.append(['',0,0])
    if not contigs_2_16: contigs_2_16.append(['',0,0])
    # 对最多前4种组合计算paired数量
    for l in contigs_2_16[:2]:
        for r in contigs_1_16[:2]:
            c = Reverse_Complement_ACGT(l[0]) + Int_To_Seq(seed, kmer_size) + r[0]
            c_weight = l[1] + r[1]
            contig_len = len(c)
            paired_count = deque()
            r_count = 0
            for j in range(contig_len - slice_len):
                if contig_len - slice_len - j >= 0:
                    slice_str = c[contig_len - slice_len - j:contig_len - j]
                    if slice_str in _reads_dict:
                        r_count += _reads_dict[slice_str][0]
                        paired_count.append(_reads_dict[slice_str][1])
            # 序列，序列的拼接权重，切片数，配对的切片数
            processed_contigs.append([c, c_weight, r_count, count_duplicates(paired_count)])
    # max_paired = max(x[3] for x in processed_contigs)
    # processed_contigs = [x for x in processed_contigs if x[3] >= max_paired * 0.9 ]
    return processed_contigs, set(kmer_list_1 + kmer_list_2), contig_pos

if __name__ == '__main__':
    try:
        if sys.platform.startswith('win'):
            multiprocessing.freeze_support()
        pars.add_argument('-ka', metavar='<int>', type=int, help='''kmer of assemble''',  default=21)
        pars.add_argument('-k_max', metavar='<int>', type=int, help='''max kmer of assemble''',  default=63)
        pars.add_argument('-k_min', metavar='<int>', type=int, help='''max kmer of assemble''',  default=21)
        pars.add_argument('-limit_count', metavar='<int>', type=int, help='''limit of kmer count''', required=False, default=2)
        args = pars.parse_args()
        # 初始化文件夹
        if not os.path.isdir(os.path.join(args.o, 'results')):
            os.mkdir(os.path.join(args.o, 'results'))
        if not os.path.isdir(os.path.join(args.o, 'contigs_all')):
            os.mkdir(os.path.join(args.o, 'contigs_all'))
        print("Do not close this window manually, please!")

        # 载入参考序列信息
        if not Get_Ref_Info(args.r, ref_path_list, ref_path_dict, ref_length_dict):
            Write_Print(os.path.join(args.o,  "log.txt"), 'Invaild reference!')
            sys.exit(0)
        t0 = time.time()

        Write_Print(os.path.join(args.o,  "log.txt"), '======================== Assemble =========================')
        # 定义默认权重值
        cur_weight, assemble_count, failed_count  = 4, 0, 0
        result_dict = {}
        for key, value in ref_length_dict.items(): 
            limit = args.limit_count
            depth = 0
            assemble_count += 1
            contig_best_path = os.path.join(args.o, "results", key + ".fasta")
            contig_all_path = os.path.join(args.o, "contigs_all", key + ".fasta")
            if os.path.isfile(contig_best_path) == False:
                with open(contig_best_path, 'w') as out: pass
                write_contig = False
                Write_Print(os.path.join(args.o,  "log.txt"), 'Assembling', key, assemble_count,'/',len(ref_length_dict)," " * 32)
                # 检查是哪种扩展名
                file_extensions = ['.fasta', '.fq']
                Filted_File_Ext = '.fq'
                filtered_file_path = None
                for ext in file_extensions:
                    file_path = os.path.join(args.o,'filtered', key + ext)
                    if os.path.exists(file_path):
                        filtered_file_path = file_path
                        Filted_File_Ext = ext
                        break
                # 清理文件
                if os.path.isfile(os.path.join(args.o, 'filtered', key + Filted_File_Ext )) == False:
                    if os.path.isfile(contig_best_path): os.remove(contig_best_path)
                    if os.path.isfile(contig_all_path): os.remove(contig_all_path)
                    continue
                # 获取种子列表
                ref_dict, filtered_dict, reads_dict = {}, {}, {}
                # 获取最大切片长度，建立reads切片字典
                slice_len = Make_Reads_Dict([filtered_file_path], reads_dict)
                if not reads_dict:
                    failed_count += 1
                    if os.path.isfile(contig_best_path): os.remove(contig_best_path)
                    if os.path.isfile(contig_all_path): os.remove(contig_all_path)
                    Write_Print(os.path.join(args.o,  "log.txt"), "No reads were obtained for the", key)
                    result_dict[key] = "no reads"
                    continue
                # 构建当前参考序列列表
                # 如果不指定ka, 估算最大ka，执行动态高精度拼接
                current_ka = args.ka
                if not current_ka:
                    ref_list = Get_Ref_List([ref_path_dict[key]])
                    # 获取reads和参考序列的最大重叠长度
                    keys_with_slice_len = {key for key in reads_dict.keys() if len(key) == slice_len}
                    current_ka = find_max_overlap(ref_list, keys_with_slice_len, min_overlap = args.k_min, step = 8, max_overlap_limit = min((args.k_max + 2),slice_len-1))
                    if not current_ka:
                        failed_count += 1
                        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
                        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
                        Write_Print(os.path.join(args.o,  "log.txt"), "The reference sequence for the ", key, " may be too distantly related or the sequencing depth may be too low.", sep='')
                        result_dict[key] = "distant references"
                        continue
                    # 略微降低一个级别的k提供更好的兼容性
                    current_ka -= 2
                    Write_Print(os.path.join(args.o,  "log.txt"), "Use k=", current_ka, " for assembling the ", key ,".", sep='')
                # 制作参考序列的kmer字典
                Make_Kmer_Dict_v6(ref_dict, [ref_path_dict[key]], current_ka, True, True)
                # 制作用于拼接的kmer字典
                Make_Assemble_Dict([filtered_file_path], current_ka, filtered_dict, ref_dict)
                # 缩减filtered_dict，保留大于limit和有位置信息的
                if limit > 0:
                    filtered_dict = {k: v for k, v in filtered_dict.items() if v[0] > limit or v[1] > 0}
                # 必须有filtered_dict才能继续
                if not filtered_dict:
                    failed_count += 1
                    if os.path.isfile(contig_best_path): os.remove(contig_best_path)
                    if os.path.isfile(contig_all_path): os.remove(contig_all_path)
                    Write_Print(os.path.join(args.o,  "log.txt"), 'Could not get enough reads from filter. Estimate depth:', depth ,' '*16)
                    result_dict[key] = "insufficient reads"
                    continue
                # 处理ref_dict，标记不在filtered_dict中的kmer
                for i in ref_dict:
                    if i not in filtered_dict:
                        ref_dict[i] |= 1023  #前10位全部置1，用来代表没有位置信息
                # 如果用ref_dict做seed_list，主要考虑参考序列的保守区
                # 用filtered_dict做seed_list，主要考虑测序的高丰度区
                # 长度位置在10~990之间，与参考序列方向一致v[2] == 0
                seed_list = [(k, v[0], v[1]) for k, v in filtered_dict.items() if v[1]>10 and v[1]<990 and not v[2]]
                list.sort(seed_list, key=itemgetter(1), reverse=True)
                # 必须有seed_list, 否则意味着跟参考序列差别过大
                if not seed_list:
                    failed_count += 1
                    if os.path.isfile(contig_best_path): os.remove(contig_best_path)
                    if os.path.isfile(contig_all_path): os.remove(contig_all_path)
                    Write_Print(os.path.join(args.o,  "log.txt"), 'Could not get enough seeds. Estimate depth:', depth ," "*16)
                    result_dict[key] = "no seed"
                    continue
                # 获取seed集合，用来加速集合操作
                seed_list_len = len(seed_list)
                seed_set = set([i[0] for i in seed_list])
                # 获取contigs
                contigs_all = []
                contigs_all_low = []
                contigs_best = []
                # 获取contigs
                while len(seed_list) > seed_list_len >> 1: # 该子集耗费了大于一半的seed就没必要再做了 
                    # org_contigs: 0序列 1序列的拼接权重 2切片数 3配对的切片数
                    org_contigs, kmer_set, contig_pos = Get_Contig_v6(reads_dict, slice_len, filtered_dict, seed_list[0][0], current_ka, iteration = 4096, weight = cur_weight)
                    seed_list = [item for item in seed_list if (item[0] not in kmer_set) and (Reverse_Int(item[0], current_ka) not in kmer_set)]
                    for contig in org_contigs:
                        if contig[2] * slice_len > len(contig[0]): # 起码要有reads高质量切片能够覆盖contig，否则就是错误的拼接
                            # contigs_all: 0序列 1使用的种子数量 2序列位置 3序列的拼接权重 4切片数 5配对的切片数
                            contigs_all.append([contig[0], len(seed_set & kmer_set), contig_pos, contig[1], contig[2], contig[3]])
                        else:
                            contigs_all_low.append([contig[0], len(seed_set & kmer_set), contig_pos, contig[1], contig[2], contig[3]])
                # 设计规则重新排序，此处使用切片数排序
                contigs_all.sort(key=lambda x: x[4], reverse=True)
                # 排序第一位作为 best contig
                if contigs_all:
                    contigs_best.append(contigs_all[0])
                if not contigs_best:
                    if contigs_all_low == []:
                        failed_count += 1
                        if os.path.isfile(contig_best_path): os.remove(contig_best_path)
                        if os.path.isfile(contig_all_path): os.remove(contig_all_path)
                        Write_Print(os.path.join(args.o,  "log.txt"), "Insufficient reads coverage, unable to obtain valid contig.")
                        result_dict[key] = "no contigs"
                        continue
                    else:
                        contigs_all_low.sort(key=lambda x: x[4], reverse=True)
                        contigs_best.append(contigs_all_low[0])
                        with open(contig_best_path, 'w') as out:
                            for x in contigs_best:
                                out.write('>contig_' + str(len(x[0])) + '_' + str(x[1]) + '_' + str(x[2]) + '_' + str(x[3]) + '_' + str(x[4]) + '_' + str(x[5]) + '\n')
                                out.write(x[0] + '\n')
                        with open(contig_all_path, 'w') as out:
                            for x in contigs_all_low:
                                out.write('>contig_' + str(len(x[0])) + '_' + str(x[1]) + '_' + str(x[2]) + '_' + str(x[3]) + '_' + str(x[4]) + '_' + str(x[5]) + '\n')
                                out.write(x[0] + '\n')
                        ref_dict, filtered_dict = {}, {}
                        result_dict[key] = "low quality"
                        gc.collect()
                        continue
                # # 获取contigs中seed个数的最大值
                # max_seed_count = max([i[1] for i in contigs_best])
                # # 移除小于max_seed_count一半的contig
                # if len(contigs_best) > 1:
                #     contigs_best = [i for i in contigs_best if i[1] > max_seed_count >> 1]
                # if len(contigs_best) > 1:
                #     Write_Print(os.path.join(args.o,  "log.txt"), key, 'have mutiply contigs', ' '*10)
                #     result_dict[key] = "mutiply contigs"
                # 保存contigs，格式为 长度_使用的种子数_粗略的千分比位置_权重_跟参考序列重叠的切片数_paired计数
                with open(contig_best_path, 'w') as out:
                    for x in contigs_best:
                        out.write('>contig_' + str(len(x[0])) + '_' + str(x[1]) + '_' + str(x[2]) + '_' + str(x[3]) + '_' + str(x[4]) + '_' + str(x[5]) + '\n')
                        out.write(x[0] + '\n')
                with open(contig_all_path, 'w') as out:
                    for x in contigs_all:
                        out.write('>contig_' + str(len(x[0])) + '_' + str(x[1]) + '_' + str(x[2]) + '_' + str(x[3]) + '_' + str(x[4]) + '_' + str(x[5]) + '\n')
                        out.write(x[0] + '\n')
                ref_dict, filtered_dict = {}, {}
                result_dict[key] = "success"
                gc.collect()
        Write_Dict(result_dict, os.path.join(args.o, "result_dict.txt"), False, True)
        t1 = time.time()
        Write_Print(os.path.join(args.o,  "log.txt"), '\nTime cost:', t1 - t0, " "*32,'\n') # 拼接所用的时间
    except Exception as e:
        Write_Print(os.path.join(args.o,  "log.txt"), e)
  