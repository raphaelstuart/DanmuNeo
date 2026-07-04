using Avalonia.Controls;
using Avalonia.Media;

namespace Yohuke.DanmuNeo.Views;

internal class AttachedDialogWindow : Window
{
    public AttachedDialogWindow()
    {
        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Background = Brushes.Transparent;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
    }
}
