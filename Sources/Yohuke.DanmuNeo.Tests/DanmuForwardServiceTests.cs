using Yohuke.DanmuNeo.Apis.Models.Bilibili;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class DanmuForwardServiceTests
{
    [Fact]
    public void IsMatchAllowsEmptyUidAndPattern()
    {
        var service = new DanmuForwardService();
        var message = CreateMessage(42, "【翻译】");

        Assert.True(service.IsMatch(message, new()));
    }

    [Fact]
    public void IsMatchCanFilterByUidOnly()
    {
        var service = new DanmuForwardService();
        var rule = new DanmuForwardRuleState
        {
            SenderUid = "42"
        };

        Assert.True(service.IsMatch(CreateMessage(42, "任意内容"), rule));
        Assert.False(service.IsMatch(CreateMessage(7, "任意内容"), rule));
    }

    [Fact]
    public void IsMatchCanFilterByPatternOnly()
    {
        var service = new DanmuForwardService();
        var rule = new DanmuForwardRuleState
        {
            ContentPattern = "^【.+】$"
        };

        Assert.True(service.IsMatch(CreateMessage(42, "【翻译】"), rule));
        Assert.False(service.IsMatch(CreateMessage(42, "普通弹幕"), rule));
    }

    [Fact]
    public void IsMatchRequiresUidAndPatternWhenBothConfigured()
    {
        var service = new DanmuForwardService();
        var rule = new DanmuForwardRuleState
        {
            SenderUid = "42",
            ContentPattern = "^【.+】$"
        };

        Assert.True(service.IsMatch(CreateMessage(42, "【翻译】"), rule));
        Assert.False(service.IsMatch(CreateMessage(7, "【翻译】"), rule));
        Assert.False(service.IsMatch(CreateMessage(42, "普通弹幕"), rule));
    }

    [Fact]
    public void ValidateReturnsMessageForInvalidPattern()
    {
        var service = new DanmuForwardService();
        var rule = new DanmuForwardRuleState
        {
            ContentPattern = "["
        };

        Assert.NotEmpty(service.Validate(rule));
    }

    private static BilibiliDanmuMessage CreateMessage(long uid, string content)
    {
        return new()
        {
            Uid = uid,
            Content = content,
            UserName = "用户"
        };
    }
}
