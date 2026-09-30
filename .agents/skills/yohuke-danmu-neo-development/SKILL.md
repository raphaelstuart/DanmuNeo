---
name: yohuke-danmu-neo-development
description: Use when modifying, reviewing, explaining, testing, planning, or publishing the Yohuke.DanmuNeo Avalonia desktop application, including workspaces and live-room tabs, attached dialogs, settings, split state storage and backups, shortcuts, lyrics, danmu forwarding, replacement rules, the live player, exports, and win-x64 or osx-arm64 App/DMG packaging, signing, and first-launch distribution guidance.
---

# Yohuke Danmu Neo 研发

## 核心流程

1. 确认仓库根目录包含 `Sources/Yohuke.DanmuNeo.sln`。先读当前 `AGENTS.md`、`README.md`、项目文件和目标功能附近代码；以磁盘源码为准，不把历史聊天中的中间方案当作当前实现。
2. 优先使用可用的 Rider MCP 感知符号、引用和项目结构；不可用时使用 `rg`、`rg --files` 和精确文件读取。保留用户已有改动，不清理无关 diff。
3. 修改前追踪完整纵向链路：持久化模型 → `AppStateService.Normalize`/迁移 → Service → ViewModel 属性和命令 → AXAML/code-behind → 测试 → 发布资源。不要只改可见 UI。
4. 按任务读取 `references/architecture.md` 和 `references/workflows.md`。涉及 B 站登录、Cookie、HTTP API、WBI、弹幕 WebSocket、发送或直播流时，同时使用 `$yohuke-danmu-neo-bilibili-api-development`。
5. 将纯业务逻辑留在 Service/ViewModel；只把剪贴板、文件选择器、焦点、Caret、窗口、原生控件生命周期等 UI 边界放在 code-behind。
6. 修改后按 `references/validation.md` 串行验证。不要并行执行 `dotnet build` 和 `dotnet test`，Avalonia 生成过程可能争用同一 `obj` 文件。涉及 macOS 发布时还要验证签名、DMG 校验和实际挂载内容。

## 架构边界

- 将 `MainWindowViewModel` 视为当前组合根：它加载状态、创建服务、构造 Workspace/Room VM，并注入账号解析、保存、播放器、监听与转发依赖。
- 将 `AppState` 与 `Models/State/*` 视为持久化事实来源；为持久化字段提供向后兼容默认值，并在 `AppStateService.Normalize` 中处理 null、未知引用、非法数值、旧版本迁移和排序。
- 沿用 CommunityToolkit.Mvvm 的 ObservableProperty/RelayCommand 模式。不要为了一个 UI 操作把平台对象传入 ViewModel。
- 维护一个文件一个顶层类型；花括号独占一行，优先 `var` 和可推断的 `new()`，公开类型与公开方法写 XML 文档，不加复述代码的注释。
- 更新 `ObservableCollection`、Bitmap 和绑定状态时尊重 Avalonia UI 线程；后台监听、播放器、定时器和 CancellationTokenSource 必须有明确停止与 Dispose 路径。
- 不用普通 Avalonia ZIndex 假设解决原生视图覆盖。先确认控件是否为 `NativeControlHost`/`NativeWebView`，并读取当前弹窗宿主与生命周期实现。
- 不记录 Cookie、CSRF、登录回跳 URL、完整弹幕账号凭据或其他密钥；日志只记录非敏感诊断信息。

## 变更检查清单

- 新增设置时，同时检查默认值、设置页、保存触发、Normalize、旧配置、备份/导出和测试。
- 新增直播间字段时，同时检查 State、Room VM、添加/修改 UI、Workspace 分享导入导出、账号/转发引用清理和测试。
- 修改集合型 ComboBox 时避免无条件 `Clear()` 再重建；若选项未变化，保留集合与对象引用，防止 SelectedValue 被临时写空。
- 修改弹窗时检查 owned window 堆叠、主窗口移动/缩放/最小化、嵌套确认框、关闭状态回写和原生控件覆盖。
- 修改播放器/监听/转发时检查切房间、删房间、删工作区、打开设置、关闭 App、异常重试及旧会话释放。
- 修改发布资源、图标、原生库或项目文件时执行 `Publish.ps1`，并检查 win-x64、osx-arm64 App、签名、DMG 及其挂载内容，而不仅是命令退出码。

## 按需参考

- 需要定位技术栈、启动组合、目录、状态文件、窗口/弹窗或主要模块时，读取 `references/architecture.md`。
- 需要新增设置、工作区字段、快捷键、歌词、转发、播放器或导出功能时，读取 `references/workflows.md`。
- 需要选择测试、构建、发布或手动验收范围时，读取 `references/validation.md`。
