using System.Net;
using Yohuke.DanmuNeo.Models.BrowserLogin;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class BrowserCookieLoginServiceTests
{
    [Fact]
    public void CreateResult_NormalizesBilibiliCookies()
    {
        var service = new BrowserCookieLoginService();
        var result = service.CreateResult(BrowserLoginPlatform.Bilibili,
        [
            new("buvid3", "b3"),
            new("SESSDATA", "sess"),
            new("bili_jct", "csrf"),
            new("DedeUserID", "42"),
            new("unused", "x")
        ]);

        Assert.Equal("buvid3=b3;SESSDATA=sess;bili_jct=csrf;DedeUserId=42", result.Cookie);
    }

    [Fact]
    public void CreateResult_ThrowsWhenBilibiliLoginCookiesMissing()
    {
        var service = new BrowserCookieLoginService();

        Assert.Throws<InvalidOperationException>(() => service.CreateResult(BrowserLoginPlatform.Bilibili, []));
    }

    [Fact]
    public void CreateResult_ExtractsQQMusicCookies()
    {
        var service = new BrowserCookieLoginService();
        var result = service.CreateResult(BrowserLoginPlatform.QQMusic,
        [
            new("uin", "123"),
            new("qm_keyst", "key"),
            new("unused", "x")
        ]);

        Assert.Equal("uin=123;qm_keyst=key", result.Cookie);
    }

    [Fact]
    public void CreateResult_UsesQQMusicKeyFallback()
    {
        var service = new BrowserCookieLoginService();
        var result = service.CreateResult(BrowserLoginPlatform.QQMusic,
        [
            new("uin", "123"),
            new("qqmusic_key", "fallback")
        ]);

        Assert.Equal("uin=123;qm_keyst=fallback", result.Cookie);
    }
}
