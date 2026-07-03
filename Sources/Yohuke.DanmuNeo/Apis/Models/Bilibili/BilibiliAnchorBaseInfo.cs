using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站主播基础资料。
/// </summary>
public class BilibiliAnchorBaseInfo
{
    /// <summary>
    /// 用户名。
    /// </summary>
    [JsonProperty("uname")]
    public string? Name { get; set; }

    /// <summary>
    /// 头像链接。
    /// </summary>
    [JsonProperty("face")]
    public string? Face { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
