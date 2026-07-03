namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 可分享工作区包。
/// </summary>
public class WorkspaceSharePackage
{
    /// <summary>
    /// 分享包格式版本。
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// 工作区名称。
    /// </summary>
    public string WorkspaceName { get; set; } = "";

    /// <summary>
    /// 同传开标记。
    /// </summary>
    public string TranslateOpenMark { get; set; } = "【";

    /// <summary>
    /// 同传闭标记。
    /// </summary>
    public string TranslateCloseMark { get; set; } = "】";

    /// <summary>
    /// 歌词开标记。
    /// </summary>
    public string LyricOpenMark { get; set; } = "【♪";

    /// <summary>
    /// 歌词闭标记。
    /// </summary>
    public string LyricCloseMark { get; set; } = "】";

    /// <summary>
    /// 默认歌词来源。
    /// </summary>
    public string DefaultLyricSource { get; set; } = "wy";

    /// <summary>
    /// 直播间列表。
    /// </summary>
    public List<WorkspaceShareRoom> Rooms { get; set; } = [];
}
