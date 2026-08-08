Yohuke Danmu Neo for macOS
首次打开与解除隔离说明

本版本为独立分发版本，未经过 Apple 公证。macOS 可能提示“无法验证开发者”、
“Apple 无法检查其是否包含恶意软件”或“应用已损坏”。请只在确认安装包来源可信时继续。

一、安装

1. 将“Yohuke Danmu Neo.app”拖入同一窗口中的“Applications”快捷方式。
2. 从“应用程序”文件夹启动应用。

二、推荐打开方式

1. 在 Finder 中按住 Control 点击“Yohuke Danmu Neo.app”。
2. 选择“打开”，然后在确认窗口中再次选择“打开”。
3. 如果没有“打开”按钮，先尝试正常启动一次，再前往：
   系统设置 → 隐私与安全性 → 安全性 → 仍要打开。

三、命令行解除本应用的隔离属性

打开“终端”，执行：

xattr -dr com.apple.quarantine "/Applications/Yohuke Danmu Neo.app"
open "/Applications/Yohuke Danmu Neo.app"

如果第一条命令提示权限不足，可仅为该应用使用管理员权限：

sudo xattr -dr com.apple.quarantine "/Applications/Yohuke Danmu Neo.app"

不要对整个“/Applications”目录执行 xattr，也不要使用 sudo 直接运行应用。

四、全局开启“任何来源”（不推荐）

该操作会降低整台 Mac 的安全保护，仅在你了解风险时使用：

sudo spctl --global-disable

较旧的 macOS 可能需要使用：

sudo spctl --master-disable

执行后可在“系统设置 → 隐私与安全性”中检查“任何来源”选项。
完成应用首次打开后，建议重新选择“App Store 与已知开发者”，恢复 Gatekeeper 保护。
