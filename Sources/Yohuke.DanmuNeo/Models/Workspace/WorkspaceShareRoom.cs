using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 分享工作区中的直播间。
/// </summary>
public class WorkspaceShareRoom
{
    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 直播间名称。
    /// </summary>
    public string RoomName { get; set; } = "";

    /// <summary>
    /// 主播 UID。
    /// </summary>
    public string OwnerUid { get; set; } = "";

    /// <summary>
    /// 歌词标题。
    /// </summary>
    public string LyricTitle { get; set; } = "";

    /// <summary>
    /// 歌词文本。
    /// </summary>
    public string LyricText { get; set; } = "";

    /// <summary>
    /// 翻译歌词文本。
    /// </summary>
    public string TranslatedLyricText { get; set; } = "";

    /// <summary>
    /// 歌词发送内容模式。
    /// </summary>
    public LyricSendMode LyricSendMode { get; set; } = LyricSendMode.Bilingual;
}
