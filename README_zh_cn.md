# GeneMiner2: 系统发育基因组学工具包
**[To View README in English](README.md)**

# 介绍
GeneMiner2是一款为系统发育基因组学设计的全功能工具包，软件主要功能包括：
- 从二代测序数据中挖掘单拷贝核基因、质体基因等分子标记
- 将多个分子标记切齐、排序、建立串联和溯祖系统发育树
- 拼接注释动植物质体基因组

用户能够在GeneMiner2中完成从NGS数据获取到系统发育树建立的所有工作。

## 引用
Yu XY, Tang ZZ, Zhang Z, Song YX, He H, Shi Y, Hou JQ, Yu Y. GeneMiner2: Accurate and automated recovery of genes from genome skimming data. *Molecular Ecology Resources*（Under Review）.

GeneMiner2基于我们之前开发的GeneMiner和Easy353软件，并整合了Blast、Minimap2、Fasttree、Muscle5、Mafft、Astral、PPD、PGA、NOVOPlasty、OrthoFinder等优秀工具，请在使用对应功能时引用软件提示的文献。


该工具建立在我们之前发布的方法成功的基础之上：
- Zhang Z, Xie PL, Guo YL, Zhou WB, Liu EY, Yu Y. 2022. Easy353: A tool to get Angiosperms353 genes for phylogenomic research. Molecular Biology and Evolution 39(12): msac261.

- Xie PL, Guo YL, Teng Y, Zhou WB, Yu Y. 2024. GeneMiner: A tool for extracting phylogenetic markers from next-generation sequencing data. Molecular Ecology Resources: e13924. [doi:10.1111/1755-0998.13924](https://www.researchgate.net/publication/377298770_GeneMiner_A_tool_for_extracting_phylogenetic_markers_from_next-generation_sequencing_data)


# 安装和需求

![](images/fig1.png)

![](images/fig2.jpg)


GeneMiner2的源代码在Github和Gitee上均可获取。

## Windows和macOS用户

请从下面的地址获取最新的安装包:

**[SourceForge](https://sourceforge.net/projects/geneminer/files/)**

**注意:** 请勿在移动磁盘上运行GeneMiner2。请勿将GeneMiner2安装到带有中文的文件夹中。

**关于并行运行:** 不要在同一个文件夹下打开多个窗口并行运行，可以复制GeneMiner所在的文件夹，并在副本文件中运行第二个窗口。文件夹路径不能有中文名。

**macOS用户:** 在macOS上运行GeneMiner2时，如果遇到 "GeneMiner.app已损坏，无法打开"之类的错误，请打开终端，运行类似下面的命令:
- xattr -cr **把GeneMiner.app拖放到此处**
- 例如: xattr -cr /Applications/GeneMiner.app

由于GeneMiner2通过Wine技术运行在macOS上，运行效率远低于Windows，因此不建议用GeneMiner2在macOS上进行大规模的分析。

## Linux和其他\*nix用户

**[桌面Linux系统上的运行方法](manual/ZH_CN/linux_desktop.md)**

**[在类Unix平台上的命令行版本的构建指南](manual/ZH_CN/command_line.md)**

您也可以使用`scripts`文件夹中的Python脚本，这些脚本提供了GeneMiner2的所有核心功能，并可以在Windows、macOS或Linux上部署。

---

# 示例演示

[Tutorial 1 - 快速掌握使用方法(单个样品)](/DEMO/DEMO1/DEMO1.md)


[Tutorial 2 - 获取质体基因组和质体基因](/DEMO/DEMO2/DEMO2.md)

  
[Tutorial 3 - 单拷贝基因建树流程(批量，多个样品)](DEMO/DEMO3/DEMO3.md)





# 详细说明


详细说明请移步 [manual](manual/manual_geneminer.pdf)

有关软件图形界面和功能的详细说明[请见此处](manual/ZH_CN/readme_detailed.md)

有关输出目录详解[请见此处](manual/ZH_CN/output.md)


# 常见问题
[常见问题解答](manual/ZH_CN/FAQ.md)


# 联系方式
有关GeneMiner任何建议、问题，请联系邮箱
Xinyi_Yu2021@163.com.