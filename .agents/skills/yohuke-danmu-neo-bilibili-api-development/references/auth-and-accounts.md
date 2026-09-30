# 登录、Cookie 与账号选择

## 浏览器登录抓取

入口：

- `Views/BrowserCookieLoginWindow.axaml(.cs)`：创建 `NativeWebView`、设置平台环境、导航登录页、读取 CookieManager。
- `Services/BrowserCookieLoginService.cs`：提供平台名称、登录 URI、隔离的 WebView profile 目录和 Cookie 结果转换。
- `Apis/ApiCookieHelper.cs`：解析、规范化和写入 Cookie。
- `Models/BrowserLogin/*`：登录平台与结果。

流程：

1. B 站登录 URI 为 `https://passport.bilibili.com/login`。
2. 为 B 站与 QQ 音乐使用独立持久化 profile，避免 Cookie 串平台；不要每次随机 profile，否则用户需要重复登录。
3. WebView 打开后由用户完成登录；点击“完成并抓取 Cookie”时调用 `TryGetCookieManager()` 与 `GetCookiesAsync()`。
4. `BrowserCookieLoginService.CreateResult` 将 Cookie 名称按大小写不敏感字典处理。
5. B 站只保留当前 API 需要的 `buvid3`、`SESSDATA`、`bili_jct`、`DedeUserID/DedeUserId`，再经 `ApiCookieHelper.CreateBilibiliCookie` 规范化。
6. 没有得到有效 Cookie 时抛出可读错误，不创建“已登录但空 Cookie”的账号。

NativeWebView 是原生视图。登录窗口可以使用它，但必须处理：初始化失败、CookieManager 不可用、窗口关闭、profile 路径和日志脱敏。不要把直播播放器的层级方案机械套到独立登录窗口。

## 二维码登录路径

`BilibiliApi` 仍提供：

- `GetLoginUrlAsync`：调用 QR generate，返回 URL 与 `qrcode_key`。
- `GetLoginInfoAsync`：轮询 QR 状态。
- `GetLoginCookieAsync`：访问成功回跳 URL，允许重定向并从 CookieContainer 提取字段。

使用时：

1. 对二维码过期、未扫码、已扫码未确认和成功状态分别建模，不用单一 bool。
2. 轮询支持 CancellationToken，并设置合理间隔；窗口关闭立即取消。
3. 成功回跳 URL 可能携带敏感参数，不写日志。
4. 最终仍转成标准 B 站 Cookie 字符串后存入 `BilibiliAccount.Cookie`。

## Cookie 与 CSRF

`ApiCookieHelper`：

- `Parse`：去除空白并按分号拆分键值。
- `CreateBilibiliCookie`：输出固定字段集合。
- `AddCookies`：写入 CookieContainer。
- `GetValue`：读取 `bili_jct` 等值。

`BilibiliApi.UpdateCookie` 同时更新账号槽 Cookie 与 CSRF 槽。写 API 使用同一槽的 `GetCookie(number)`/`GetCsrf(number)`，不要把账号 A 的 Cookie 与账号 B 的 CSRF 混用。

## 持久化账号

`BilibiliAccount` 持久化：

- `Id`：内部稳定引用，不等于 B 站 UID。
- `Name`：用户可编辑标签。
- `Cookie`：标准化 Cookie。
- `IsGlobalDefault`：全局默认账号。

账号写入 `accounts.json`。UI 可以显示 `CookieStatus` 和尾段摘要，但不要展示完整 Cookie。删除账号时清理：

- 全局默认状态。
- `WorkspaceState.AccountOverrideId`。
- `LiveRoomTabState.AccountOverrideId`。
- `DanmuForwardRuleState.AccountOverrideId`。

## 账号解析层级

普通房间请求使用 `AccountSelectionService.Resolve`：

1. 房间账号覆盖。
2. 工作区账号覆盖。
3. 全局默认账号。

转发：

- 源 WebSocket 账号按源 workspace/room 解析。
- 目标发送账号优先规则 `AccountOverrideId`，为空才回退目标房间账号。
- 指定账号不存在与 Cookie 为空是两个不同状态，不应启动转发。

## 安全检查

- 不把真实 Cookie 固化到测试、示例、skill 或源码。
- 不在 StartupLog 记录 Cookie、CSRF、回跳 URL、二维码 key。
- 报错只说“缺少 Cookie/指定账号不存在/登录已过期”等，不附凭据。
- 导出全部配置会包含账号敏感数据时，UI 和文档必须明确风险。
