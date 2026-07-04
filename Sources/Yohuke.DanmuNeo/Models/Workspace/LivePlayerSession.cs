namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 本地直播播放器会话。
/// </summary>
public class LivePlayerSession
{
    /// <summary>
    /// 会话令牌。
    /// </summary>
    public string Token { get; set; } = "";

    /// <summary>
    /// 播放器页面地址。
    /// </summary>
    public string PlayerUrl { get; set; } = "";
}
