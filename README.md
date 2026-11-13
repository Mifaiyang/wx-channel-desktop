<div align="center">
  <img src="src/app-icon.png" width="88" alt="视频号下载助手图标">
  <h1>视频号下载助手</h1>
  <p><strong>让喜欢的视频，轻松留在电脑里。</strong></p>
  <p>一个为 Windows 微信视频号设计的桌面下载助手</p>
  <p>
    <a href="https://github.com/Mifaiyang/wx-channel-desktop/releases/tag/v0.1.0"><strong>下载 Windows 预览版</strong></a>
    &nbsp;&nbsp;·&nbsp;&nbsp;
    <a href="#开始使用">开始使用</a>
    &nbsp;&nbsp;·&nbsp;&nbsp;
    <a href="#构建项目">构建项目</a>
  </p>
  <p><sub>WINDOWS &nbsp; / &nbsp; DESKTOP EXPERIENCE &nbsp; / &nbsp; OPEN SOURCE</sub></p>
</div>

<br>

![视频号下载助手主界面，展示就绪状态与彩色流光](docs/preview.png)

<br>

## 关于这个作品

视频号下载助手是一个 Windows 桌面作品，围绕日常保存视频的需求，重新设计了启动、状态反馈和使用引导。深色界面、点阵字样和多彩流光让等待服务就绪的过程更直观，也赋予工具更鲜明的视觉体验。

图形界面由 **Mifaiyang · 皓舟**制作，下载能力基于开源项目 [nobiyou/wx_channel](https://github.com/nobiyou/wx_channel)。

## 体验设计

- **主动启动** — 点击按钮启动服务，经过约 3 秒的唤醒动效后进入服务启动流程。
- **流光反馈** — 彩色波形随服务状态变化，下载时呈现动态反馈，动画以 60 Hz 为刷新目标。
- **清晰的运行状态** — 在界面查看就绪状态和监听端口，下载期间保持窗口打开即可。
- **面向 Windows 屏幕** — 支持高 DPI 显示，并提供自定义窗口与应用图标。
- **开箱使用** — 解压后从根目录启动，配套使用及问题排查说明。

## 开始使用

1. 前往 [v0.1.0 发布页面](https://github.com/Mifaiyang/wx-channel-desktop/releases/tag/v0.1.0)，在 Assets 中下载 `wx-channel-desktop-v0.1.0-windows.zip`。
2. 完整解压后，双击根目录的 **视频号下载助手.exe**。
3. 点击 **启动服务**，等待界面显示就绪。
4. 打开电脑微信并进入视频号，在视频页面点击下载，选择所需画质。
5. 返回助手，点击 **打开下载文件夹** 查看文件。

使用过程中请保持助手窗口打开。首次运行时，下载内核可能提示安装用于 HTTPS 代理的根证书并调整代理设置。程序目前未签名，Windows 11 智能应用控制可能阻止运行。详细操作与排查方法见压缩包内的说明。

## 构建项目

仓库包含桌面启动器源码、资源和构建脚本。环境要求及构建步骤见 [BUILD.md](BUILD.md)。下载内核来自上游项目，发布包使用的内核版本为 v5.7.16。

> v0.1.0 的发布标签创建于源码上传之前。获取完整源码请从仓库默认分支下载，或克隆本仓库。

## 致谢与许可

感谢 [nobiyou/wx_channel](https://github.com/nobiyou/wx_channel) 的作者与贡献者提供下载内核。本项目与上游项目独立维护，上游许可见 [THIRD-PARTY-LICENSE.txt](THIRD-PARTY-LICENSE.txt)，本仓库许可见 [LICENSE](LICENSE)。

<br>

<div align="center">
  <sub>设计与制作 · <a href="https://github.com/Mifaiyang">Mifaiyang · 皓舟</a></sub>
</div>
