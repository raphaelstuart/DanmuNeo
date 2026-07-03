using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播弹幕服务器地址。
/// </summary>
public class BilibiliDanmuHost
{
    /// <summary>
    /// 主机名。
    /// </summary>
    [JsonProperty("host")]
    public string Host { get; set; } = "";

    /// <summary>
    /// WSS 端口。
    /// </summary>
    [JsonProperty("wss_port")]
    public int WssPort { get; set; }

    /// <summary>
    /// WS 端口。
    /// </summary>
    [JsonProperty("ws_port")]
    public int WsPort { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
