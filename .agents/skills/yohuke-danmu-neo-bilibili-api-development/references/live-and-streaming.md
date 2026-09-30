# 直播 WebSocket、取流与转发

## WebSocket 建连

当前入口：`BilibiliLiveWebSocket` 实现 `ILiveDanmuSocket`，生产由 `BilibiliLiveDanmuSocketFactory` 创建，测试使用 fake factory/socket。

流程：

1. `GetDanmuInfoAsync` 对 `getDanmuInfo` 请求做 WBI 签名，参数包含 room ID、type、web_location。
2. 从响应选择 host，创建 `wss://{host}:{wssPort}/sub`。
3. ClientWebSocket 设置 User-Agent、live Origin、标准 Cookie。
4. 发送 enter-room binary packet，token 来自 danmu info。
5. 每 30 秒发送固定 heartbeat packet。
6. 循环接收完整 binary message，按 16 字节 header 解包。

## 包解析

Header 使用大端字段：packet length、header length、version、operation、sequence。

- operation 3：人气/心跳响应，当前忽略。
- operation 5：业务消息。
- version 0/1：直接解析业务 JSON。
- version 3：Brotli 解压后递归解析。
- 一个 WebSocket message 可能串联多个 packet；按 packet length 递归拆分。

当前关注命令：

- `DANMU_MSG*`：从 `info` 读取 UID、用户名、内容。
- `SUPER_CHAT_MESSAGE*`：从 `data` 读取用户、价格、内容、时间。
- 普通弹幕还会尝试按同传格式正则发布 `TranslatedDanmuReceived`。

扩展 command/version 时先保存脱敏 fixture，给静态 parser 增加测试。不要把未知包直接当字符串吞掉；先检查长度、version 和 operation。

## 重连与取消

`StartAsync` 在取消前循环建连：

- 首次异常触发 Error 与 Disconnected。
- 约 2 秒后重试。
- 后续成功触发 Recovered。
- 用户取消抛出的 OperationCanceledException 正常结束，不报告错误。

Room VM 每次 StartListening 创建 CancellationTokenSource。Stop、切换/删除房间和 App Dispose 必须取消；事件回调更新 Avalonia 属性时切 UI 线程。

## 播放信息与流选择

`BilibiliLiveStreamService`：

1. 验证公开 room ID。
2. 调 `GetRoomPlayInfoAsync`，传选择的 qn、Cookie、timeout。
3. 检查业务 code。
4. 交给 `BilibiliLivePlayInfoSelector.Select`。

当前 selector：

- `LiveStatus != 1` 区分未开播与轮播。
- 要求 `PlayUrlInfo.PlayUrl`。
- 优先 `protocol=http_stream` → `format=flv` → `codec=avc`。
- 从 host + base_url + extra 拼流 URL。
- 清晰度列表取 codec.accept_qn 与 g_qn_desc 的交集。

不要简单取第一个 stream/codec；新增 HLS/HEVC 回退时保留明确优先级和平台能力测试。

## 本地直播代理与播放器

当前 `LivePlayerService` 使用 loopback `HttpListener`：

- `/player`：播放器页面。
- `/mpegts.js`：内嵌本地脚本。
- `/stream/{token}`：代理真实 FLV。

会话 token 映射 `StreamUrl`、RoomId、标准 Cookie 与独立 CancellationTokenSource。代理上游设置 User-Agent、Accept、Origin、房间 Referer 和可用 Cookie；UI 只接触 loopback URL，不接触 Cookie。

生命周期：

1. 创建新会话后再撤销旧 token，避免切流窗口完全空白。
2. Stop/追帧/切房/删房/关闭 App 调 `Revoke`。
3. Revoke 取消正在复制的上游流并 Dispose token source。
4. 服务 Dispose 关闭 listener、HttpClient 与所有会话。

播放器后端曾多次演进。修改前必须读取当前 `ILivePlayerService`、`LivePlayerService`、项目资源和工作页 code-behind；不要假定永远是 WebView/mpegts 或 LibVLC。

## WebView 播放风险

若当前仍使用 NativeWebView/mpegts：

- NativeWebView 可能压住 Avalonia overlay；owned window 可绕开视觉层问题。
- worker 中流 URL 使用绝对 loopback URL。
- autoplay 有声播放会被策略阻止；先静音并由文档内用户手势解锁。
- 打开/关闭设置或重建视图时，先显式 destroy player、pause、清 src，再移除 WebView，避免旧音频重叠。
- waiting/stalled 不应立刻无限重建；用 watchdog、恢复闸门和最大重试次数。
- 只看 currentTime 会把“音频推进、视频无帧”误判正常；同时检查 videoWidth/Height 与 mpegts statistics。

## 转发

每条启用规则独立监听源房间 socket：

1. 从内部 `SourceWorkspaceId/SourceRoomStateId` 定位公开 room ID。
2. 源账号按源工作区/房间层级解析。
3. 规则校验 SenderUid 与 ContentPattern。
4. 目标发送账号优先规则 AccountOverrideId，否则目标房间账号。
5. 匹配后由 `DanmuForwardService` 生成符号化文本，统一交 `IDanmuSendService`。
6. 禁用/删除/修改规则先取消旧 socket；默认新导入规则不启用。

刷新来源选项时，如果 key 序列未变化，不 Clear ObservableCollection；否则 ComboBox 会短暂写入 null 并清空持久化来源。

## 测试建议

- `BilibiliLiveWebSocketTests`：URI、command、DANMU/SC fixture、压缩/拆包边界。
- `BilibiliLivePlayInfoSelectorTests`：未开播、轮播、空 playurl、FLV/AVC 优先、清晰度交集。
- `FakeLiveDanmuSocketFactory`：主动推送消息，验证监听状态和转发发送账号。
- `FakeBilibiliLiveStreamService`/`FakeLivePlayerService`：验证追帧撤销旧 token、清晰度传递和停止。
- `LivePlayerServiceTests`：token 撤销、loopback 页面/资源、代理头和播放器脚本锚点；不拉真实 CDN。
