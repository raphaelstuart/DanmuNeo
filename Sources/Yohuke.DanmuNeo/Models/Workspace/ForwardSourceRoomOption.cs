namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 可作为转发源的直播间选项。
/// </summary>
public class ForwardSourceRoomOption
{
    /// <summary>
    /// 选项键。
    /// </summary>
    public string Key => $"{WorkspaceId}|{RoomStateId}";

    /// <summary>
    /// 工作区 ID。
    /// </summary>
    public string WorkspaceId { get; set; } = "";

    /// <summary>
    /// 工作区名称。
    /// </summary>
    public string WorkspaceName { get; set; } = "";

    /// <summary>
    /// 直播间状态 ID。
    /// </summary>
    public string RoomStateId { get; set; } = "";

    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 直播间名称。
    /// </summary>
    public string RoomName { get; set; } = "";

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string DisplayName => $"{WorkspaceName} / {(string.IsNullOrWhiteSpace(RoomName) ? RoomId : RoomName)} · {RoomId}";
}
