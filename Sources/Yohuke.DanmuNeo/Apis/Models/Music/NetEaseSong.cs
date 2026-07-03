using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// 网易云音乐歌曲。
/// </summary>
public class NetEaseSong
{
    /// <summary>
    /// 歌曲 ID。
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// 歌曲名。
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 别名。
    /// </summary>
    [JsonProperty("alias")]
    public List<string> Alias { get; set; } = [];

    /// <summary>
    /// 歌手列表。
    /// </summary>
    [JsonProperty("artists")]
    public List<NetEaseArtist> Artists { get; set; } = [];

    /// <summary>
    /// 专辑。
    /// </summary>
    [JsonProperty("album")]
    public NetEaseAlbum? Album { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
