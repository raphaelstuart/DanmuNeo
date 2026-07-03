using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播弹幕服务器信息。
/// </summary>
public class BilibiliDanmuInfoData
{
    /// <summary>
    /// WebSocket 鉴权令牌。
    /// </summary>
    [JsonProperty("token")]
    public string Token { get; set; } = "";

    /// <summary>
    /// 弹幕服务器列表。
    /// </summary>
    [JsonProperty("host_list")]
    public List<BilibiliDanmuHost> HostList { get; set; } = [];

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
