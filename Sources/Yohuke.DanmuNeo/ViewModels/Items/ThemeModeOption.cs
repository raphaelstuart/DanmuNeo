using Yohuke.DanmuNeo.Models.State;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Yohuke.DanmuNeo.ViewModels.Items;

/// <summary>
/// 配色方案选项。
/// </summary>
public partial class ThemeModeOption : ObservableObject
{
    /// <summary>
    /// 初始化配色方案选项。
    /// </summary>
    public ThemeModeOption(AppThemeMode value, string name)
    {
        Value = value;
        Name = name;
    }

    /// <summary>
    /// 配色方案。
    /// </summary>
    public AppThemeMode Value { get; }

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 是否为系统配色预览。
    /// </summary>
    public bool IsSystemPreview => Value == AppThemeMode.System;

    /// <summary>
    /// 是否为浅色配色预览。
    /// </summary>
    public bool IsLightPreview => Value == AppThemeMode.Light;

    /// <summary>
    /// 是否为深色配色预览。
    /// </summary>
    public bool IsDarkPreview => Value == AppThemeMode.Dark;

    /// <summary>
    /// 是否为当前选择。
    /// </summary>
    [ObservableProperty]
    private bool isSelected;
}
