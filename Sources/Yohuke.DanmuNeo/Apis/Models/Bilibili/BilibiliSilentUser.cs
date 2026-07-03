using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播间禁言用户。
/// </summary>
public class BilibiliSilentUser
{
    /// <summary>
    /// 禁言记录 ID。
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// 用户 UID。
    /// </summary>
    [JsonProperty("tuid")]
    public long Uid { get; set; }

    /// <summary>
    /// 用户名。
    /// </summary>
    [JsonProperty("uname")]
    public string? Name { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
