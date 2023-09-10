from win_filter import * 
import shutil
pars.add_argument('-rl', metavar='<int>', type=int, help='''reads length''', required=False, default = 150)
pars.add_argument('-max_depth', metavar='<int>', type=int, help='''reads length''', required=False, default = 512)
pars.add_argument('-max_size', metavar='<int>', type=int, help='''file size''', required=False, default = 8)


if __name__ == '__main__':
    args = pars.parse_args()
    if args.lkd: 
        # 载入保存的kmer字典
        if os.path.exists(args.lkd):
            Read_Dict(kmer_dict, args.lkd)
            Write_Print(os.path.join(args.o,  "log.txt"),"The dictionary file of length", len(kmer_dict), "has been successfully loaded.")
        elif os.path.exists(os.path.join(args.o, args.lkd)):
            Read_Dict(kmer_dict, os.path.join(args.o, args.lkd))
            Write_Print(os.path.join(args.o,  "log.txt"),"The dictionary file of length", len(kmer_dict), "has been successfully loaded.")
        else:
            Write_Print(os.path.join(args.o,  "log.txt"),"Could not find dictionary file.")
            sys.exit(0)
    # 载入reads count信息
    if os.path.exists(os.path.join(args.o, "ref_reads_count_dict.txt")):
        Read_Dict(ref_reads_count_dict, os.path.join(args.o, "ref_reads_count_dict.txt"), False)
    # 载入参考序列信息
    if not Get_Ref_Info(args.r, ref_path_list, ref_path_dict, ref_length_dict):
        Write_Print(os.path.join(args.o,  "log.txt"),'Invaild reference!')
        sys.exit(0)
    reads_length = args.rl

    if not os.path.isdir(os.path.join(args.o, 'large_files')):os.mkdir(os.path.join(args.o, 'large_files'))
    for key in ref_length_dict:
        if os.path.isfile(os.path.join(args.o, "filtered",  key + ".fq" )):
            # 需要重过滤的文件放到large_files里面
            if (ref_reads_count_dict.get(key, 0)/ref_length_dict[key] * reads_length > args.max_depth) and os.path.getsize(os.path.join(args.o, "filtered", key + ".fq" )) >> 20 >= args.max_size:
                Write_Print(os.path.join(args.o,  "log.txt"),key,int(ref_reads_count_dict.get(key, 0)/ref_length_dict[key] * reads_length),os.path.getsize(os.path.join(args.o, "filtered", key + ".fq" )) >> 20)
                shutil.move(os.path.join(args.o, "filtered",  key + ".fq" ), os.path.join(args.o, 'large_files', key + ".fq" )) 
                
    filter_count = 0
    for key in ref_length_dict:
        filter_count += 1
        # 检测large_files里面的文件
        large_files_path = os.path.join(args.o, 'large_files', key + ".fq" )
        if os.path.isfile(large_files_path):
            if os.path.isfile(os.path.join(args.o, "filtered",  key + ".fq" )) == False:
                with open(os.path.join(args.o, "filtered", key + ".fq"), "w") as f: pass
                add_k = 2
                # 循环过滤
                while True:
                    Write_Print(os.path.join(args.o,  "log.txt"),"re-filter:", os.path.splitext(os.path.basename(ref_path_dict[key]))[0],'with k =', args.kf + add_k)
                    ref_reads_count_dict[key] = 0
                    used_dict = {}
                    # 过滤
                    Make_Kmer_Dict_v6(used_dict, [ref_path_dict[key]], args.kf + add_k, args.gr, False)
                    Reads_Filter_v5(used_dict, ref_reads_count_dict,[large_files_path], args.kf + add_k, args.s, large_files_path, large_files_path, args.o, args.gr)
                    # 安全的文件替代
                    # with open(os.path.join(args.o, key + ".fq"),'wb') as wfd:
                    #     with open(os.path.join(args.o, key + ".1.fasta"),'rb') as fd:
                    #         shutil.copyfileobj(fd, wfd)
                    #     os.remove(os.path.join(args.o, key + ".1.fasta"))
                    Write_Log(os.path.join(args.o, 'log.csv'), 'filter', key, ref_reads_count_dict[key], ref_length_dict[key], round(ref_reads_count_dict[key]/ref_length_dict[key]*reads_length, 2), args.kf + add_k)
                    if ref_reads_count_dict[key]/ref_length_dict[key]*reads_length < args.max_depth or os.path.getsize(os.path.join(args.o, "filtered", key + ".fq" )) >> 20 < args.max_size or args.kf + add_k >= 99:
                        if os.path.getsize(os.path.join(args.o, "filtered", key + ".fq" )) >> 20 >= args.max_size:
                            # 超大的文件直接按比例缩小，缩小比率为文件大小（M）除以args.max_size
                            get_mark = int((os.path.getsize(large_files_path)  >> 20) / args.max_size) + 1
                            with open(large_files_path, "r") as big_infile:
                                with open(os.path.join(args.o, "filtered", key + ".fq" ), "w") as s_outfile:
                                    line_count = 0
                                    for _ in big_infile:
                                        if line_count%get_mark == 0:
                                            s_outfile.writelines([_,big_infile.readline(),big_infile.readline(),big_infile.readline()])
                                        else:
                                            big_infile.readline()
                                            big_infile.readline()
                                            big_infile.readline()
                                        line_count += 1
                            ref_reads_count_dict[key] = int(ref_reads_count_dict[key]/get_mark)
                            Write_Log(os.path.join(args.o, 'log.csv'), 'filter', key, ref_reads_count_dict[key], ref_length_dict[key], round(ref_reads_count_dict[key]/ref_length_dict[key]*reads_length, 2), 'cutoff')
                            Write_Print(os.path.join(args.o,  "log.txt"),"re-filter:", os.path.splitext(os.path.basename(ref_path_dict[key]))[0] ,'with cutoff')
                        break
                    # 太深的文件大幅提高k
                    elif ref_reads_count_dict[key]/ref_length_dict[key]*reads_length > args.max_depth * 4:
                        add_k += 8
                    else:
                        add_k += 2
    Write_Dict(ref_reads_count_dict, os.path.join(args.o, "ref_reads_count_dict.txt"), False, True)