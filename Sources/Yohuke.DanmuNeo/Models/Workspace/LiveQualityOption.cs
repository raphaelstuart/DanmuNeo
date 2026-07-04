namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 直播清晰度选项。
/// </summary>
public class LiveQualityOption
{
    /// <summary>
    /// 清晰度编号。
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// 显示文本。
    /// </summary>
    public string Description { get; set; } = "";
}
