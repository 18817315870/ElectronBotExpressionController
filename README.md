# ElectronBotExpressionController

ElectronBotExpressionController 是一个 Windows 桌面程序，用来监听系统音频会话里的通知声音，并让 ElectronBot 显示对应表情、播放动作。它适合把微信、QQ 等应用的提示音转换成机器人提醒。

## 功能

- 监听 Windows 音频会话，根据程序名或会话名关键字匹配通知来源。
- 命中通知时切换 ElectronBot 脸部图片，并播放一段提醒动作。
- 支持多条通知规则、自定义表情图片和动作 JSON。
- 支持按时间触发的日程提醒动作。
- 支持开机启动快捷方式和托盘隐藏。

## 环境要求

- Windows 10 2004 或更新版本。
- .NET 9 SDK，用于从源码构建。
- ElectronBot 硬件和对应 USB 驱动环境。

## 从源码构建

```powershell
dotnet restore .\decompiled-src\ElectronBotExpressionController.csproj
dotnet build .\decompiled-src\ElectronBotExpressionController.csproj -c Release
```

构建产物位于：

```text
decompiled-src\bin\Release\net9.0-windows10.0.19041.0\
```

如果需要生成发布目录：

```powershell
dotnet publish .\decompiled-src\ElectronBotExpressionController.csproj -c Release -r win-x64 --self-contained false
```

## 运行

1. 运行构建或发布目录中的 `ElectronBotExpressionController.exe`。
2. 插上 ElectronBot，程序会尝试自动连接机器人。
3. 点击“测试提醒”，确认机器人能显示图片并执行提醒动作。
4. 在“通知规则”中配置通知名称、声音关键字、显示表情和触发动作。

首次运行时，程序会在可执行文件同目录生成 `notification-settings.json`。你也可以参考 `notification-settings.example.json` 手动创建配置。

## 配置说明

- `Name`：命中时显示在状态和日志中的通知名称。
- `Keywords`：匹配系统音频会话名，可填写多个关键字。
- `FacePath`：通知命中时显示到机器人脸部的图片路径，支持 PNG/JPG/BMP。
- `ActionPath`：通知命中时播放的动作 JSON 路径。
- `SoundThresholdPercent`：声音峰值阈值，默认 `2`。抓不到时调低，误触发时调高。
- `CooldownSeconds`：一次触发后短时间内不重复触发。
- `FrameIntervalMs`：通知动作每帧之间的间隔。

## 目录结构

```text
Assets/                         表情、动作和日程提醒图片
decompiled-src/                 C# WinForms 源码和解决方案
notification-settings.example.json 公开示例配置
README.md                       项目说明
```

## 发布到 GitHub

仓库已经配置 `.gitignore`，会排除本机日志、构建产物、调试符号、发布包、VS 缓存和个人配置。建议把源码和资源提交到仓库，把编译后的 `.exe` 和依赖 DLL 作为 GitHub Releases 附件发布。
