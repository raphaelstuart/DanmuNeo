using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站主播信息。
/// </summary>
public class BilibiliAnchorInfo
{
    /// <summary>
    /// 主播基础资料。
    /// </summary>
    [JsonProperty("base_info")]
    public BilibiliAnchorBaseInfo? BaseInfo { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
