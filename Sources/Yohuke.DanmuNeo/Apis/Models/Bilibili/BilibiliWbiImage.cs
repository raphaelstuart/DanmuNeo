using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站 WBI 图片 URL。
/// </summary>
public class BilibiliWbiImage
{
    /// <summary>
    /// 图片键 URL。
    /// </summary>
    [JsonProperty("img_url")]
    public string ImgUrl { get; set; } = "";

    /// <summary>
    /// 子键 URL。
    /// </summary>
    [JsonProperty("sub_url")]
    public string SubUrl { get; set; } = "";

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
