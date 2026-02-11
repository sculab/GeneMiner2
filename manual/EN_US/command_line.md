# Command Line Interface

For most Linux users, downloading prebuilt binaries from Sourceforge is sufficient. Refer to [Usage](#usage) to see the documentation.

## Building

First, install the default C++ compiler and [zlib](https://zlib.net/) for the platform. On Ubuntu 20.04 and later, this can be done as follows:

```
sudo apt install build-essential zlib1g zlib1g-dev
```

Advanced users can install [zlib-ng](https://github.com/zlib-ng/zlib-ng) or [cloudflare-zlib](https://github.com/cloudflare/zlib) in place of zlib to improve performance.

Second, install [Haxe 4](https://haxe.org/) and [configure the C++ target](https://haxe.org/manual/target-cpp-getting-started.html). On Ubuntu 24.04 and later, the following lines are sufficient:

```
sudo apt install haxe
haxelib setup ~/haxelib
haxelib install hxcpp
```

Third, install Python dependencies. If conda is available, run the following commands. Otherwise, it would be necessary to install them manually using the system package manager.

```
conda create -c conda-forge -n geneminer python=3.11 numpy=2.1.3 biopython matplotlib pyinstaller scipy
conda activate geneminer
```

Finally, download the source code of GeneMiner2 and build the binaries themselves:

```
git clone --depth 1 https://github.com/sculab/GeneMiner2.git
cd GeneMiner2
make
```

The binaries are placed under the `cli` directory. Run `cli/geneminer2 -h` to view the help messages. Several important tools must be available at run time. With conda, these tools can be installed as follows:

```
conda install -c bioconda aster blast clustalo fasttree iqtree mafft magicblast minimap2 muscle raxml-ng trimal veryfasttree
```

## Usage

To run an analysis, GeneMiner2 requires a a tab-delimited sample list and reference sequences in FASTA format. The sample list has the format `<Species Name><Tab><Read File 1>` (single read) or `<Species Name><Tab><Read File 1><Tab><Read File 2>` (paired-end reads), each line denoting a sample.

Assuming the repository is cloned to `/home/user/GeneMiner2`, the sample list for [DEMO 3](../../DEMO/DEMO3/DEMO3.md) looks as follows:

```
Bupleurum_chinense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_2.fq.gz
Bupleurum_fruticosum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_2.fq.gz
Bupleurum_krylovianum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_2.fq.gz
Bupleurum_malconense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_2.fq.gz
Bupleurum_wenchuanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_2.fq.gz
Bupleurum_yunnanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_2.fq.gz
```

The reference sequences has to be under a separate directory. For each gene, place all of its reference sequences in `<Gene Name>.fasta`. For example, to extract matK and psbA genes, create `matK.fasta` and `psbA.fasta` under an empty directory, and write reference sequences into the respective file.

Next, assuming the sample list is saved to `/home/user/GeneMiner2/DEMO/DEMO3/samples.tsv`, Angiosperm353 genes saved under `/home/user/Angiosperm353`, and the desired output location `/home/user/GeneMiner2/DEMO/DEMO3/output`, run GeneMiner2 with default settings:

```
cli/geneminer2 -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /home/user/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output
```

GeneMiner2 will build a coalescent tree at `/home/user/GeneMiner2/DEMO/DEMO3/output/Coalescent.tree`.

All command line parameters:

- `-f`: Sample list in tsv format
- `-r`: Reference directory
- `-o`: Output directory
- `-p`: Number of parallel processes
- `--max-reads`: Max reads per file, disabled by default
- `-kf`: Filter k-mer size
- `-s`: Filter step size
- `--max-depth`: Depth limit
- `--max-size`: File size limit
- `-ka`: Assembly k-mer size, automatic by default
- `--min-ka`: Minimum auto-estimated assembly k-mer size
- `--max-ka`: Maximum auto-estimated assembly k-mer size
- `-e`: Error threshold
- `-sb`: Soft boundary (`0`, `auto` or `unlimited`)
- `-i`: Search depth
- `-c`: Consensus threshold (as a decimal, between 0.0 and 1.0)
- `-ts`: Trim source (`assembly` or `consensus`)
- `-tm`: Trim method (`all`, `longest`, `terminal` or `isoform`)
- `-tr`: Retention length threshold (as a decimal, between 0.0 and 1.0)
- `-cs`: Source of combine results (`assembly`, `consensus` or `trimmed`)
- `--msa-program`: `mafft`, `muscle` or `clustalo`
- `-cd`: Maximum difference after clean-up
- `-cn`: Minimum number of sequences after clean-up
- `-m`: `coalescent` or `concatenation`
- `-b`: Number of ootstrap replicates
- `--phylo-program`: `fasttree`, `veryfasttree`, `iqtree` or `raxmlng`

For example, after running the command above, you can ask GeneMiner2 to build a concatenation tree based on previous results using `tree` subcommand and `-m concatenation` argument:

```
cli/geneminer2 tree -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /home/user/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output -m concatenation
```

Similarly, it is also possible to ask GeneMiner2 to run specific steps. For example, given these parameters:

| Parameter                  | Value             |
| -------------------------- | ----------------- |
| Source Sequence            | Consensus Results |
| Retention Length Threshold | 50%               |
| Trim Method                | All Fragments     |
| Alignment Program          | muscle            |
| Maximum difference >       | 0.2               |
| Number of sequences        | 5                 |

The following line runs **Trim With Reference** and **Combine Results**:

```
cli/geneminer2 trim combine -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /mnt/data/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output -ts consensus -tm all -tr 0.5 -cd 0.2 -cn 5 --msa-program muscle
```

All parameters and output are analogus to their counterparts in the graphical version, except that the command line interface only accepts decimal values between 0.0 and 1.0 for percentages. Additionally, several internal options (such as `--min-depth`) are also exposed, providing extra flexibility for advanced users.
