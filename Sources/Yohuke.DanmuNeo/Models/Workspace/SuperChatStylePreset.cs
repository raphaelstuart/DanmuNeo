using Avalonia.Media;

namespace Yohuke.DanmuNeo.Models.Workspace;

internal sealed class SuperChatStylePreset
{
    public decimal CnyPrice { get; init; }

    public int BatteryAmount { get; init; }

    public string ColorName { get; init; } = "";

    public Color BorderColor { get; init; }

    public string PriceText => $"{BatteryAmount} 电池";

    public string DisplayName => $"{PriceText} · ￥{CnyPrice:0.##} · {ColorName}";
}
