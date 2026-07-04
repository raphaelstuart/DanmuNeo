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

        Assert.Contains("<video id=\"player\" autoplay muted playsinline>", html);
        Assert.DoesNotContain("<video id=\"player\" controls", html);
        Assert.Contains("点击开启声音", html);
        Assert.Contains("root.addEventListener(\"pointerdown\", unlockSoundByUserGesture);", html);
        Assert.Contains("window.yohukeSetLivePlayerAudio", html);
        Assert.Contains("requestedMuted || !soundUnlocked", html);
        Assert.DoesNotContain("正在开启声音", html);
    }

    [Fact]
    public void PlayerHtmlDetectsMissingVideoFrames()
    {
        var method = typeof(LivePlayerService).GetMethod("CreatePlayerHtml", BindingFlags.NonPublic | BindingFlags.Static);
        var html = Assert.IsType<string>(method?.Invoke(null, null));

        Assert.Contains("video.videoWidth", html);
        Assert.Contains("video.videoHeight", html);
        Assert.Contains("lastVideoFrameAt", html);
        Assert.Contains("lastDecodedFrames", html);
        Assert.Contains("mpegts.Events.MEDIA_INFO", html);
        Assert.Contains("mpegts.Events.STATISTICS_INFO", html);
        Assert.Contains("直播画面未输出", html);
    }
}
