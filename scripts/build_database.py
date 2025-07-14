#!/usr/bin/env python
# -*- coding: utf-8 -*-
# @Time       : 2022/5/29 10:38 下午
# @Author     : zzhen
# @File       : build_database.py
# @Software   : PyCharm
# @Description: use to download AGS sequences from Kew Tree of Life Explorer
# @Copyright  : Copyright (c) 2022 by sculab, All Rights Reserved.
import argparse
import csv
import json
import os
import re
from collections import defaultdict
from urllib.error import HTTPError
from urllib.request import urlopen
from concurrent.futures import ThreadPoolExecutor, as_completed
from Bio import SeqIO


# detect network_connect
def network_connect() -> bool:
    connect = False
    # network_connect exit_code == 0
    exit_code = os.system("ping www.baidu.com")
    if not exit_code:
        connect = True
    return connect


# decode classification.json
# return {"Family1":"Family","Genus1":"Genus"}
def parse_classification_json() -> dict:
    _classification_ = defaultdict(str)
    classification_file_path =  "classification.json"
    with open(classification_file_path, "r", encoding="UTF-8") as f:
        _classification_dict_ = json.load(f)
    for key, value in _classification_dict_.items():
        for i in value:
            _classification_[i] = key
    return _classification_


# Determine to which level: order, family or genus the Latin name belongs
def detect_classification(classifications: list) -> dict:
    # set output # {str:list[str]} eg. {"Family": ["family1", "family2"]}
    result_dict = defaultdict(list)
    classification_dict = parse_classification_json()
    # capitalize the latin name
    _classifications_ = [classification.capitalize() for classification in classifications if classification]
    for classification in _classifications_:
        tmp = classification_dict.get(classification, None)
        if tmp is None:
            print("The Latin name {} may be incorrect or the database does not have the sequences"
                  .format(classification))
        else:
            result_dict[tmp].append(classification)
    return result_dict


# Get file information to be downloaded based on the taxon
def generate_download_info(classification_dict: dict) -> list:
    result_list = []
    kew_data_file_path = "kew_data.csv"
    _reader_ = csv.DictReader(open(kew_data_file_path, "r", newline=""))
    # store the order, family and genus in the list
    for key, value in classification_dict.items():
        # row is a dict that stores the download information
        for row in _reader_:
            if row in result_list:
                continue
            if row[key] in value:
                result_list.append(row)
    return result_list


# function to download fasta file
def download_fasta_file(_spec_info_: dict, output_dir: str):
    info = None
    url = _spec_info_['Fasta file url']
    file_path = os.path.join(output_dir, _spec_info_['Fasta file name'])
    for _ in range(3):
        if os.path.isfile(file_path):
            info = "INFO: File {} already exists".format(file_path)
            break
        else:
            try:
                response = urlopen(url, timeout=10)
                with open(file_path, 'wb') as f:
                    f.write(response.read())
                break
            except HTTPError:
                info = "INFO: Url {} does not exist".format(url)
                break
            except Exception as e:
                info = "WARNING: {}".format(e)
    return info


# Get the path of all fasta files under the folder
def generate_fasta_path(_dir_path_: str) -> list:
    # store all the paths
    file_path_list = []
    if not os.path.isdir(_dir_path_):
        raise FileNotFoundError("The path is not a directory!")
    # Set the file extension
    extension = (".fasta", ".fas", ".fa", ".faa")
    # Traversal of folders
    for _root_, _dir_, _file_ in os.walk(top=_dir_path_, topdown=False):
        if _file_:
            _file_ = [os.path.join(_root_, _one_file_) for _one_file_ in _file_ if _one_file_.endswith(extension)]
            file_path_list.extend(_file_)
    return file_path_list


# generate fasta files of different genes as reference files
def generate_gene_file(_file_path_list_: list, _output_dir_: str, _exclude_species_: list = None) -> None:
    if not os.path.isdir(_output_dir_):
        os.makedirs(_output_dir_)
    for _file_ in _file_path_list_:
        for record in SeqIO.parse(_file_, "fasta"):
            with open(os.path.join(_output_dir_, record.id + ".fasta"), "a") as _out_file_:
                species_name = re.findall(".*Species:(.*)Repository.*", record.description)[0].strip().replace(" ", "_")
                record.id = species_name + "-" + record.id
                if not _exclude_species_ or species_name not in _exclude_species_:
                    _out_file_.write(">" + record.id + "\n")
                    _out_file_.write(str(record.seq) + "\n")
            _out_file_.close()
    return


def main():
    pars = argparse.ArgumentParser(formatter_class=argparse.RawDescriptionHelpFormatter,
                                   description='''The script for making the AGS353 sequences''')
    pars.add_argument('-i', dest="input_dir", type=str, help="The input directory of existed files from kew.org", default = r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\temp\AGS353")
    pars.add_argument('-o', dest="output_dir", type=str,
                      help="The output directory stored the downloaded sequences file", default = r"D:\working\Develop\EasyMiner Develop\EasyMiner\bin\Debug\net6.0-windows\temp\org_seq")

    args = pars.parse_args()
    print("Do not close this window manually, please!")

    # set input_dir and output_dir
    input_dir, output_dir = args.input_dir, args.output_dir
    exclude_species = []
    if input_dir is None and output_dir is None:
        print("Input and output directory cannot both be empty!")
        exit(-1)
    if output_dir is not None:
        if not os.path.isdir(output_dir):
            os.makedirs(output_dir)
        if input_dir is None:
            input_dir = output_dir
    if input_dir is not None and output_dir is None:
        output_dir = input_dir


    print("INFO: Generating species data into reference files")
    # Merge the files under input_dir with the files in output_dir
    input_file_list = generate_fasta_path(input_dir)
    output_file_list = generate_fasta_path(output_dir)
    file_path_list = list((set(input_file_list).union(set(output_file_list))))
    generate_gene_file(_file_path_list_=file_path_list,
                       _output_dir_=os.path.join(output_dir), _exclude_species_=exclude_species)


if __name__ == '__main__':
    main()
