namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// 从 B 站直播弹幕中解析出的翻译弹幕。
/// </summary>
public class BilibiliTranslatedDanmuMessage
{
    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 说话人。
    /// </summary>
    public string Speaker { get; set; } = "";

    /// <summary>
    /// 翻译内容。
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 原始弹幕内容。
    /// </summary>
    public string RawContent { get; set; } = "";
}
