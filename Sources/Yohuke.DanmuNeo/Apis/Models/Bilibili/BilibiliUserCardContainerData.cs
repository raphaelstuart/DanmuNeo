using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站用户名片容器。
/// </summary>
public class BilibiliUserCardContainerData
{
    /// <summary>
    /// 用户公开资料。
    /// </summary>
    [JsonProperty("card")]
    public BilibiliUserCardData? Card { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}