# 验证与发布

## 基础命令

从仓库根目录串行执行：

```bash
dotnet restore Sources/Yohuke.DanmuNeo.sln
dotnet build Sources/Yohuke.DanmuNeo.sln --no-restore
dotnet test Sources/Yohuke.DanmuNeo.sln --no-restore
```

依赖未变化且资产已还原时可跳过 restore。不要并行运行 build/test；Avalonia XAML 生成可能争用 `obj` 文件。

## 测试落点

| 变更 | 首选测试 |
| --- | --- |
| 状态默认、迁移、裁剪、备份 | `AppStateServiceTests`, `AppBackupServiceTests` |
| 账号覆盖 | `AccountSelectionServiceTests`, `MainWindowViewModelTests` |
| 浏览器 Cookie | `BrowserCookieLoginServiceTests` |
| B 站直播协议 | `BilibiliLiveWebSocketTests`, `BilibiliLivePlayInfoSelectorTests` |
| 房间命令、歌词、播放、转发 | `LiveRoomTabViewModelTests` 与 fake 服务 |
| 播放器页面/代理 | `LivePlayerServiceTests` |
| 转发匹配 | `DanmuForwardServiceTests` |
| 歌词解析/库 | `LyricTimelineServiceTests`, `LyricLibraryServiceTests` |
| 快捷键 | `ShortcutBindingServiceTests` |
| 替换/符号 | `ShieldReplacementServiceTests`, `MarkSymbolServiceTests` |
| 工作区分享 | `WorkspaceShareServiceTests` |
| 历史 Excel | `TranslateHistoryExportServiceTests` |

使用接口和 fake 隔离网络、WebSocket、播放器与发送次数。不要在单元测试中访问真实 B 站账号或真实直播间。

## 手动验证

根据风险选择：

- AXAML/布局：不同窗口尺寸、splitter、选中/禁用状态、长文本和空集合。
- owned window：移动、缩放、最小化、恢复、设置上再开删除确认。
- 输入：中文 IME、Enter、Delete/Backspace、Caret 插入、快捷键冲突和长按释放。
- 歌词：加载、重载清除 IsSent、自动滚动、速率、微调、防重发开关。
- 转发：启用/禁用、编辑 UID/正则、来源下拉保持、账号缺失、删除来源房间。
- 播放器：播放、停止、追帧、清晰度、音量、打开弹窗、切换房间、关闭 App、断流恢复。

## 发布

涉及 `.csproj`、Assets、图标、原生库、播放器或平台行为时执行：

```bash
pwsh -NoProfile -File ./Publish.ps1 -Clean
```

检查：

- `Artifacts/Publish/win-x64` 中 exe、配置和必要 native runtime。
- `Artifacts/Publish/osx-arm64/Yohuke Danmu Neo.app` 的 `Info.plist`、图标、主程序、资源和必要 dylib/plugins。
- `Artifacts/Publish/Yohuke-Danmu-Neo-{version}-osx-arm64.dmg` 已生成，文件名与项目/覆盖版本一致。
- macOS 原生资源架构、rpath、xattr 和 codesign；ad-hoc 签名通过不代表已公证。
- `hdiutil verify` 通过，并以只读方式挂载 DMG 验收 App、`Applications -> /Applications` 和中文说明文档。
- 发布目录受 `.gitignore` 管理，不把二进制产物提交到仓库，除非它们是明确 vendored 的源码依赖。

macOS 发布的建议验收命令：

```bash
file "Artifacts/Publish/osx-arm64/Yohuke Danmu Neo.app/Contents/MacOS/Yohuke.DanmuNeo"
codesign --verify --deep --strict --verbose=2 "Artifacts/Publish/osx-arm64/Yohuke Danmu Neo.app"
hdiutil verify "Artifacts/Publish/Yohuke-Danmu-Neo-{version}-osx-arm64.dmg"
```

使用 `mktemp -d` 创建精确的临时挂载点，记录 `hdiutil attach -readonly -nobrowse` 返回的设备节点，验收后卸载该精确设备并删除仅由本次验收创建的临时目录。对说明文档使用 `cmp` 检查打包前后一致性，并在交付时报告 DMG 大小、SHA-256、签名类型和是否公证。

## 工作树安全

- 先运行 `git status --short`，区分用户已有改动与本次修改。
- 不回退、覆盖或格式化无关文件。
- Rider 格式化后检查 diff，尤其是 AXAML tab/空格和 C# raw string。
- 使用 `git diff --check` 检查空白；最终只汇报本次实际修改和真实验证结果。
