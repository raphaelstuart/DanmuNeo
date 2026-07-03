using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播间屏蔽词。
/// </summary>
public class BilibiliShieldKeyword
{
    /// <summary>
    /// 屏蔽词。
    /// </summary>
    [JsonProperty("keyword")]
    public string Keyword { get; set; } = "";

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
