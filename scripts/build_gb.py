import csv
import os
import shutil
from Bio import SeqIO
import argparse
import datetime

def str2bool(v):
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Unsupported value encountered.')

def extract_gb_info(genbank_file):
    gb_info = []
    count = 0
    for record in SeqIO.parse(genbank_file, "genbank"):
        total_length = len(record.seq)
        species = record.annotations.get("organism", "Unknown")
        gene_count = sum(1 for feature in record.features if feature.type == "gene")
        full_sequence = str(record.seq)
        date_str = record.annotations.get("date", "1900-01-01")
        try:
            gb_date = datetime.datetime.strptime(date_str, "%d-%b-%Y")
            decimal_year = gb_date.year + (gb_date.timetuple().tm_yday - 1) / 365.0
            decimal_year = round(decimal_year, 4) 
        except ValueError:
            decimal_year = 1900.0  # Default value in case of error
        
        count += 1
        gb_info.append({
            "ID": count,
            "Name": record.description,
            "Sequence": full_sequence,
            "State": record.id,
            "Date": decimal_year,
            "Quantity": 1,
            "Organism": species
        })

    return gb_info


def save_to_csv(data, csv_file):
    with open(csv_file, mode='w', newline='', encoding='utf-8') as file:
        fieldnames = ["ID","Name","Sequence","State", "Date", "Quantity", "Organism"]
        writer = csv.DictWriter(file, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(data)

def split_gb(input_data, out_dir, clean = True):
    if clean: 
        if os.path.exists(out_dir): shutil.rmtree(out_dir)
    if not os.path.exists(out_dir):
        os.makedirs(out_dir)
    records = SeqIO.parse(input_data, "genbank")
    my_id = 1
    while os.path.exists(os.path.join(out_dir, str(my_id) + ".gb")): my_id += 1
    for record in records:
        organism = record.annotations.get("organism", "organism").replace(" ", "_").replace(".", "")
        accession = record.annotations.get("accessions", ["accessions"])[0]
        output_filename = os.path.join(out_dir, str(my_id) + ".gb")
        with open(output_filename, "w") as output_file:
            SeqIO.write(record, output_file, "genbank")
        my_id += 1

if __name__ == "__main__":
    pars = argparse.ArgumentParser(formatter_class=argparse.RawDescriptionHelpFormatter, description=''' MakeData YY ''')
    pars.add_argument('-input', metavar='<str>', type=str, help='''input file.''', required=False, default='chloroplast.gb')
    pars.add_argument('-outdir', metavar='<str>', type=str, help='''output folder''', required=False, default='out_gb')
    pars.add_argument('-clean', type=str2bool, nargs='?', const=True, help='''do clean''', default = True)
    pars.add_argument('-outcsv', metavar='<str>', type=str, help='''output csv''', required=False, default='gb_info.csv')
    args = pars.parse_args()
    print("Do not close this window manually, please!")
    genbank_file = args.input
    csv_file = args.outcsv
    out_dir = args.outdir
    do_clean = args.clean
    split_gb(genbank_file, out_dir, do_clean)
    gb_info = extract_gb_info(genbank_file)
    save_to_csv(gb_info,os.path.join(out_dir,csv_file))
    
