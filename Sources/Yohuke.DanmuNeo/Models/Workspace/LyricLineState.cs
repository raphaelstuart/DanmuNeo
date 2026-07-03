namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 歌词时间轴行。
/// </summary>
public class LyricLineState
{
    /// <summary>
    /// 时间轴秒数。
    /// </summary>
    public double TimeSeconds { get; set; }

    /// <summary>
    /// 时间轴文本。
    /// </summary>
    public string Timeline { get; set; } = "";

    /// <summary>
    /// 歌词内容。
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 是否为当前播放行。
    /// </summary>
    public bool IsActive { get; set; }
}
