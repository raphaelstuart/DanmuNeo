# B站 HTTP API 与 WBI

## 基础封装

`BaseApi` 提供：

- GET JSON/文本/完整响应。
- form POST、JSON POST、原始 JSON 文本 POST。
- 默认浏览器 User-Agent。
- 查询参数 URL 编码、InvariantCulture 格式化。
- per-request 超时与 CancellationToken。
- HTTP `EnsureSuccessStatusCode`。

HTTP 2xx 不代表业务成功。调用方必须检查 `ApiResponse<T>.Code`，错误消息优先使用 `Message`，再用 `Msg`，最后给稳定的本地说明。

## 当前端点地图

| 能力 | 方法 | 端点/要点 |
| --- | --- | --- |
| 房间资料 | `GetRoomInfoAsync` | `xlive/web-room/v1/index/getInfoByRoom`, `room_id` |
| 播放信息 | `GetRoomPlayInfoAsync` | `xlive/web-room/v2/index/getRoomPlayInfo`; room、qn、protocol/format/codec、platform、ptype、dolby |
| 弹幕配置 | `GetDanmuConfigAsync` | `dM/GetDMConfigByGroup`; 需要账号头 |
| 房间内用户信息 | `GetUserInfoAsync` | `index/getInfoByUser`; 字数限制、颜色/模式等 |
| 用户名片 | `GetUserCardContainerAsync` | `x/web-interface/card?mid=UID`，带 space Referer |
| 用户 WBI 资料 | `GetUserCardAsync` | `x/space/wbi/acc/info`，使用 signer |
| 设置弹幕配置 | `SetDanmuConfigAsync` | form POST，Cookie + csrf/csrf_token |
| 发送弹幕 | `SendDanmuAsync` | `https://api.live.bilibili.com/msg/send`，roomid/msg/mode/rnd/csrf |
| 禁言列表/增删 | `GetSilentUserListAsync` 等 | web-ucenter/banned 与 silent service |
| 屏蔽词列表/增删 | `GetShieldKeywordListAsync` 等 | web-ucenter/banned shield endpoints |
| 搜索主播 | `SearchLiveUsersAsync` | `x/web-interface/search/type`, `search_type=live_user` |
| QR 登录 | `GetLoginUrlAsync`/`GetLoginInfoAsync` | passport QR generate/poll |

调用前以当前 `BilibiliApi.cs` 为准；B 站端点、参数、业务 code 可能变更。

## Headers

- `CreateDefaultHeaders`：User-Agent。
- `CreateLiveHeaders`：增加 `Origin: https://live.bilibili.com` 与 live Referer。
- `CreateAccountHeaders`：在 live headers 上增加指定槽 Cookie。
- 用户空间资料：使用 `https://space.bilibili.com/{uid}` Referer。
- 播放信息：Referer 应包含目标 room ID；可用账号时加标准化 Cookie。
- 搜索：当前使用 search Origin 与显式 Cookie。

不要给所有请求无差别添加 Cookie。公共读取接口优先匿名，只有风控、清晰度或账号能力需要时携带。

## WBI 签名

`BilibiliWbiSigner`：

1. 从导航接口读取 img/sub key。
2. 缓存 key，并用 SemaphoreSlim 防止并发重复刷新。
3. 添加 UTC Unix `wts`。
4. 按参数名 Ordinal 排序，值执行 WBI 约定过滤/编码。
5. 通过 mixin table 生成 32 字符 mixin key。
6. MD5 计算小写 `w_rid`。

新增 WBI API 时调用 `FillAsync`，不要复制 table 或自行缓存 key。测试签名纯函数时用固定 img/sub key 和参数，避免依赖在线导航接口。

## 房间 ID 与主播 UID

- room ID：房间资料、播放信息、发送弹幕、监听入口。
- owner UID：用户资料与头像缓存身份。
- internal room state ID：工作区配置/转发引用，不可传给 B 站 API。
- account ID：本地账号引用，不等于 `DedeUserID`。

`AvatarCacheService` 当前回退链：

1. Owner UID 可解析时，先 `x/web-interface/card`。
2. 失败后尝试 WBI space info。
3. 仍失败时用 room info 获取 anchor face/uid/name。
4. 头像缓存 key 优先 Owner UID，URL hash 用于版本变化；下载只记录本地路径，不保存响应 Cookie。

修改房间 ID/Owner UID 后应强制失效旧头像并重拉；失败不能把旧 UID 错配到新头像。

## 发送弹幕

`DanmuSendService` 负责：

- 验证账号和 Cookie。
- 全局 SemaphoreSlim 串行发送。
- 遵守 `SendIntervalMs`。
- 用设置的 Timeout 调 `BilibiliApi.SendDanmuAsync`。
- 检查业务 code，产生本地发送历史。
- 按房间最大长度切分内容。

同传/歌词/转发应统一经过发送服务，避免各自绕过限速、历史和错误处理。发送前的符号组与屏蔽词替换属于上层业务，不写入 API client。

## 新增 API 检查表

1. 新增独立 DTO 文件与 `ApiResponse<T>` 映射。
2. 确认 GET/form/JSON 方法、参数名和 Headers。
3. 明确是否需要 Cookie、CSRF、WBI 和 Referer。
4. 传入 timeout 与 CancellationToken。
5. Service 层检查业务 code 并转成领域错误。
6. 用注入 HttpClient/fake handler 测参数、响应与失败，不访问真实账号。
