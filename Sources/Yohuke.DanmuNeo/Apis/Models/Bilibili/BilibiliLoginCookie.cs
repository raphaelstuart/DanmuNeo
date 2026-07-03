namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站登录 Cookie。
/// </summary>
public class BilibiliLoginCookie
{
    /// <summary>
    /// 浏览器设备标识。
    /// </summary>
    public string Buvid3 { get; set; } = "";

    /// <summary>
    /// 登录会话。
    /// </summary>
    public string SessData { get; set; } = "";

    /// <summary>
    /// CSRF 令牌。
    /// </summary>
    public string BiliJct { get; set; } = "";

    /// <summary>
    /// 用户 ID。
    /// </summary>
    public string DedeUserId { get; set; } = "";

    /// <summary>
    /// 拼装为 B 站直播接口使用的 Cookie 字符串。
    /// </summary>
    public string ToCookieString()
    {
        return $"buvid3={Buvid3};SESSDATA={SessData};bili_jct={BiliJct};DedeUserId={DedeUserId}";
    }
}
