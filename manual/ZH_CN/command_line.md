# 命令行版本

## 构建

如果要在Linux环境中运行GeneMiner2，需要手动进行构建。一般而言，编译的二进制文件可以在相同或者更新的Linux发行版上运行。

首先，安装[Haxe 4](https://haxe.org/)并[配置C++目标](https://haxe.org/manual/target-cpp-getting-started.html)。本质上是执行这两行命令并按提示操作。

```
haxelib setup
haxelib install hxcpp
```

接下来安装依赖项。可以用下面的命令通过conda安装，也可以手动用系统的软件包管理器安装。

```
conda create -n geneminer python=3.11 numpy=2.1.3 biopython matplotlib pyinstaller scipy
conda activate geneminer
```

最后构建软件包。

```
make
```

二进制文件放在`cli`目录下。

## 用法

编译得到的二进制文件只依赖几个重要的库，对依赖的要求较少。不过，运行环境还是需要几个重要工具。用conda安装的方法如下：

```
conda install -c bioconda blast clustalo mafft minimap2 muscle trimal
```

只要这些工具在`PATH`中能够找到，GeneMiner2就能正常运行。可以通过把工具放在`cli/bin`下的方法强制GeneMiner2使用特定的可执行文件。例如，创建符号链接`cli/bin/mafft`会强制GeneMiner2使用链接的目标作为mafft的可执行文件。

```
ln -s /home/user/.local/bin/mafft cli/bin/mafft
```

GeneMiner的入口点是`cli/geneminer2`。在运行之前，需要准备一个tsv格式的样本列表。具体格式是`<物种名><Tab><数据文件1>`（单端）或者`<物种名><Tab><数据文件1><Tab><数据文件2>`（双端），每一行代表一个样本。例如，要处理 _Aegopodium podagraria_ 的双端测序数据和 _Torilis scabra_ 的单端测序数据，tsv的内容大概是这样：

```
Aegopodium_podagraria	/mnt/data/Aegopodium_podagraria_1.fq.gz	/mnt/data/Aegopodium_podagraria_2.fq.gz
Torilis_scabra	/mnt/data/Torilis_scabra.fq.gz
```

假设样本列表保存在`/mnt/data/samples.tsv`，被子植物353参考基因保存在`/mnt/data/angiosperm353`（每个基因一个FASTA文件），用默认设置运行GeneMiner2的命令如下：

```
cli/geneminer2 -f /mnt/data/samples.tsv -r /mnt/data/angiosperm353 -o /mnt/data/output
```

特别注意，命令行版本**不支持**建树。你需要手动用`combined_trimed.fasta`构建串联树，或者用`combined_trimed`下的所有FASTA文件构建溯祖树。

只运行某些步骤也是可行的。例如，假设给出这些参数：

| 参数                        | 值                |
| -------------------------- | ----------------- |
| Source Sequence            | Consensus Results |
| 保留长度阈值                 | 50%               |
| Trim Method                | All Fragments     |
| 比对程序                     | muscle            |
| 最大差异大于                 | 0.2               |
| 序列数量小于                 | 5                 |

可以用下面的命令执行**基于参考切齐**和**合并结果**：

```
cli/geneminer2 -f /mnt/data/samples.tsv -r /mnt/data/angiosperm353 -o /mnt/data/output -t consensus -n 0.5 -m all --msa-program muscle -d 0.2 -q 5 trim combine
```

运行`cli/geneminer2 -h`查看所有命令行参数。所有参数和输出与图形界面的版本含义一致，除了命令行版本在需要填写百分数的位置只支持0-1的小数。此外，一部分内部参数（例如`--min-depth`）也可以修改。这些改动有助于提高大规模运行的灵活性。
