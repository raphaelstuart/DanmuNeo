namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 工作区持久化状态。
/// </summary>
public class WorkspaceStorageState
{
    /// <summary>
    /// 工作区列表。
    /// </summary>
    public List<WorkspaceState> Workspaces { get; set; } = [];

    /// <summary>
    /// 当前工作区 ID。
    /// </summary>
    public string? SelectedWorkspaceId { get; set; }
}
