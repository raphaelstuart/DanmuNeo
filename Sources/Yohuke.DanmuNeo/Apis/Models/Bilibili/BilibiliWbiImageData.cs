using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站 WBI 图片键数据。
/// </summary>
public class BilibiliWbiImageData
{
    /// <summary>
    /// WBI 图片信息。
    /// </summary>
    [JsonProperty("wbi_img")]
    public BilibiliWbiImage? WbiImage { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
