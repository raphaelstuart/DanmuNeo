namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 工作区状态。
/// </summary>
public class WorkspaceState
{
    /// <summary>
    /// 工作区 ID。
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 工作区名称。
    /// </summary>
    public string Name { get; set; } = "默认工作区";

    /// <summary>
    /// 排序序号。
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// B 站账号覆盖 ID。
    /// </summary>
    public string? AccountOverrideId { get; set; }

    /// <summary>
    /// 当前工作区使用的符号组 ID。
    /// </summary>
    public string? SelectedMarkGroupId { get; set; }

    /// <summary>
    /// 直播间标签页。
    /// </summary>
    public List<LiveRoomTabState> LiveRooms { get; set; } = [];

    /// <summary>
    /// 当前直播间标签页 ID。
    /// </summary>
    public string? SelectedLiveRoomId { get; set; }
}
