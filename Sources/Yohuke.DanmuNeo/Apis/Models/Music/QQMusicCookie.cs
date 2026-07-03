namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐 Cookie。
/// </summary>
public class QQMusicCookie
{
    /// <summary>
    /// QQ 号 Cookie。
    /// </summary>
    public string Uin { get; set; } = "";

    /// <summary>
    /// QQ 音乐登录票据。
    /// </summary>
    public string QmKeyst { get; set; } = "";

    /// <summary>
    /// 拼装为 QQ 音乐接口使用的 Cookie 字符串。
    /// </summary>
    public string ToCookieString()
    {
        return $"uin={Uin};qm_keyst={QmKeyst}";
    }
}
