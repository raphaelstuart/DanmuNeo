namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 本地直播代理流条目。
/// </summary>
internal class LivePlayerStreamEntry
{
    /// <summary>
    /// 上游流地址。
    /// </summary>
    public string StreamUrl { get; set; } = "";

    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// Cookie。
    /// </summary>
    public string Cookie { get; set; } = "";

    /// <summary>
    /// 取消令牌。
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; } = new();
}
