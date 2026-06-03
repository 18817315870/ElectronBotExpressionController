# 使用说明

## 下载哪个文件

如果你只是想运行程序，请下载 Releases 里的：

```text
ElectronBotExpressionController-v1.0.0-win-x64.zip
```

不要下载 GitHub 自动生成的 `Source code.zip`，也不要点绿色 `Code -> Download ZIP`。那些是源码，不能直接当成普通软件使用。

## 第一次运行

1. 把 ZIP 解压到一个固定目录，例如：

   ```text
   D:\Tools\ElectronBotExpressionController
   ```

2. 进入解压后的文件夹。
3. 双击运行：

   ```text
   ElectronBotExpressionController.exe
   ```

4. 插上 ElectronBot。
5. 等程序显示已连接。
6. 点击“测试提醒”，确认机器人能显示图片并执行动作。

不要只把 `ElectronBotExpressionController.exe` 单独拖到桌面运行。`Assets` 文件夹必须和 exe 放在同一个目录。

## 配置通知规则

程序默认会监听微信、QQ、TIM 一类通知声音。你也可以自己添加规则：

1. 点击“新增”。
2. 填写通知名称，例如 `QQ`、`微信`、`企业微信`。
3. 让目标软件播放一次通知声音。
4. 点击“捕获当前声音”。
5. 选择通知命中时显示的表情图片。
6. 选择通知命中时播放的动作。
7. 点击“保存”。

如果抓不到通知声音，可以把阈值调低一点。如果误触发太多，可以把阈值调高一点。

## 日程提醒

程序支持按时间触发动作，例如上班、吃饭、休息、下班提醒。

这些配置保存在：

```text
notification-settings.json
```

你可以在程序界面里修改，也可以参考仓库里的：

```text
notification-settings.example.json
```

## 常见问题

### Windows 提示风险怎么办

如果出现 SmartScreen 提示，可以点“更多信息”，再点“仍要运行”。

这是因为程序没有购买代码签名证书，不代表一定有病毒。

### 双击 exe 后图片不显示

确认 `Assets` 文件夹还在 exe 同目录。

正确结构应该类似：

```text
ElectronBotExpressionController.exe
Assets\
RUNNING.txt
```

### GitHub 上源码 ZIP 能不能直接用

不建议。源码 ZIP 是给开发者看的，需要 .NET SDK 构建。

普通用户请下载 Releases 里的 `win-x64.zip`。

### 程序会生成哪些文件

运行后可能生成：

```text
notification-settings.json
listener.log
audio-sessions.log
```

这些是你的本机配置和日志，不需要上传到 GitHub。
