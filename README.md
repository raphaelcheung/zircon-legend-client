# 皓石传奇三  Zircon Mir3

本开源项目仅供学习游戏技术，禁止商用以及非法用途。

## 简介

### 完整的传奇三游戏

- 含了四个职业：战士、法师、道士、刺客<br/>
<img src="Images/biqi.jpg" title="比奇城内截图"><br/>
<img src="Images/fashi.jpg" title="法师截图"><br/>
<img src="Images/cike.jpg" title="刺客截图"><br/>
	
- 技能丰富，平均每个职业有 38 个技能<br/>
<img src="Images/lianyue.jpg" title="莲月剑法截图"><br/>


- 地图和道具及其丰富，玩到 100级没压力；

- 技能正常修炼到 3级以后，还可通过打出高等级技能书一直升到 6级；

- 武器和首饰均可精炼，品质高的装备精炼上限也更高；

- 法师招宠与道士的宠物最高可升至暗金等级，各项属性翻倍，非常实用；

- 刺杀剑术破防之余，技能等级越高，刺杀剑术的攻速越快，爽之又爽；

### 支持多平台部署

服务端支持在 Linux、Windows、Docker 平台上部署。

<img src="Images/docker.jpg" title="Docker 运行截图">
<br/>
	
### 便捷传送

每个传送石都可以方便地传送到任意地图。<br/>
<img src="Images/chuansong.jpg" title="Docker 运行截图">
<br/>

## 服务器部署

参见项目 【[ZirconLegend-Server](https://gitee.com/raphaelcheung/zircon-legend-server)】

## 客户端运行

### 下载依赖数据

包含了海量地图和道具资源，压缩后仍有 3GB大小，只能通过百度网盘来分享。

【[百度网盘分享 2024-8-14](链接：https://pan.baidu.com/s/1OMkb834cOtxF8KIrlJMKRQ?pwd=h1bv)】


### 安装依赖组件

- .Net Framework 4.8

- DirectX 9.0

### 下载执行文件

从本项目发布页面下载最新的运行文件，与前面已下载的依赖数据文件解压到同一目录。

根据自己的情况修改` Legend.ini `中的服务器地址、端口。

客户端窗口化运行还存在一些 UI 问题，建议使用全屏模式，分辨率采用显示器推荐分辨率。

## 代码编译

开发环境依赖：

- Microsoft Visual Studio Community 2022

- .Net Framework 4.8

运行时还依赖：

- DirectX 9.0

安装这些后拉取全库代码，拉取的时候要选中` Recursive `。这样才能把子模块一并拉取下来。

项目的编译依赖都已预设好，直接编译即可