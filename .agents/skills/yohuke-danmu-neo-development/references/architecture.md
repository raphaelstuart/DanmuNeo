# 项目架构与源码地图

> 本参考描述当前架构形态；版本、播放器后端和具体实现以仓库当前源码为准。

## 技术栈

- .NET 10，桌面入口为 `WinExe`。
- Avalonia 11，Fluent Theme、Inter 字体、Lucide 图标。
- CommunityToolkit.Mvvm 生成可观察属性与命令。
- Newtonsoft.Json/RestSharp 与项目自有 `BaseApi` 处理外部 API。
- Avalonia WebView 只应在确有平台浏览器需求时使用；原生视图有 Z-order 与焦点限制。
- xUnit 测试项目：`Sources/Yohuke.DanmuNeo.Tests`。

## 启动与组合根

1. `Program.cs` 创建 Avalonia AppBuilder。
2. `App.axaml.cs` 在经典桌面生命周期中创建 `MainWindow` 和 `MainWindowViewModel`，应用主题并显示窗口。
3. `MainWindowViewModel` 直接构造状态、备份、目录、账号选择、歌词、转发、播放器等服务。
4. `CreateWorkspaceViewModel`/`ConfigureRoom` 将函数和接口注入每个 `LiveRoomTabViewModel`。
5. `MainWindowViewModel.Dispose` 停止房间播放器并释放全局服务；房间级监听、转发、歌词循环也必须各自停止。

这里没有通用 DI 容器。新增依赖时优先扩展现有构造/Configure 边界，并同步测试 fake；参数过多时再设计聚合上下文，不要随意使用 Service Locator。

## 目录职责

| 目录/文件 | 职责 |
| --- | --- |
| `Apis/` | HTTP、WBI、Cookie、B 站 WebSocket、音乐 API |
| `Apis/Models/*` | 外部协议 DTO；一个文件一个顶层类型 |
| `Models/State/*` | 持久化状态、账号、工作区、房间、规则和设置 |
| `Models/Workspace/*` | 运行时 UI/业务展示模型和分享包模型 |
| `Services/*` | 状态、备份、播放器、监听工厂、发送、歌词、快捷键、导出等逻辑 |
| `ViewModels/MainWindowViewModel.cs` | 组合根、全局命令、设置分区、工作区和账号生命周期 |
| `ViewModels/Items/*` | 房间、工作区、转发规则、快捷键行等局部 VM |
| `Views/MainWindow*` | 主窗口、窗口级快捷键、尺寸和弹窗协调 |
| `Views/AttachedDialog*` | 吸附主窗口内容区的 owned window 弹窗 |
| `Views/Components/*` | 工作区、设置、侧栏、播放器/歌词/转发 UI |
| `Views/Styles/MainWindowStyles.axaml` | 全局控件样式和组件 class 样式 |
| `Publish.ps1` | win-x64、osx-arm64 自包含发布、macOS `.app` 签名和 DMG 打包 |
| `Packaging/macOS/*` | 随 DMG 分发的 macOS 首次打开与解除隔离说明 |
| `Artifacts/Publish/*` | 已忽略的本地发布产物；包含 win-x64、osx-arm64 App 和版本化 DMG |

## 状态与文件

`AppStateService` 将状态拆分到系统 ApplicationData 下的 `Yohuke.DanmuNeo` 目录：

- `settings.json`：全局设置、快捷键、符号组、替换库、布局尺寸、音乐 Cookie。
- `accounts.json`：B 站账号和 Cookie。
- `lyric-library.json`：歌词库。
- `workspaces.json`：工作区、直播间、转发规则和当前选择。
- `Backups/`：写入前自动备份，数量由设置控制。
- `Corrupt/`：解析失败文件留存。

保存使用 Normalize + 原子写入。新增字段必须：

1. 给类型安全的默认值。
2. 在 Normalize 中限制范围、补 null、移除未知引用或升级版本。
3. 考虑旧 `state.json` 与拆分存储迁移。
4. 保持账号、工作区、房间、转发规则之间的 ID 引用一致。

## 账号选择层级

`AccountSelectionService` 解析账号覆盖：直播间 → 工作区 → 全局默认。转发规则还可单独覆盖目标发送账号；源监听账号按源工作区/房间解析。删除账号时必须清理这些引用，不要留下指向已删除 ID 的配置。

## 窗口与弹窗

- `MainWindow` 使用自定义标题栏、侧栏、工作区内容和可保存尺寸/列宽。
- 新增、设置、删除确认由 `AttachedDialogCoordinator` 创建无边框 owned window，并吸附到标题栏下方内容区。
- nested modal 的堆叠顺序由协调器的创建/激活策略决定；不要只在 AXAML 里加 ZIndex。
- 主窗口移动/缩放会高频触发位置同步。合并更新并只写变化的 bounds，避免 owned window 抖动和滞后。
- 原生 WebView/NativeControlHost 可能永远压在 Avalonia 视觉层上。需要弹窗覆盖时，优先使用 owned TopLevel，或明确暂停/卸载/替换原生控件。

## 主要功能域

- 工作区与直播间：侧栏树、Tabs、房间资料、Owner UID、头像缓存、账号覆盖、分享导入导出。
- 同传：草稿、标记、屏蔽词替换、发送间隔、历史和 Excel 导出。
- 歌词：歌词库、在线导入、时间轴、播放速率、0.5 秒微调、滚动/进度、已发送去重。
- 转发：源房间、发送人 UID、正则、符号组、目标账号覆盖、独立 WebSocket 生命周期。
- 快捷键：目录定义、持久化绑定、录制、冲突检测、窗口 Tunnel 分发和输入焦点边界。
- 直播播放器：取流/清晰度与播放宿主分层；当前后端与资源必须从 `ILivePlayerService`、项目文件和源码核对。
- 运维：启动日志、缓存、配置目录、自动备份、全量导出、发布脚本。
