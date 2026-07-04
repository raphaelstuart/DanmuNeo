using System.Reflection;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class LivePlayerServiceTests
{
    [Fact]
    public void PlayerHtmlHidesNativeControlsByDefault()
    {
        var method = typeof(LivePlayerService).GetMethod("CreatePlayerHtml", BindingFlags.NonPublic | BindingFlags.Static);
        var html = Assert.IsType<string>(method?.Invoke(null, null));

        Assert.Contains("<video id=\"player\" autoplay playsinline>", html);
        Assert.DoesNotContain("<video id=\"player\" controls", html);
        Assert.Contains("video.addEventListener(\"click\", playVideoByUserGesture);", html);
    }
}
