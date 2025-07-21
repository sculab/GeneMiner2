#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import math
import gzip
import argparse
import operator
import re
import os
import sys
import csv
import matplotlib.pyplot as plt
import numpy as np
from scipy.stats import gaussian_kde


amb = {"-": "-", "A": "A", "C": "C", "G": "G", "N": "N", "T": "T",
        "-A": "a", "-C": "c", "-G": "g", "-N": "n", "-T": "t",
        "AC": "M", "AG": "R", "AN": "a", "AT": "W", "CG": "S",
        "CN": "c", "CT": "Y", "GN": "g", "GT": "K", "NT": "t",
        "-AC": "m", "-AG": "r", "-AN": "a", "-AT": "w", "-CG": "s",
        "-CN": "c", "-CT": "y", "-GN": "g", "-GT": "k", "-NT": "t",
        "ACG": "V", "ACN": "m", "ACT": "H", "AGN": "r", "AGT": "D",
        "ANT": "w", "CGN": "s", "CGT": "B", "CNT": "y", "GNT": "k",
        "-ACG": "v", "-ACN": "m", "-ACT": "h", "-AGN": "r", "-AGT": "d",
        "-ANT": "w", "-CGN": "s", "-CGT": "b", "-CNT": "y", "-GNT": "k",
        "ACGN": "v", "ACGT": "N", "ACNT": "h", "AGNT": "d", "CGNT": "b",
        "-ACGN": "v", "-ACGT": "N", "-ACNT": "h", "-AGNT": "d", "-CGNT": "b",
        "-ACGNT": "N"}

def parse_cigar(cigarstring, seq, pos_ref):
    cigar = [{"type": m[1], "length": int(m[0])} for m in re.findall(r"(\d+)([MIDNSHPX=])", cigarstring)]
    start, start_ref, seqout, insert = 0, pos_ref, "", []
    for c in cigar:
        l, t = c["length"], c["type"]
        if t in ["=", "X", "M"]: seqout += seq[start:start + l]; start_ref += l
        elif t in ["D", "N", "P"]: seqout += "-" * l; start_ref += l
        elif t == "I": insert.append((start_ref, seq[start:start + l]))
        start += l if t in ["=", "X", "M", "I", "S"] else 0
        if t not in ["H", "=", "X", "M", "D", "N", "P", "I", "S"]: print("SAM file probably contains unmapped reads")
    return seqout, insert

def parse_args():
    parser = argparse.ArgumentParser(formatter_class=argparse.RawDescriptionHelpFormatter, description='''Constructing Consensus Sequences Based on SAM Files''')
    parser.add_argument("-i", "--input", action="store", dest="filename",
                        help="Name of the SAM file, SAM does not need to be sorted and can be compressed with gzip")
    parser.add_argument("-c", "--consensus-thresholds", action="store", dest="thresholds", type=str, default="0.25",
                        help="List of consensus threshold(s) separated by commas, no spaces, example: -c 0.25,0.75,0.50, default=0.75")
    parser.add_argument("-n", action="store", dest="n", type=int, default=0,
                        help="Split FASTA output sequences every n nucleotides, default=do not split sequence")
    parser.add_argument("-o", "--outfolder", action="store", dest="outfolder",
                        help="Name of output folder, default=same folder as input")
    parser.add_argument("-p", "--prefix", action="store", dest="prefix",
                        help="Prefix for output file name, default=input filename without .sam extension")
    parser.add_argument("-m", "--min-depth", action="store", dest="min_depth", type=int, default=2,
                        help="Minimum read depth at each site to report the nucleotide in the consensus, default=1")
    parser.add_argument("-f", "--fill", action="store", dest="fill", default="-",
                        help="Character for padding regions not covered in the reference, default= - (gap)")
    parser.add_argument("-d", "--maxdel", action="store", dest="maxdel", default=150,
                        help="Ignore deletions longer than this value, default=150")
    parser.add_argument("-s", "--save_mutations", action="store", type=int, dest="save_mutations", default=1,
                        help="Save the Base Composition for Each Locus")
    return parser.parse_args()

def process_sam_header(mapfile):
    sequences = {}
    coverages = {}
    insertions = {}
    refname = ""
    for line in mapfile:
        if line.startswith("@"):
            if line.startswith("@SQ"):
                refname = line.split("\t")[1].replace("SN:", "").split()[0]
                reflength = int(line.split("\t")[2].replace("LN:", ""))
                sequences[refname] = [{"-": 0, "A": 0, "C": 0, "G": 0, "N": 0, "T": 0} for _ in range(reflength)]
                coverages[refname] = [0] * reflength
                insertions[refname] = []
        else:
            break

    return sequences, coverages, insertions, refname, reflength

def parse_sam_file(mapfile, sequences, coverages, insertions, refname, maxdel):
    reads_total = 0
    reads_mapped = 0

    while True:
        mapfile_chunk = mapfile.readlines(50000)
        if not mapfile_chunk:
            break

        for line in mapfile_chunk:
            reads_total += 1
            if line[0] != "\t":
                if line[0] != "@" and line.split("\t")[5] != "*":
                    reads_mapped += 1
                    sam_record = line.split("\t")
                    refname = sam_record[2].split()[0]
                    pos_ref = int(sam_record[3]) - 1
                    seqout, insert = parse_cigar(sam_record[5], sam_record[9], pos_ref)
                    if seqout.count("-") <= maxdel:
                        for nuc in seqout:
                            try:
                                sequences[refname][pos_ref][nuc] += 1
                                pos_ref += 1
                            except:
                                print(refname, pos_ref, nuc, sam_record)
                    else:
                        for nuc in seqout:
                            if nuc != "-" and nuc != "*":
                                sequences[refname][pos_ref][nuc] += 1
                            pos_ref += 1
                            # Show progress
            if reads_total % 500000 == 0:
                print((str(reads_total)+" reads processed.\r"))
    mapfile.close()
    return reads_total, reads_mapped

def save_to_csv(data, output_file):
    # Define the column headers
    headers = ['Position', 'A', 'G', 'C', 'T', 'N', '-']

    with open(output_file, 'w', newline='') as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=headers)
        writer.writeheader()

        for key, value in data.items():
            # Extract the position from the key
            position = key.split('_')[-1]
            # Convert list of tuples into a dictionary
            nuc_counts = dict(value)
            # Prepare the row for CSV
            row = {'Position': position}
            row.update({nuc: nuc_counts.get(nuc, 0) for nuc in headers[1:]})
            writer.writerow(row)

def calculate_proportions(data):
    proportions_dict = {}
    for key, value in data.items():
        nuc_counts = dict(value)
        total_count = sum(nuc_counts.get(nuc, 0) for nuc in ['A', 'G', 'C', 'T'])
        if total_count > 0:
            proportions = [(nuc, round(nuc_counts.get(nuc, 0) / total_count,4)) for nuc in ['A', 'G', 'C', 'T']]
        proportions_dict[key] = proportions
    return proportions_dict

def filter_by_max_proportion(proportions_dict, threshold=0.9):
    filtered_dict = {key: value for key, value in proportions_dict.items() if max([x[1] for x in value]) < threshold}
    return filtered_dict

def reformat_sequences(sequences, coverages, insertions):
    for refname in sequences:
        for pos, cov in enumerate(coverages[refname]):
            coverages[refname][pos] = sum(sequences[refname][pos].values())
            sequences[refname][pos] = reformat_nucleotide_counts(sequences[refname][pos])
        
        if insertions[refname]:
            ins_tmp1 = {}
            for ins in insertions[refname]:
                ins_tmp1.setdefault(ins[0], {}).setdefault(ins[1], 0)
                ins_tmp1[ins[0]][ins[1]] += 1
                
            ins_tmp2 = {}
            for pos in sorted(ins_tmp1):
                motif_max_len = max(len(motif) for motif in ins_tmp1[pos])
                ins_tmp2[pos] = [{"-": 0, "A": 0, "C": 0, "G": 0, "N": 0, "T": 0} for _ in range(motif_max_len)]

            for pos in sorted(ins_tmp1):
                for motif in ins_tmp1[pos]:
                    for col in range(len(motif)):
                        ins_tmp2[pos][col][motif[col]] += ins_tmp1[pos][motif]

            for pos in sorted(ins_tmp2):
                for col in range(len(ins_tmp2[pos])):
                    ins_tmp2[pos][col]["-"] = coverages[refname][pos] - sum(ins_tmp2[pos][col].values())
                    ins_tmp2[pos][col] = reformat_nucleotide_counts(ins_tmp2[pos][col])

            insertions[refname] = ins_tmp2

    return sequences

def reformat_nucleotide_counts(nucleotide_counts):
    count_nucs = {value: [key for key, val in nucleotide_counts.items() if val == value] for value in nucleotide_counts.values()}
    count_nucs = sorted(count_nucs.items(), reverse=True)
    count_nucs = [[i[0] * len(i[1]), i[1]] for i in count_nucs]
    return count_nucs

def add_insertions(fasta_seqout, coverages, refname, pos, threshold, insertions):
    if refname in insertions and pos in insertions[refname]:
        for col, counts in enumerate(insertions[refname][pos]):
            nucs = []
            cov_nucs = 0
            for count in counts:
                if cov_nucs < threshold * coverages[refname][pos]:
                    nucs += count[1]
                    cov_nucs += count[0]
                else:
                    break
            if amb["".join(sorted(nucs))] != "-":
                fasta_seqout += amb["".join(sorted(nucs))]
    return fasta_seqout

def filter_empty_references(sequences, coverages, insertions):
    sequences2erase = [refname for refname in coverages if sum(coverages[refname]) == 0]
    for refname in sequences2erase:
        del sequences[refname]
        del insertions[refname]
    return sequences

def save_fastas(sequences, fill, coverages, outfolder, prefix, min_depth, insertions, nchar, thresholds):
    fastas = {}
    for refname in sequences:
        for t in thresholds:
            fasta_seqout = ""
            fasta_header = ""
            sumcov = 0
            for pos, cov in enumerate(coverages[refname]):
                if sequences[refname][pos]:
                    sumcov += cov
                    if cov >= min_depth:
                        nucs = []
                        cov_nucs = 0
                        for count in sequences[refname][pos]:
                            if cov_nucs < t * cov:
                                nucs += count[1]
                                cov_nucs += count[0]
                            else:
                                break
                        fasta_seqout += amb["".join(sorted(nucs))]
                        fasta_seqout = add_insertions(fasta_seqout, coverages, refname, pos, t, insertions)

                    else:
                        fasta_seqout += fill
                else:
                    fasta_seqout += fill

            fasta_header = (f">{prefix}|c{int(t*100)} reference:{refname} coverage:{round(sumcov/len(fasta_seqout), 2)} "
                            f"length:{len(fasta_seqout.replace('-', ''))} consensus_threshold:{int(t*100)}%")

            if len(fasta_seqout.replace('-', '')) > 0:
                if refname not in fastas:
                    fastas[refname] = [[fasta_header, fasta_seqout]]
                else:
                    fastas[refname].append([fasta_header, fasta_seqout])

    for reference in fastas:
        outfile = open(os.path.join(outfolder, prefix+".fasta"), "w")
        if nchar == 0:
            outfile.write("\n".join([i[0] + "\n" + i[1] for i in fastas[reference]]) + "\n")
        else:
            outfile.write("\n".join([i[0] + "\n" + "\n".join([i[1][s:s+nchar] for s in range(0, len(i[1]), nchar)]) for i in fastas[reference]]) + "\n")
        outfile.close()
        if len(thresholds) == 1:
            print(f"Consensus sequence at {int(thresholds[0]*100)}% saved for {reference} in: {outfolder+prefix}.fasta")
        else:
            print(f"Consensus sequences at {','.join([str(int(i*100))+'%' for i in thresholds])} saved for {reference} in: {outfolder+prefix}.fasta")
    print("Done.\n")

def save_mutations_table(sequences, coverages, outfolder, prefix, min_depth):
    pos_dict = {}
    for refname in sequences:
        for pos, cov in enumerate(coverages[refname]):
            if sequences[refname][pos]:
                if cov >= min_depth:
                    nucs = []
                    for count in sequences[refname][pos]:
                        for n in count[1]:
                            nucs.append((n, count[0]))
                    pos_dict[f"{refname}_{pos}"] = nucs

    # save_to_csv(pos_dict, os.path.join(outfolder, prefix +".csv"))
    filted_dict = filter_by_max_proportion(calculate_proportions(pos_dict))
    if filted_dict:
        # save_to_csv(filted_dict, os.path.join(outfolder, prefix +".pec.csv"))
        data = []
        for v in filted_dict.values():
            for x in v:
                if x[1] < 0.9 and x[1]>0.1:
                    data.append(x[1])
        if len(data) > 4:
            density = gaussian_kde(data)
            xs = np.linspace(min(data), max(data), 200)
            density.covariance_factor = lambda : .2
            density._compute_covariance()
            # 绘制概率密度曲线
            plt.plot(xs, density(xs))
            plt.savefig(os.path.join(outfolder, prefix +".png"))



def main():
    args = parse_args()
    filename = args.filename
    opener = gzip.open if filename.endswith(".gz") else open
    thresholds = [float(i) for i in args.thresholds.split(",")]
    prefix = args.prefix or os.path.splitext(os.path.basename(args.filename))[0]
    outfolder = args.outfolder
    os.makedirs(outfolder, exist_ok=True)
    min_depth = args.min_depth
    nchar = args.n
    fill = args.fill
    maxdel = args.maxdel
    try:
        sequences, coverages, insertions, refname, reflength = process_sam_header(opener(filename))
        reads_total, reads_mapped = parse_sam_file(opener(filename), sequences, coverages, insertions, refname, maxdel)
        print(f"A total of {reads_total} reads were processed, out of which, {reads_mapped} reads were mapped.\n")
        sequences = reformat_sequences(sequences, coverages, insertions)
        sequences = filter_empty_references(sequences, coverages, insertions)
        if args.save_mutations: save_mutations_table(sequences, coverages, outfolder, prefix.replace("_tmp",""), min_depth)
        save_fastas(sequences, fill, coverages, outfolder, prefix, min_depth, insertions, nchar, thresholds)
    except Exception as e:
        Write_Print(os.path.join(args.outfolder, os.pardir, "log.txt"), "error:" , e)

def Write_Print(log_path, *log_str, sep = " "):
    """
    记录日志并打印
    """
    line = sep.join(map(str,log_str)).strip()
    with open(log_path, 'a') as out:
        out.write(line)
        out.write('\n')
    print(line)

if __name__ == "__main__":
    main()
