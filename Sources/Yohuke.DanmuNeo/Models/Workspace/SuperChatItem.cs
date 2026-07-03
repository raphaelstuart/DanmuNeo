namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// Super Chat 展示项。
/// </summary>
public class SuperChatItem
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
    /// 金额文本。
    /// </summary>
    public string PriceText { get; set; } = "";

    /// <summary>
    /// SC 内容。
    /// </summary>
    public string Content { get; set; } = "";
}
