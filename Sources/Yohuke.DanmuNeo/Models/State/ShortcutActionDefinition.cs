namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 快捷键动作定义。
/// </summary>
public class ShortcutActionDefinition
{
    /// <summary>
    /// 初始化快捷键动作定义。
    /// </summary>
    public ShortcutActionDefinition(
        string actionKey,
        string groupKey,
        string groupName,
        string name,
        string defaultGestureText)
    {
        ActionKey = actionKey;
        GroupKey = groupKey;
        GroupName = groupName;
        Name = name;
        DefaultGestureText = defaultGestureText;
    }

    /// <summary>
    /// 动作键。
    /// </summary>
    public string ActionKey { get; }

    /// <summary>
    /// 分组键。
    /// </summary>
    public string GroupKey { get; }

    /// <summary>
    /// 分组名称。
    /// </summary>
    public string GroupName { get; }

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 默认快捷键文本。
    /// </summary>
    public string DefaultGestureText { get; }
}
