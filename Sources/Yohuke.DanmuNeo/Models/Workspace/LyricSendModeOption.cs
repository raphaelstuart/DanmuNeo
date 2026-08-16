using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Models.Workspace;

/// <summary>
/// 歌词发送内容模式选项。
/// </summary>
public class LyricSendModeOption
{
    /// <summary>
    /// 初始化歌词发送内容模式选项。
    /// </summary>
    public LyricSendModeOption(LyricSendMode value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    /// <summary>
    /// 模式值。
    /// </summary>
    public LyricSendMode Value { get; }

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string DisplayName { get; }
}
