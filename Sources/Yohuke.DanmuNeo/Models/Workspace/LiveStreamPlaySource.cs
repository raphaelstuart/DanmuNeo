namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 可播放直播流。
/// </summary>
public class LiveStreamPlaySource
{
    /// <summary>
    /// 直播间 ID。
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// 直播状态。
    /// </summary>
    public int LiveStatus { get; set; }

    /// <summary>
    /// 开播时间戳。
    /// </summary>
    public long LiveTime { get; set; }

    /// <summary>
    /// 流地址。
    /// </summary>
    public string StreamUrl { get; set; } = "";

    /// <summary>
    /// 当前清晰度。
    /// </summary>
    public int CurrentQuality { get; set; }

    /// <summary>
    /// 当前清晰度显示文本。
    /// </summary>
    public string CurrentQualityDescription { get; set; } = "";

    /// <summary>
    /// 可用清晰度。
    /// </summary>
    public List<LiveQualityOption> QualityOptions { get; set; } = [];
}
