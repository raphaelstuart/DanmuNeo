# 常见功能工作流

## 新增或修改全局设置

1. 在 `Models/State/AppSettings.cs` 增加带默认值的 Property。
2. 在 `AppStateService.Normalize` 增加数值裁剪、null 修复或版本迁移。
3. 判断设置属于哪个设置页；现有分区由 `MainWindowViewModel.SettingsSectionOptions` 与 `SettingsConfigContainerView` 共同控制。
4. 直接绑定简单设置；需要副作用时由 MainWindow VM 属性/命令处理并调用 `SaveAsync`。
5. 检查导出全部备份是否已自然包含该设置；若新增独立文件则扩展备份服务。
6. 在 `AppStateServiceTests` 增加缺字段、非法值、保存/读取覆盖。

## 新增直播间或工作区字段

1. 修改 `WorkspaceState` 或 `LiveRoomTabState`。
2. 在对应 VM 暴露 Property，并写回 State、通知派生属性和触发必要刷新。
3. 补添加对话框、工作区设置页、右键修改入口。
4. 若字段影响头像、账号、监听或播放器，修改时先停止/失效旧资源，再按新值重建。
5. 同步 `WorkspaceShareRoom`/分享服务，避免导出再导入后字段丢失。
6. 删除账号、房间或工作区时清理所有跨 ID 引用和后台任务。

## 新增设置分区

1. 增加独立 View/Code-behind 文件，不把复杂设置塞进 `GlobalSettingsView`。
2. 在 `SettingsSectionOptions` 增加 key、名称、描述。
3. 在 MainWindow VM 增加 `IsXxxSettingsSelected` 派生属性并在选择变化时通知。
4. 在 `SettingsConfigContainerView` 挂载新 View。
5. 集合编辑应保持对象引用稳定；保存 Normalize 不要替换 UI 正在绑定的对象，除非同时刷新 VM。

## 修改弹窗

1. 用 MainWindow VM 的布尔状态和命令表达打开/关闭。
2. 在 `AttachedDialogCoordinator.SyncDialogs` 接入新弹窗或调整 owned window 堆叠。
3. 弹窗 View 复用主 VM DataContext；平台文件选择器、窗口 API 放在 code-behind。
4. 从设置窗口打开删除确认时，确保确认窗口最后激活且不会被设置窗口压住。
5. 验证主窗口移动、缩放、最小化、恢复和关闭时的同步与资源释放。

## 修改快捷键

1. 在 `ShortcutActionKeys` 增加稳定 ActionKey。
2. 在 `ShortcutActionCatalog` 定义分组、显示名和默认手势。
3. 让 `ShortcutBindingService.Normalize` 补齐旧配置、保留明确禁用/清空、迁移旧默认值并检测冲突。
4. 在窗口 Tunnel 阶段分发全局应用内快捷键；原生 WebView 获得焦点时可能吞键，需要单独桥接或明确限制。
5. 输入框焦点下默认不劫持编辑键；明确允许的“清空输入”等动作除外。
6. 长按动作使用 timer，KeyUp、页面卸载、焦点丢失时停止。

## 修改歌词

1. 解析留在 `LyricTimelineService`，播放时钟和发送会话留在 `LiveRoomTabViewModel`。
2. 时间定位后统一刷新 Active 行、Progress、滚动目标和播放时钟。
3. `IsSent` 是播放会话状态；重新 Apply/Load/Clear 歌词时清空。`PreventRepeatedLyricSend` 是房间持久化偏好。
4. 播放速率只缩放时钟/延迟，不改变原始时间戳。
5. UI 自动滚动由 View 监听 Active 项并调用 `ScrollIntoView`；不要把 ListBox 传入 VM。
6. 测试使用短时间轴或纯计算方法，避免每个用例等待数秒。

## 修改转发

1. 持久化字段在 `DanmuForwardRuleState`，通知与重启入口在 `DanmuForwardRuleViewModel`/Room VM。
2. 来源账号按源工作区/房间解析；发送账号优先规则覆盖，否则使用目标房间账号。
3. 任何规则变更都可能停止并重启 socket；先验证是否真的需要重启，避免每次输入字符产生网络抖动。
4. `RefreshForwardSourceRooms` 不应无条件 Clear。来源 key 未变化时保持集合和 SelectedValue；来源消失时才重建并显示明确状态。
5. 从弹幕右键生成规则时默认禁用，来源、UID、目标写入完整但不立即连接。
6. 每条启用规则持有独立 CancellationTokenSource；删除、禁用、切换目标和关闭 App 时停止。

## 修改直播播放器

1. 先读取当前 `ILivePlayerService`、`LivePlayerService`、`BilibiliLiveStreamService`、Room VM 和工作页，不依据历史方案判断后端。
2. 保持取流/清晰度、会话 token、UI 宿主三层边界。追帧必须撤销旧会话并生成新流，不能让旧音频继续运行。
3. 播放/停止、静音、音量和清晰度的持久化位置要明确；音量变化不应重新取流。
4. 错误和未开播状态必须可观察，不要吞异常后只隐藏画面。
5. NativeWebView 路径必须检查 Z-order、autoplay、MSE、worker 绝对 URL、销毁 JS、声音重叠和本地代理生命周期。
6. 内存绘制/native 解码路径必须检查原生运行库、帧缓冲 UI 线程更新、音频停止、发布复制与签名。

## 修改 macOS 发布

1. 先读根目录 `Publish.ps1`、项目文件和 `Packaging/macOS/首次打开说明.txt`；版本默认从项目 `AssemblyVersion` 解析，也可通过 `-Version` 覆盖。
2. 保持 App Bundle 的 `Info.plist`、arm64 主程序、`.icns` 图标和发布资源完整；修改原生库时同时检查架构与签名封装。
3. `-MacCodeSignIdentity` 默认为 `-`，表示 ad-hoc 签名。传入 Developer ID Application 证书时启用 hardened runtime 和 timestamp，但这仍不等于 Apple 公证。
4. DMG 必须包含 `Yohuke Danmu Neo.app`、指向 `/Applications` 的符号链接和 `首次打开说明.txt`。使用独立临时 staging 目录、`ditto` 复制 App，再用 `hdiutil create -format UDZO` 生成压缩镜像。
5. 说明文档按安全性优先级给出 Finder 右键“打开”、系统设置“仍要打开”、仅针对单个 App 的 `xattr -dr com.apple.quarantine` 以及全局“任何来源”；明确标记全局关闭 Gatekeeper 不推荐。
6. 不把签名密码、公证凭据或 Apple 账号写入脚本。若后续加入公证，使用 Keychain Profile 或 CI Secret，并增加 notarization、stapler 和 Gatekeeper 验收。

## 平台 UI 边界

以下操作保留在 View/code-behind，并调用 VM 的纯逻辑入口：

- Save/Open file picker 与选择路径。
- Clipboard。
- TextBox Focus、CaretIndex 和点击弹幕插入。
- ContextMenu 的动态工作区/直播间层级。
- Window Topmost、尺寸、位置和 owned window。
- NativeWebView 环境、CookieManager 与平台配置。

## 异步和生命周期

- 使用 CancellationToken 结束监听、转发、播放器代理和自动歌词。
- fire-and-forget 必须在内部捕获异常并回写状态；不要让 async void 承载业务流程，事件处理器除外。
- UI 集合与绑定属性从后台线程变化时使用 `Dispatcher.UIThread`。
- 事件订阅必须在 DataContext 切换、VisualTree Detach 或 Dispose 时解除。
