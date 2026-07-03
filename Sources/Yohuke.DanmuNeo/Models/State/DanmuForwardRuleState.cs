namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 直播间弹幕转发规则。
/// </summary>
public class DanmuForwardRuleState
{
    /// <summary>
    /// 规则 ID。
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 源工作区 ID。
    /// </summary>
    public string SourceWorkspaceId { get; set; } = "";

    /// <summary>
    /// 源直播间状态 ID。
    /// </summary>
    public string SourceRoomStateId { get; set; } = "";

    /// <summary>
    /// 监听发送人 UID。
    /// </summary>
    public string SenderUid { get; set; } = "";

    /// <summary>
    /// 同传格式匹配正则。
    /// </summary>
    public string ContentPattern { get; set; } = "";
}
