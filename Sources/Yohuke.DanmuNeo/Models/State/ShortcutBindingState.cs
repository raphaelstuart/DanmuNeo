using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 快捷键绑定状态。
/// </summary>
public partial class ShortcutBindingState : ObservableObject
{
    private string actionKey = "";
    private string gestureText = "";
    private bool isEnabled = true;

    /// <summary>
    /// 快捷键动作键。
    /// </summary>
    public string ActionKey
    {
        get => actionKey;
        set => SetProperty(ref actionKey, value);
    }

    /// <summary>
    /// 快捷键文本。
    /// </summary>
    public string GestureText
    {
        get => gestureText;
        set => SetProperty(ref gestureText, value);
    }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }
}
