# Command Line Interface

## Building

To run GeneMiner2 natively on Unix-like environments, a custom build is necessary. The general rule of binary compatibility on Linux applies: the compiled binaries will work on similar distributions with identical or newer libraries.

First, install [Haxe 4](https://haxe.org/) in the build environment. It is also necessary to [configure the C++ target](https://haxe.org/manual/target-cpp-getting-started.html), which basically boils down to two commands:

```
haxelib setup
haxelib install hxcpp
```

Then, use the following commands to install build dependencies with conda. Alternatively, it is also possible to install them manually using other package managers.

```
conda create -n geneminer python=3.11 numpy=2.1.3 biopython matplotlib pyinstaller scipy
conda activate geneminer
```

Finally, build the binaries themselves:

```
make
```

The binaries are placed under the `cli` directory.

## Usage

The binaries depend on a few essential libraries. Therefore, pre-built GeneMiner2 binaries can generally work on a wide range of similar distributions. Nevertheless, several important tools must be available at run time. These tools can be installed using conda as follows:

```
conda install -c bioconda blast clustalo mafft minimap2 muscle trimal
```

GeneMiner2 will work as long as these tools are in `PATH`. To specify a particular executable, place it under `cli/bin`. For example, creating a symbolic link at `cli/bin/mafft` forces GeneMiner2 to use that certain executable.

```
ln -s /home/user/.local/bin/mafft cli/bin/mafft
```

The entry point is `cli/geneminer2`. Before running the pipeline, prepare a tab-delimited sample list. The format is `<Species Name><Tab><Read File 1>` (single read) or `<Species Name><Tab><Read File 1><Tab><Read File 2>` (paired-end reads), with each line denoting a sample. For example, given paired-end data from _Aegopodium podagraria_ and single read data from _Torilis scabra_, the file will look as follows:

```
Aegopodium_podagraria	/mnt/data/Aegopodium_podagraria_1.fq.gz	/mnt/data/Aegopodium_podagraria_2.fq.gz
Torilis_scabra	/mnt/data/Torilis_scabra.fq.gz
```

Next, assuming the sample list is saved at `/mnt/data/samples.tsv` and Angiosperm353 genes saved as FASTA files (one per gene) under `/mnt/data/angiosperm353`, run GeneMiner2 with default settings:

```
cli/geneminer2 -f /mnt/data/samples.tsv -r /mnt/data/angiosperm353 -o /mnt/data/output
```

Note, the command line interface **DO NOT** build trees. You might need to build the tree manually using `combined_trimed.fasta` (concatenation) or every FASTA file in `combined_trimed` (coalescent).

It is also possible to run only specific steps. For example, given a set of parameters:

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
cli/geneminer2 -f /mnt/data/samples.tsv -r /mnt/data/angiosperm353 -o /mnt/data/output -t consensus -n 0.5 -m all --msa-program muscle -d 0.2 -q 5 trim combine
```

Run `cli/geneminer2 -h` to view all command line switches. All parameters and output are analogus to their counterparts in the graphical version, except that the command line interface only accepts decimal values between 0.0 and 1.0 for percentages. Additionally, several internal options (such as `--min-depth`) are also exposed. This provides more flexibility in large-scale runs.
