using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class MarkSymbolServiceTests
{
    [Fact]
    public void NormalizeCreatesDefaultGroupFromLegacyMarks()
    {
        var settings = new AppSettings
        {
            TranslateOpenMark = "「",
            TranslateCloseMark = "」",
            LyricOpenMark = "♪",
            LyricCloseMark = "♪"
        };

        MarkSymbolService.Normalize(settings);

        Assert.Single(settings.MarkGroups);
        Assert.Equal("「", settings.MarkGroups[0].TranslateOpenMark);
        Assert.Equal("♪", settings.MarkGroups[0].LyricOpenMark);
    }

    [Fact]
    public void ResolveUsesWorkspaceSelectedGroup()
    {
        var settings = new AppSettings
        {
            MarkGroups =
            [
                new()
                {
                    Id = "default",
                    Name = "默认"
                },
                new()
                {
                    Id = "live",
                    Name = "节目",
                    TranslateOpenMark = "【TL:"
                }
            ]
        };
        var workspace = new WorkspaceState
        {
            SelectedMarkGroupId = "live"
        };
        var service = new MarkSymbolService();

        var group = service.Resolve(settings, workspace);

        Assert.Equal("节目", group.Name);
        Assert.Equal("【TL:", group.TranslateOpenMark);
    }
}
