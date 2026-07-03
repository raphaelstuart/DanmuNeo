namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 直播弹幕展示项。
/// </summary>
public class DanmuFeedItem
{
    /// <summary>
    /// 发送时间。
    /// </summary>
    public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 弹幕内容。
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 是否为当前应用发送记录。
    /// </summary>
    public bool IsLocalRecord { get; set; }

    /// <summary>
    /// 状态文本。
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// 展示用时间。
    /// </summary>
    public string DisplayTime => Time.ToLocalTime().ToString("HH:mm:ss");

    /// <summary>
    /// 展示用附加信息。
    /// </summary>
    public string DetailLine => string.IsNullOrWhiteSpace(Status)
        ? DisplayTime
        : $"{DisplayTime} · {Status}";
}
