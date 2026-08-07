namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播普通弹幕。
/// </summary>
public class BilibiliDanmuMessage
{
    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 用户 UID。
    /// </summary>
    public long Uid { get; set; }

    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 弹幕内容。
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 是否为 B 站协议标记的单表情弹幕。
    /// </summary>
    public bool IsEmoticon { get; set; }
}
