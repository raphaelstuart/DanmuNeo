using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播间屏蔽词列表数据。
/// </summary>
public class BilibiliShieldKeywordListData
{
    /// <summary>
    /// 屏蔽词列表。
    /// </summary>
    [JsonProperty("list")]
    public List<BilibiliShieldKeyword> List { get; set; } = [];

    /// <summary>
    /// 总数。
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
