# ElectronBotExpressionController

ElectronBotExpressionController 是一个 Windows 桌面程序，用来监听微信、QQ 等应用的通知声音，并让 ElectronBot 显示表情、播放提醒动作。
基于绿萌大佬的源代码开发：https://github.com/maker-community/ElectronBot.DotNet

## 普通用户下载

不要点绿色 `Code -> Download ZIP`，那个下载的是源码。

请在仓库右侧 `Releases` 里下载：

```text
ElectronBotExpressionController-v1.0.0-win-x64.zip
```

下载后先解压整个 ZIP，再双击：

```text
ElectronBotExpressionController.exe
```

更详细的运行说明见 [USAGE.md](USAGE.md)。

## 主要功能

- 监听 Windows 系统音频会话中的通知声音。
- 支持微信、QQ、TIM 等关键字匹配。
- 通知命中后切换 ElectronBot 脸部图片。
- 通知命中后播放一段机器人动作。
- 支持自定义通知规则、表情图片和动作 JSON。
- 支持按时间触发日程提醒。

- 由于监听系统通知音频，所以需要打开新消息通知声音

<img width="1202" height="839" alt="截图_20260603174908" src="https://github.com/user-attachments/assets/46cf7845-800f-48a6-afcd-e9566e57ec99" />


## 效果演示


https://github.com/user-attachments/assets/54227846-a035-41a6-8ad5-101c722805db


## 开发者构建

需要 Windows 10 2004 或更新版本，以及 .NET 9 SDK。

## 目录说明

```text
Assets/                             表情图片、动作 JSON、日程提醒图片
decompiled-src/                     C# WinForms 源码
notification-settings.example.json  示例配置
USAGE.md                            使用说明
```

