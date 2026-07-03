namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 应用持久化状态。
/// </summary>
public class AppState
{
    /// <summary>
    /// 应用设置。
    /// </summary>
    public AppSettings Settings { get; set; } = new();

    /// <summary>
    /// B 站账号列表。
    /// </summary>
    public List<BilibiliAccount> Accounts { get; set; } = [];

    /// <summary>
    /// 本地歌词库。
    /// </summary>
    public List<LyricLibraryItem> LyricLibrary { get; set; } = [];

    /// <summary>
    /// 工作区列表。
    /// </summary>
    public List<WorkspaceState> Workspaces { get; set; } = [];

    /// <summary>
    /// 当前工作区 ID。
    /// </summary>
    public string? SelectedWorkspaceId { get; set; }
}
