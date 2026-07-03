namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 直播间标签页状态。
/// </summary>
public class LiveRoomTabState
{
    /// <summary>
    /// 标签页 ID。
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 直播间显示名称。
    /// </summary>
    public string RoomName { get; set; } = "未命名直播间";

    /// <summary>
    /// 主播 UID。
    /// </summary>
    public string OwnerUid { get; set; } = "";

    /// <summary>
    /// 排序序号。
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 标签页账号覆盖 ID。
    /// </summary>
    public string? AccountOverrideId { get; set; }

    /// <summary>
    /// 主播头像缓存路径。
    /// </summary>
    public string AvatarPath { get; set; } = "";

    /// <summary>
    /// 同传输入草稿。
    /// </summary>
    public string InputDraft { get; set; } = "";

    /// <summary>
    /// 当前歌词标题。
    /// </summary>
    public string LyricTitle { get; set; } = "";

    /// <summary>
    /// 当前歌词文本。
    /// </summary>
    public string LyricText { get; set; } = "";

    /// <summary>
    /// 弹幕转发规则。
    /// </summary>
    public List<DanmuForwardRuleState> ForwardRules { get; set; } = [];
}