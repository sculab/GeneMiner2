## NOTE：
* Please do not install GeneMiner on a portable hard drive.
* If files (sequences) are currently in use by GeneMiner, please do not open them to avoid program conflicts that may cause crashes.



**1.** **Does sequencing data require the removal of junctions and low-quality reads?**


It's recommended to use the high-quality (HQ) version of the data provided by the sequencing company. Using low-quality data may result in less optimal extraction outcomes. If HQ data is not available, removing junctions and low-quality reads is advisable to enhance results.




**2.** **Causes and solutions for the absence of assemble result.**


*The selected reference sequence may not be sufficiently related. (Find a more closely related sequence manually).

*The sequencing data might not be deep enough. (Consider reducing the filter K value or extending the [Reads/File(M)]).

*If the assembly results are not ideal, re-analyze using [Analysis > Iteration].

*Consider reducing the Kmer value during assembly.

**Note:**  Lowering the Kmer value might affect accuracy. Manual discernment of false positive sequences is recommended.


**Note:**  It's advisable to set the Kmer value to be greater than 17 and ensure it's an odd number.




**3.** **What is the memory requirement of the software?**


The software requires minimal memory. Adjust the [Threads] setting according to your computer's memory capacity.




**4.** **How to obtain the intron sequence data?**


Firstly, acquire the gb file via [Analyse > Plant Chloroplast Genome], then import it using [File > Load Reference]. Confirm to report the file as a gene list. Determine the intron region length using [Extend Left] and [Extend Right].

**Note:** If the imported reference CDS includes an internal vacant intron region, the software's analysis will include this vacant intron.




**5.** **No result for  [Combine & Trim]?**


[Batch > Combine & Trim] requires: sequencing data, reference sequences, and a catalog folder where results exist.

**Note:** The ID number in the result folder(3) should match the imported sequencing file ID(2).  
*Ensure there is no Chinese catalog folder.

![](images/trim_details.jpg)




**6.** **No results for PPD?**


Ensure the selection of three or more species for batch extraction. Ensure there is no Chinese catalog folder.




**7.** **No results for Mitochondrial Genome?**


For mitochondrial genome assembly, it's recommended to utilize the full read length of the sequencing file. Uncheck the [Reads/File(M)] checkbox at [Analysis > Filter] to analyze the full read length data.


**8. ** **Why are the results obtained from [Trim With Reference] incomplete and of varying lengths?**

Reference-based trimming retains only the sequence results that are longer than a set threshold times the median length of the sequences. If the option [Keep Only the Longest Match] is not selected, the sequence will preserve the maximum length matched with all reference sequences.






