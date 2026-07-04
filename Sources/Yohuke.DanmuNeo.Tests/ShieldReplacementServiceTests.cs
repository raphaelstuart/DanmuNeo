using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class ShieldReplacementServiceTests
{
    [Fact]
    public void ApplyReplacesTextBySortOrder()
    {
        var settings = new AppSettings
        {
            ShieldReplacementRules =
            [
                new()
                {
                    SourceText = "猫",
                    ReplacementText = "狗",
                    SortOrder = 1
                },
                new()
                {
                    SourceText = "狗",
                    ReplacementText = "兔",
                    SortOrder = 2
                }
            ]
        };

        var result = ShieldReplacementService.Apply("猫猫", settings, false);

        Assert.Equal("兔兔", result);
    }

    [Fact]
    public void ApplyIgnoresEmptySourceText()
    {
        var settings = new AppSettings
        {
            ShieldReplacementRules =
            [
                new()
                {
                    SourceText = "",
                    ReplacementText = "空"
                },
                new()
                {
                    SourceText = "猫",
                    ReplacementText = "狗"
                }
            ]
        };

        var result = ShieldReplacementService.Apply("猫", settings, false);

        Assert.Equal("狗", result);
    }

    [Fact]
    public void ApplySkipsLyricsWhenLyricOptionDisabled()
    {
        var settings = new AppSettings
        {
            ApplyShieldReplacementToLyrics = false,
            ShieldReplacementRules =
            [
                new()
                {
                    SourceText = "猫",
                    ReplacementText = "狗"
                }
            ]
        };

        var result = ShieldReplacementService.Apply("猫", settings, true);

        Assert.Equal("猫", result);
    }

    [Fact]
    public void ApplySkipsAllWhenShieldProcessingDisabled()
    {
        var settings = new AppSettings
        {
            EnableShieldProcessing = false,
            ShieldReplacementRules =
            [
                new()
                {
                    SourceText = "猫",
                    ReplacementText = "狗"
                }
            ]
        };

        var result = ShieldReplacementService.Apply("猫", settings, false);

        Assert.Equal("猫", result);
    }
}
