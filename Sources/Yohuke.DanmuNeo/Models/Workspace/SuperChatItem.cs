using Avalonia.Media;

namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// Super Chat 展示项。
/// </summary>
public class SuperChatItem
{
    /// <summary>
    /// SC 消息 ID。
    /// </summary>
    public string MessageId { get; set; } = "";

    /// <summary>
    /// 发送时间。
    /// </summary>
    public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 金额文本。
    /// </summary>
    public string PriceText { get; set; } = "";

    /// <summary>
    /// SC 边框颜色。
    /// </summary>
    public Color BorderColor { get; set; } = Color.Parse("#2A60B2");

    /// <summary>
    /// 展示时间。
    /// </summary>
    public string DisplayTime => Time.ToLocalTime().ToString("HH:mm:ss");

    /// <summary>
    /// 详情行。
    /// </summary>
    public string DetailLine => string.IsNullOrWhiteSpace(PriceText) ? DisplayTime : $"{DisplayTime} · {PriceText}";

    /// <summary>
    /// SC 内容。
    /// </summary>
    public string Content { get; set; } = "";
}
