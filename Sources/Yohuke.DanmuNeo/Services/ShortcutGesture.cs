using Avalonia.Input;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 快捷键手势。
/// </summary>
public class ShortcutGesture
{
    /// <summary>
    /// 初始化快捷键手势。
    /// </summary>
    public ShortcutGesture(Key key, KeyModifiers modifiers)
    {
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    /// 主按键。
    /// </summary>
    public Key Key { get; }

    /// <summary>
    /// 修饰键。
    /// </summary>
    public KeyModifiers Modifiers { get; }
}
