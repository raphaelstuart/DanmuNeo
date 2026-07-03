using Avalonia;
using Avalonia.Styling;
using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 应用配色方案。
/// </summary>
public static class AppThemeService
{
    /// <summary>
    /// 应用配色方案。
    /// </summary>
    public static void Apply(AppThemeMode themeMode)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = themeMode switch
        {
            AppThemeMode.Light => ThemeVariant.Light,
            AppThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}
