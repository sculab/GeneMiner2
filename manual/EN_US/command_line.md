# Command Line Interface

## Building

To run GeneMiner2 natively on Unix-like environments, a custom build is necessary. The general rule of binary compatibility on Linux applies: the compiled binaries will work on similar distributions with identical or newer libraries.

First, install the default C++ compiler and [zlib](https://zlib.net/) for the platform. On Ubuntu 20.04 and later, this can be done as follows:

```
sudo apt install build-essential zlib1g zlib1g-dev
```

Advanced users can install [zlib-ng](https://github.com/zlib-ng/zlib-ng) or [cloudflare-zlib](https://github.com/cloudflare/zlib) in place of zlib to improve performance.

Second, install [Haxe 4](https://haxe.org/) and [configure the C++ target](https://haxe.org/manual/target-cpp-getting-started.html). On Ubuntu 20.04 and later, the following lines are sufficient:

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
cd GeneMiner2
make
```

The binaries are placed under the `cli` directory.

## Usage

The binaries themselves have few dependencies and can generally work on a wide range of similar distributions. For example, GeneMiner2 built on Ubuntu 20.04 runs on Debian bullseye and later, Ubuntu 20.04 and later, and Ubuntu derivatives based on Ubuntu 20.04 or later.

Nevertheless, several important tools must be available at run time. With conda, these tools can be installed as follows:

```
conda install -c bioconda aster blast clustalo fasttree iqtree mafft magicblast minimap2 muscle raxml-ng trimal veryfasttree
```

GeneMiner2 will work as long as these tools are in `PATH`. To specify a particular executable, place it under `cli/bin`. For example, creating such a symbolic link at `cli/bin/mafft` forces GeneMiner2 to use that certain executable.

```
ln -s /home/user/.local/bin/mafft cli/bin/mafft
```

The entry point is `cli/geneminer2`. To run an analysis, GeneMiner2 requires a a tab-delimited sample list, with format `<Species Name><Tab><Read File 1>` (single read) or `<Species Name><Tab><Read File 1><Tab><Read File 2>` (paired-end reads), each line denoting a sample. For example, the sample list for [DEMO 3](../../DEMO/DEMO3/DEMO3.md) looks as follows:

```
Bupleurum_chinense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_2.fq.gz
Bupleurum_fruticosum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_2.fq.gz
Bupleurum_krylovianum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_2.fq.gz
Bupleurum_malconense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_2.fq.gz
Bupleurum_wenchuanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_2.fq.gz
Bupleurum_yunnanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_2.fq.gz
```

Next, assuming the sample list is saved to `/home/user/GeneMiner2/DEMO/DEMO3/samples.tsv` and Angiosperm353 genes saved as FASTA files (one per gene) under `/home/user/Angiosperm353`, run GeneMiner2 with default settings:

```
cli/geneminer2 -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /home/user/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output
```

GeneMiner2 would build a coalescent tree at `/home/user/GeneMiner2/DEMO/DEMO3/output/Coalescent.tree`. A concatenation tree can be built using `-m concatenation`. Using command line arguments, it is also possible to ask GeneMiner2 to run specific steps. For example, given these parameters:

| Parameter                  | Value             |
| -------------------------- | ----------------- |
| Source Sequence            | Consensus Results |
| Retention Length Threshold | 50%               |
| Trim Method                | All Fragments     |
| Alignment Program          | muscle            |
| Maximum difference >       | 0.2               |
| Number of sequences        | 5                 |

The following line runs Trim With Reference and Combine Results with them:

```
cli/geneminer2 trim combine -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /mnt/data/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output -ts consensus -tm all -tr 0.5 -cd 0.2 -cn 5 --msa-program muscle
```

Run `cli/geneminer2 -h` to view all command line switches. All parameters and output are analogus to their counterparts in the graphical version, except that the command line interface only accepts decimal values between 0.0 and 1.0 for percentages. Additionally, several internal options (such as `--min-depth`) are also exposed, providing extra flexibility for advanced users.
