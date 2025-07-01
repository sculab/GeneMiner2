# Directory Annotation

## Output Files

**contigs_all**: All possible assembly results.

**filtered**: The fq files obtained after filtering.

**iteration**: Files obtained from the first or multiple iterations, with filenames and meanings consistent with the parent folder.

**large_files**: Original fq files that exceed the depth or file size limits during further filtering. If all filtering results are within limits, this folder will not appear.

**log.txt**: Log file.

**consensus**: Maps the result sequences to the filtered fq files. Sequences with a number of ambiguous bases above the set value will be retained.

**supercontigs**: Consensus reconstruction result files, generating degenerate sequences using IUPAC codes, with degenerate bases marking SNP sites.

**paralogs**: Result files from paralogous gene screening. The _ref.fasta file stores paralogous genes, the csv file records the number of times different base positions are mapped, and the .pec.csv file records the frequency of base mutations.

**summary.csv**: Summary statistics of the results, including:
  - Reference Median Length: The median length of the reference sequences, used for filtering during the Reference-based Trimming step.

  - Reads Counts: The number of reads retained after filtering and matching.

  - Result Availability: Indicates whether an assembly result exists (1 = yes).

  - Multicopy Presence: Indicates whether multi-copy sequences are detected (1 = yes).


**results: The sequence with the highest weight in the assembly results,  the final result.**

  - kmer_dict_k31.dict: kmer dictionary file, formatted as: kmer fragment (in hexadecimal), kmer count (in hexadecimal).

  - result_dict.txt: Results file, formatted as: gene name, sequence assembly status, number of reads assembled.

  - ref_reads_count_dict.txt: Total number of kmers split from each reference gene sequence.

  - result_dict.txt: Results file, formatted as: gene name, sequence assembly status, number of reads assembled.

---

**Organelle**: Assembly results of the organelle genomes.

  - Gennome_cp.fasta: Plant chloroplast genome assembly result.

  - Gennome_cp.gb: Annotated plant chloroplast genome assembly result.

  - Gennome_mito_plant.fasta: Plant mitochondrial genome assembly result.

  - temp: Files from the organelle genome assembly process that were not completed due to terminal closure.

  - Gennome_mito.fasta: Animal mitochondrial genome assembly result.


#### Batch- Analysis :

**Your Sequencing Filename**: A folder named after the sequencing sequence, storing the assembly results obtained from each sequencing sequence separately.

**combined_results**: Stores the combined result files.

**combined_trimmed**: Stores the result files after merging and trimming.

**combined_results.fasta**: Concatenated result file.

**combined_trimmed.fasta**: Trimmed concatenated result file.

**aligned**: Results of multiple sequence alignment.

####  :

**PPD>result>supercontig>s8 _rm_paralogs> Final_kept_genes**: This is the final result file of [Paralogous Detection (PPD)], for specific meanings of other files see [PPD GitHub](https://github.com/Bean061/putative_paralog#part2-trims-alignment-and-detectes-the-putative-paralogs).


 ---
