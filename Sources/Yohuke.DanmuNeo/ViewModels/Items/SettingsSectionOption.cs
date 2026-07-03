using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.ViewModels.Items;

/// <summary>
/// 设置页分区选项。
/// </summary>
public partial class SettingsSectionOption : ObservableObject
{
    /// <summary>
    /// 初始化设置页分区选项。
    /// </summary>
    public SettingsSectionOption(string key, string name, string description)
    {
        Key = key;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// 分区键。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 描述。
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 是否为当前选择。
    /// </summary>
    [ObservableProperty]
    private bool isSelected;
}
