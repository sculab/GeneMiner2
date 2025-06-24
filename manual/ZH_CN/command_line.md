# 命令行版本

## 构建

如果要在Linux或类Unix环境中运行GeneMiner2，需要手动进行构建。一般而言，编译的二进制文件可以在相同或者更新的Linux发行版上运行。

第一步，安装系统的默认C++编译器和[zlib](https://zlib.net/)。在Ubuntu 20.04 或更新版本上，可以运行下面的命令：

```
sudo apt install build-essential zlib1g zlib1g-dev
```

高级用户可以选择安装[zlib-ng](https://github.com/zlib-ng/zlib-ng)或[cloudflare-zlib](https://github.com/cloudflare/zlib)代替zlib以提高性能。

第二步，安装[Haxe 4](https://haxe.org/)并[配置C++目标](https://haxe.org/manual/target-cpp-getting-started.html)。在Ubuntu 20.04 或更新版本上，运行下面的命令：

```
sudo apt install haxe
haxelib setup ~/haxelib
haxelib install hxcpp
```

第三步，安装Python和依赖项。如果可以使用conda，执行下面的命令即可；否则，需要用系统的软件包管理器手动安装这些包。

```
conda create -c conda-forge -n geneminer python=3.11 numpy=2.1.3 biopython matplotlib pyinstaller scipy
conda activate geneminer
```

最后，下载GeneMiner2的源代码并构建可执行文件。

```
cd GeneMiner2
make
```

编译完成的可执行文件放在`cli`目录下。

## 用法

编译得到的二进制文件本身对依赖的要求较少，在类似的系统上均可以运行。例如，在Ubuntu 20.04上编译的GeneMiner2可以在Debian 11或更高版本、Ubuntu 20.04或更高版本、基于Ubuntu 20.04或更高版本的其他发行版上运行。

不过，软件在运行时仍然需要几个独立的生物信息学工具。用conda安装的方法如下：

```
conda install -c bioconda aster blast clustalo fasttree iqtree mafft magicblast minimap2 muscle raxml-ng trimal veryfasttree
```

只要这些工具在`PATH`中能够找到，GeneMiner2就能正常运行。可以通过把工具放在`cli/bin`下的方法强制GeneMiner2使用特定的可执行文件。例如，创建符号链接`cli/bin/mafft`会强制GeneMiner2使用链接的目标作为mafft的可执行文件，示例如下（不需要运行）：

```
ln -s /home/user/.local/bin/mafft cli/bin/mafft
```

GeneMiner2的入口点是`cli/geneminer2`。在运行之前，需要准备一个tsv格式的样本列表。具体格式是`<物种名><Tab><数据文件1>`（单端）或者`<物种名><Tab><数据文件1><Tab><数据文件2>`（双端），每一行代表一个样本。例如，[DEMO 3](../../DEMO/DEMO3/DEMO3.md)的样本列表大概是这样：

```
Bupleurum_chinense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_2.fq.gz
Bupleurum_fruticosum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_2.fq.gz
Bupleurum_krylovianum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_2.fq.gz
Bupleurum_malconense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_2.fq.gz
Bupleurum_wenchuanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_2.fq.gz
Bupleurum_yunnanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_2.fq.gz
```

假设样本列表保存在`/home/user/GeneMiner2/DEMO/DEMO3/samples.tsv`，被子植物353参考基因保存在`/home/user/Angiosperm353`（每个基因一个FASTA文件），用默认设置运行GeneMiner2的命令如下：

```
cli/geneminer2 -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /mnt/data/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output
```

执行这行命令后，GeneMiner2会构建一棵溯祖树，路径为`/home/user/GeneMiner2/DEMO/DEMO3/output/Coalescent.tree`。使用`-m concatenation`参数可以构建串联树。同样，可以用命令行参数要求GeneMiner2只运行某些步骤。假设给出这些参数：

| 参数                        | 值                |
| -------------------------- | ----------------- |
| Source Sequence            | Consensus Results |
| 保留长度阈值                 | 50%               |
| Trim Method                | All Fragments     |
| 比对程序                     | muscle            |
| 最大差异大于                 | 0.2               |
| 序列数量小于                 | 5                 |

可以用下面的命令只执行**基于参考切齐**和**合并结果**：

```
cli/geneminer2 trim combine -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /mnt/data/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output -ts consensus -tm all -tr 0.5 -cd 0.2 -cn 5 --msa-program muscle
```

运行`cli/geneminer2 -h`会显示所有命令行参数。所有参数和输出与图形界面的版本含义一致，除了命令行版本在需要填写百分数的位置只支持0-1的小数。此外，一部分内部参数（例如`--min-depth`）也可以修改，允许高级用户更灵活地控制软件的行为。
