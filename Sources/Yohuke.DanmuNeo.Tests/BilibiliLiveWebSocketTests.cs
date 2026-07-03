using Newtonsoft.Json.Linq;
using Yohuke.DanmuNeo.Apis;
using Yohuke.DanmuNeo.Apis.Models.Bilibili;

namespace Yohuke.DanmuNeo.Tests;

public class BilibiliLiveWebSocketTests
{
    [Fact]
    public void ParseDanmuMessageReadsUserAndContent()
    {
        var json = JObject.Parse("""
        {
          "cmd": "DANMU_MSG",
          "info": [
            [],
            "你好",
            [42, "用户A"]
          ]
        }
        """);

        var message = BilibiliLiveWebSocket.ParseDanmuMessage("100", json);

        Assert.NotNull(message);
        Assert.Equal("100", message.RoomId);
        Assert.Equal(42, message.Uid);
        Assert.Equal("用户A", message.UserName);
        Assert.Equal("你好", message.Content);
    }

    [Fact]
    public void CreateWebSocketUriUsesWssPort()
    {
        var uri = BilibiliLiveWebSocket.CreateWebSocketUri(new BilibiliDanmuHost
        {
            Host = "zj-cn-live-comet.chat.bilibili.com",
            WssPort = 2245
        });

        Assert.Equal("wss://zj-cn-live-comet.chat.bilibili.com:2245/sub", uri.ToString());
    }

    [Theory]
    [InlineData("DANMU_MSG", "DANMU_MSG", true)]
    [InlineData("DANMU_MSG:4:0:2:2:2:0", "DANMU_MSG", true)]
    [InlineData("SUPER_CHAT_MESSAGE_JPN", "SUPER_CHAT_MESSAGE", true)]
    [InlineData("SEND_GIFT", "DANMU_MSG", false)]
    public void IsCommandMatchesBilibiliCommandPrefix(string command, string expectedCommand, bool expected)
    {
        Assert.Equal(expected, BilibiliLiveWebSocket.IsCommand(command, expectedCommand));
    }

    [Fact]
    public void ParseSuperChatMessageReadsPriceAndContent()
    {
        var json = JObject.Parse("""
        {
          "cmd": "SUPER_CHAT_MESSAGE",
          "data": {
            "message": "SC内容",
            "price": 30,
            "price_text": "￥30",
            "ts": 123456,
            "user_info": {
              "uname": "用户B"
            }
          }
        }
        """);

        var message = BilibiliLiveWebSocket.ParseSuperChatMessage("200", json);

        Assert.NotNull(message);
        Assert.Equal("200", message.RoomId);
        Assert.Equal("用户B", message.UserName);
        Assert.Equal(30, message.Price);
        Assert.Equal("￥30", message.PriceText);
        Assert.Equal("SC内容", message.Content);
    }
}
