# 命令行版本

对于大多数Linux用户而言，可以直接下载Sourceforge上提供的二进制软件包。命令行版本的使用方法请参见[用法](#用法)。

## 构建

第一步，安装系统的默认C++编译器和[zlib](https://zlib.net/)。在Ubuntu 24.04 或更新版本上，可以运行下面的命令：

```
sudo apt install build-essential zlib1g zlib1g-dev
```

高级用户可以选择安装[zlib-ng](https://github.com/zlib-ng/zlib-ng)或[cloudflare-zlib](https://github.com/cloudflare/zlib)代替zlib以提高性能。

第二步，安装[Haxe 4](https://haxe.org/)并[配置C++目标](https://haxe.org/manual/target-cpp-getting-started.html)。在Ubuntu 24.04 或更新版本上，运行下面的命令：

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

最后，将GeneMiner2的源代码下载到GeneMiner2文件夹中，构建可执行文件。

```
git clone --depth 1 https://gitee.com/sculab/GeneMiner2.git
cd GeneMiner2
make
```

编译完成的可执行文件放在`cli`目录下，运行`cli/geneminer2 -h`查看帮助。软件在运行时仍然需要几个独立的生物信息学工具。用conda安装的方法如下：

```
conda install -c bioconda aster blast clustalo fasttree iqtree mafft magicblast minimap2 muscle raxml-ng trimal veryfasttree
```

## 用法

GeneMiner2需要tsv格式的样本列表和FASTA格式的参考序列。样本列表的具体格式是`<物种名><Tab><数据文件1>`（单端）或者`<物种名><Tab><数据文件1><Tab><数据文件2>`（双端），每一行代表一个样本。其中的数据文件建议采用绝对路径。

假设代码的位置在`/home/user/GeneMiner2`，[DEMO 3](../../DEMO/DEMO3/DEMO3.md)的样本列表大概是这样：

```
Bupleurum_chinense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_chinense_2.fq.gz
Bupleurum_fruticosum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_fruticosum_2.fq.gz
Bupleurum_krylovianum	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_krylovianum_2.fq.gz
Bupleurum_malconense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_malconense_2.fq.gz
Bupleurum_wenchuanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_wenchuanense_2.fq.gz
Bupleurum_yunnanense	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_1.fq.gz	/home/user/GeneMiner2/DEMO/DEMO3/DATA/PLANT/Bupleurum_yunnanense_2.fq.gz
```

参考序列需要放在一个独立的文件夹下，每个基因可以有一条或多条参考序列。对于每个基因，将对应的所有参考序列存在`<基因名>.fasta`下。假设需要提取matK和psbA两个基因，则需要在一个空白文件夹下创建`matK.fasta`和`psaA.fasta`两个文件，分别保存对应的参考序列。

假设样本列表保存在`/home/user/GeneMiner2/DEMO/DEMO3/samples.tsv`，被子植物353参考基因保存在`/home/user/Angiosperm353`，期望的输出文件夹为`/home/user/GeneMiner2/DEMO/DEMO3/output`，用默认设置运行GeneMiner2的命令如下：

```
cli/geneminer2 -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /home/user/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output
```

执行这行命令后，GeneMiner2会构建一棵溯祖树，路径为`/home/user/GeneMiner2/DEMO/DEMO3/output/Coalescent.tree`。

所有命令行参数的说明如下：

- `-f`: 样本列表tsv文件
- `-r`: 参考序列文件夹
- `-o`: 输出文件夹
- `-p`: 同时运行的进程数
- `--max-reads`: 每文件限制的最大读长数量，默认禁用
- `-kf`: 过滤kmer大小
- `-s`: 过滤步长
- `--max-depth`: 进一步过滤的深度限制
- `--max-size`: 进一步过滤的文件大小限制
- `-ka`: 组装kmer大小，默认为自动
- `--min-ka`: 自动估算k值的下限
- `--max-ka`: 自动估算k值的上限
- `-e`: 错误阈值
- `-sb`: 软边界，支持`0`、`auto`和`unlimited`三个选项
- `-i`: 搜索深度
- `-c`: 一致性阈值（介于0-1的小数形式）
- `-ts`: 基于参考切齐的来源序列，可以是`assembly`或`consensus`
- `-tm`: 基于参考切齐的模式，可以是`all`、`longest`、`terminal`或`isoform`
- `-tr`: 基于参考切齐的保留长度阈值（介于0-1的小数形式）
- `-cs`: 合并结果的来源序列，可以是`assembly`、`consensus`或`trimmed`
- `--msa-program`: 多序列比对的软件，支持`mafft`、`muscle`和`clustalo`
- `-cd`: 清理序列的最大差异
- `-cn`: 清理序列的最小序列数量
- `-m`: 建树的方法，支持`coalescent`和`concatenation`
- `-b`: 支持度计算的重复次数
- `--phylo-program`: 建树的软件，支持`fasttree`、`veryfasttree`、`iqtree`和`raxmlng`

例如，在上述命令运行结束后，使用`tree`子命令和`-m concatenation`参数，可以要求GeneMiner2基于之前的结果构建串联树：

```
cli/geneminer2 tree -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /home/user/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output -m concatenation
```

类似，可以通过传递子命令和参数，只运行特定的分析步骤。假设给出这些参数：

| 参数                        | 值                |
| -------------------------- | ----------------- |
| Source Sequence            | Consensus Results |
| 保留长度阈值                 | 50%               |
| Trim Method                | All Fragments     |
| 比对程序                    | muscle            |
| 最大差异大于                 | 0.2               |
| 序列数量小于                 | 5                 |

可以用下面的命令只执行**基于参考切齐**和**合并结果**：

```
cli/geneminer2 trim combine -f /home/user/GeneMiner2/DEMO/DEMO3/samples.tsv -r /mnt/data/Angiosperm353 -o /home/user/GeneMiner2/DEMO/DEMO3/output -ts consensus -tm all -tr 0.5 -cd 0.2 -cn 5 --msa-program muscle
```

所有参数和输出与图形界面的版本含义一致，除了命令行版本在需要填写百分数的位置只支持0-1的小数。此外，一部分内部参数（例如`--min-depth`）也可以修改，允许高级用户更灵活地控制软件的行为。
