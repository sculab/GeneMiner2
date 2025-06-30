
# 输出结果注解

## 输出文件
**aligned**:[结果加入参考-比对]的结果文件。

**trimed**：[结果加入参考-切齐]的结果文件

**contigs_all**: 所有可能的组装结果。

**filtered**: 过滤后得到的fq文件。

**iteration**: 首次或多次迭代得到的文件，内部文件名和文件含义与上级文件夹相同。

**large_files**: 进一步过滤时超过深度限制或者文件大小限制的原始fq文件。如果所有过滤结果都在限制以内，则不会出现该文件夹。

**log.txt**: 日志文件。

**consensus**：将结果序列和过滤后的fq文件进行映射。存在设定值以上的模糊碱基数的序列将会被保留。

**supercontigs**：一致性重构的结果文件，使用IPUAC代码生成的退化序列，使用简并碱基标注了SNP位点。

**multicopy**：旁系同源基因筛选的结果文件，其中_ref.fasta文件储存旁系同源基因，csv文件记录不同位置碱基map出现的次数，.pec.csv文件记录碱基变异的频率。

**results：拼接结果中权重最大的序列，即最终结果。**

    - kmer_dict_k31.dict: kmer字典文件，格式为：kmer片段(十六进制)，kmer计数（十六进制）。

    - result_dict.txt: 结果文件，格式为：基因名，序列拼接状态，用于组装的序列数量。

    - ref_reads_count_dict.txt: 每个参考基因序列拆分成kmer的总条数。

    - best_refs: '获得最佳参考序列'的结果文件。即匹配reads kmer最多的参考序列。

---
**Organelle**：细胞器基因组的拼接结果。

    - Gennome_cp.fasta：植物叶绿体基因组拼接结果。

    - Gennome_cp.gb：注释后的植物叶绿体基因组拼接结果。

    - Gennome_mito_plant.fasta：植物线粒体基因组拼接结果。

    - temp：因终端关闭终止分析，未完成的细胞器基因组的拼接过程文件。

    - Gennome_mito.fasta：动物线粒体基因组拼接结果。

---

#### 批量分析结果：

**您的测序文件名**：以测序序列名命名的文件夹，储存每个测序序列分别得到的拼接结果。
    子文件夹**blast**：储存基于参考切齐后的结果序列。

**combined_results**：储存合并后的结果文件。

**combined_trimed**：储存合并并切齐后的结果文件。

**combined_results.fasta**: 串联结果文件。

**combined_trimed.fasta**： 修剪后的串联结果文件。

**aligned**: 多序列比对的结果。

**summary.csv**: 统计汇总结果，内包含：

    Reference Median Length：参考序列的长度中值，用于在[基于参考切齐]步骤对序列进行筛选。
    
    Reads Counts: 过滤匹配的序列数量。

    Result Availability：是否存在组装结果，1为是。

    Multicopy Presence：是否存在多拷贝序列。1为是。



 ---

