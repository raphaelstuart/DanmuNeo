---
name: yohuke-danmu-neo-bilibili-api-development
description: Use when modifying, reviewing, explaining, testing, or planning Bilibili integration in Yohuke.DanmuNeo, including embedded-browser or QR login, Cookie and CSRF extraction, account selection, WBI signing, room and user APIs, avatar lookup, danmu sending and moderation, live play information and stream selection, live WebSocket packet handling, reconnection, forwarding, and stream proxy headers.
---

# Yohuke Danmu Neo · B站 API 研发

## 核心流程

1. 先确认当前仓库与消费者。读取 `BilibiliApi`、相关 DTO、Service、Room/Main VM 和对应测试；以当前源码为准，B 站非公开接口与历史实现都可能变化。
2. 按任务读取参考：登录与账号使用 `references/auth-and-accounts.md`；HTTP/WBI/头像/发送使用 `references/http-api.md`；直播 WebSocket、播放信息、代理与转发使用 `references/live-and-streaming.md`。
3. 保持分层：外部 JSON/包格式 → `Apis/Models/Bilibili` DTO → `BilibiliApi`/`BilibiliLiveWebSocket` → Service/Selector → ViewModel → UI。不要在 AXAML 或 code-behind 直接拼 B 站请求。
4. 对 HTTP 同时检查传输状态和 `ApiResponse.Code`。保留 `Message`/`Msg` 作为用户可见错误，网络异常、取消和业务错误分开处理。
5. 为请求带正确 Cookie、CSRF、Origin、Referer、User-Agent、超时和 CancellationToken。只有端点要求时才加 Cookie；不要扩大凭据传播范围。
6. 用接口、factory、纯 selector 和静态 parser 编写离线测试。不要让自动测试访问真实 B 站、发送真实弹幕或读取用户本机账号。

## 认证事实

- 本项目的“B 站 Token”主要是登录 Cookie 组合，不是一个独立 OAuth token：`buvid3`、`SESSDATA`、`bili_jct`、`DedeUserID/DedeUserId`。
- `bili_jct` 是写操作使用的 CSRF 值；Cookie 由 `ApiCookieHelper.CreateBilibiliCookie` 规范化。
- 浏览器登录、二维码登录是两条 Cookie 获取路径。修改前确认 UI 实际使用哪条，不要删除看似未被当前页面调用但仍有测试/兼容用途的路径。
- Cookie、CSRF、二维码回跳 URL 和完整 Set-Cookie 都是敏感数据。不得写入 `StartupLog`、异常文本、测试快照或最终答复；账号 UI 只显示受控摘要。

## API 设计约束

- 外部 DTO 一个文件一个顶层类型；用 `JsonProperty`/当前序列化约定映射不规则字段，不把 API DTO 当持久化 State。
- 新增 B 站 API 方法时沿用 `BaseApi` 的 GET/form POST/JSON POST、统一 User-Agent、超时和 CancellationToken。
- 需要 WBI 的接口调用 `BilibiliWbiSigner.FillAsync`，不要复制签名算法。签名 key 缓存、刷新锁与参数排序必须保持一致。
- 区分直播间 ID、房间状态 ID、主播 UID 和账号 ID。头像按 UID 查询，直播信息/播放/发送按 room ID；转发来源使用内部 workspace/room-state ID 定位，再读取公开 room ID。
- 账号选择遵循直播间覆盖 → 工作区覆盖 → 全局默认；转发目标还允许规则级账号覆盖，源监听账号仍按源房间解析。

## 运行时与故障处理

- WebSocket、转发、播放器代理都必须持有可取消生命周期。停止、切房、删房、删工作区、追帧或关闭 App 时撤销旧资源。
- 重连应区分首次连接、中断、恢复和用户取消；用户取消不报告为故障。
- B 站接口可能限频、风控、字段缺失或改变 host。提供可解释状态和回退，但不要静默吞掉所有错误后只返回空 UI。
- 直播流 URL 有时效性。追帧/刷新应重新调用播放信息接口并撤销旧 token，不长期缓存真实 CDN URL。
- 直播代理只监听 loopback，token 仅进程内有效；向上游补 Referer/Origin/Cookie，不向本地页面暴露真实 Cookie。

## 验证

- 登录/Cookie：`BrowserCookieLoginServiceTests`、`ApiCookieHelper` 相关覆盖。
- 账号层级：`AccountSelectionServiceTests`、`MainWindowViewModelTests`。
- 用户/头像：`AvatarCacheServiceTests`，使用 fake HttpClient 响应。
- WebSocket 包：`BilibiliLiveWebSocketTests`，覆盖 packet/parser，不连真实 socket。
- 播放信息：`BilibiliLivePlayInfoSelectorTests`，覆盖未开播、无地址、协议/格式/codec 优先级与清晰度。
- Room 行为：`LiveRoomTabViewModelTests` + `FakeLiveDanmuSocketFactory`/`FakeDanmuSendService`/`FakeBilibiliLiveStreamService`。
- 播放代理：`LivePlayerServiceTests`，避免依赖真实 CDN。
- 串行执行 `dotnet build Sources/Yohuke.DanmuNeo.sln --no-restore` 与 `dotnet test Sources/Yohuke.DanmuNeo.sln --no-restore`。

## 按需参考

- 登录窗口、Cookie 抓取、QR 回跳、账号持久化与选择层级：`references/auth-and-accounts.md`。
- HTTP 客户端、端点地图、WBI、头像、发送、禁言与屏蔽词：`references/http-api.md`。
- 弹幕 WebSocket 包、重连、播放信息选择、本地代理与转发：`references/live-and-streaming.md`。
