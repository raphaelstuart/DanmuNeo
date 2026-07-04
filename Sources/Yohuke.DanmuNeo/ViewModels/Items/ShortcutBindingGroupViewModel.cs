namespace Yohuke.DanmuNeo.ViewModels.Items;

/// <summary>
/// 快捷键绑定分组视图模型。
/// </summary>
public class ShortcutBindingGroupViewModel
{
    /// <summary>
    /// 初始化快捷键绑定分组视图模型。
    /// </summary>
    public ShortcutBindingGroupViewModel(string name, IReadOnlyList<ShortcutBindingViewModel> bindings)
    {
        Name = name;
        Bindings = bindings;
    }

    /// <summary>
    /// 分组名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 快捷键绑定。
    /// </summary>
    public IReadOnlyList<ShortcutBindingViewModel> Bindings { get; }
}
