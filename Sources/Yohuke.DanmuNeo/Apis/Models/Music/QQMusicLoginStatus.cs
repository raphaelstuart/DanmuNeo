namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐扫码登录状态。
/// </summary>
public class QQMusicLoginStatus
{
    /// <summary>
    /// 原始 ptuiCB 文本。
    /// </summary>
    public string RawText { get; set; } = "";

    /// <summary>
    /// 状态码。
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// 子状态码。
    /// </summary>
    public string SubCode { get; set; } = "";

    /// <summary>
    /// 确认登录后的跳转链接。
    /// </summary>
    public string RedirectUrl { get; set; } = "";

    /// <summary>
    /// 状态消息。
    /// </summary>
    public string Message { get; set; } = "";
}
