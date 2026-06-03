# ElectronBotExpressionController

ElectronBotExpressionController 是一个 Windows 桌面程序，用来监听微信、QQ 等应用的通知声音，并让 ElectronBot 显示表情、播放提醒动作。

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

## 开发者构建

需要 Windows 10 2004 或更新版本，以及 .NET 9 SDK。

```powershell
dotnet restore .\decompiled-src\ElectronBotExpressionController.csproj
dotnet build .\decompiled-src\ElectronBotExpressionController.csproj -c Release
```

生成免安装发布包：

```powershell
dotnet publish .\decompiled-src\ElectronBotExpressionController.csproj -c Release -r win-x64 --self-contained true
```

## 目录说明

```text
Assets/                             表情图片、动作 JSON、日程提醒图片
decompiled-src/                     C# WinForms 源码
notification-settings.example.json  示例配置
USAGE.md                            使用说明
```

## 注意

程序运行时会在 exe 同目录生成 `notification-settings.json`、`listener.log` 和 `audio-sessions.log`。这些是本机运行文件，不需要提交到 GitHub。
