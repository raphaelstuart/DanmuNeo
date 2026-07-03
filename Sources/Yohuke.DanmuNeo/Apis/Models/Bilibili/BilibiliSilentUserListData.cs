using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播间禁言用户列表数据。
/// </summary>
public class BilibiliSilentUserListData
{
    /// <summary>
    /// 禁言用户列表。
    /// </summary>
    [JsonProperty("list")]
    public List<BilibiliSilentUser> List { get; set; } = [];

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
