# 桌面Linux系统的运行方法

## 动机

GeneMiner2 致力于为初学者提供友好的使用体验。尽管可以编译[原生Linux版本](command_line.md)，我们推荐桌面Linux用户运行带有图形界面的Windows版。目前来说，[Proton](https://github.com/ValveSoftware/Proton)是兼容性最好的工具，可以用[Steam](https://store.steampowered.com/)或者[umu-launcher](https://github.com/Open-Wine-Components/umu-launcher)来安装。用其他Wine前端也是可行的，例如[PlayOnLinux](https://www.playonlinux.com/)，或者完全手动配置[Wine](https://www.winehq.org/)本体，但这样需要更多的配置工作。

## Steam

1. 安装[Steam](https://store.steampowered.com/)并注册账号。

2. 从[Sourceforge](https://sourceforge.net/projects/geneminer/files/)下载最新的Windows版本并解压。

3. 打开Steam，选择库。点击左下角的“添加游戏”，选择“添加非 Steam 游戏...”。

    ![](../../images/steam/cn-add-menu.png)

4. 点击左下角的“浏览...”，选择`GeneMiner.exe`。点击“添加所选程序”把GeneMiner2添加到库。

    ![](../../images/steam/cn-add-dialog.png)

5. 右键点击刚添加的"GeneMiner.exe"，选择菜单的“属性...”。

    ![](../../images/steam/cn-game-menu.png)

6. 在弹出窗口中，切换到“兼容性"，勾选“强制使用特定 Steam Play 兼容性工具”，在下面的下拉框选择“Proton 9.0-4”。

    ![](../../images/steam/cn-prop-dialog.png)

7. 点击“开始游戏”运行GeneMiner2。

另外，通过Steam打开GeneMiner2的时候，可以给Steam好友发一条“正在玩 GeneMiner2”消息，从而促进友情的结束。

## umu-launcher

假设GeneMiner2已经解压到`~/geneminer2`，可以运行这行命令启动GeneMiner2。

```
WINEPREFIX=~/.wine umu-run ~/geneminer2/GeneMiner.exe
```
