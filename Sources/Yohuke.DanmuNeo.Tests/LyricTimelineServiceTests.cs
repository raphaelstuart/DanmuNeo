using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class LyricTimelineServiceTests
{
    [Fact]
    public void ParseReadsTimelineLines()
    {
        var service = new LyricTimelineService();

        var lines = service.Parse("[00:01.50]第一句\n[00:03.00]第二句");

        Assert.Equal(2, lines.Count);
        Assert.Equal(1.5, lines[0].TimeSeconds);
        Assert.Equal("第一句", lines[0].Content);
        Assert.True(LyricTimelineService.HasTimeline(lines));
    }

    [Fact]
    public void ParseKeepsPlainLyricsWithoutTimeline()
    {
        var service = new LyricTimelineService();

        var lines = service.Parse("第一句\n第二句");

        Assert.Equal(2, lines.Count);
        Assert.All(lines, line => Assert.Equal(-1, line.TimeSeconds));
        Assert.False(LyricTimelineService.HasTimeline(lines));
    }

    [Fact]
    public void CreateMessageUsesConfiguredMarks()
    {
        var message = LyricTimelineService.CreateMessage("【♪", "】", "歌词");

        Assert.Equal("【♪歌词】", message);
    }
}
