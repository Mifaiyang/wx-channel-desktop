# 视频号下载助手 · 流光版

由 [Mifaiyang · 皓舟](https://github.com/Mifaiyang) 制作的 Windows 图形助手，下载内核来自 [nobiyou/wx_channel](https://github.com/nobiyou/wx_channel)。

![视频号下载助手界面](docs/preview.png)

## 下载与使用

1. 在 [Releases](https://github.com/Mifaiyang/wx-channel-desktop/releases) 的 Assets 下载 Windows ZIP。
2. 完整解压，双击根目录的「视频号下载助手.exe」。
3. 点击「启动服务」。看到「已就绪」后，进入电脑微信的视频号。
4. 在视频页面点下载并选择画质，回到助手打开下载文件夹。

首版为预览版。程序未签名，Windows 11 智能应用控制可能拦截。首次启动时，上游内核可能申请安装用于 HTTPS 代理的根证书，并调整相关代理设置。使用步骤和排查方法见 ZIP 里的指南。

## 致谢与许可

感谢 [nobiyou/wx_channel](https://github.com/nobiyou/wx_channel) 的维护者和贡献者。图形助手由 Mifaiyang · 皓舟制作，本仓库与上游项目独立维护。随包提供的下载内核版本为 v5.7.16，遵循上游 MIT 许可证。
