using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐专辑。
/// </summary>
public class QQMusicAlbum
{
    /// <summary>
    /// 专辑 ID。
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// 专辑 MID。
    /// </summary>
    [JsonProperty("mid")]
    public string Mid { get; set; } = "";

    /// <summary>
    /// 专辑名。
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
