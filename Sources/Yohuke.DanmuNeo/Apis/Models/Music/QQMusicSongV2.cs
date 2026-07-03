using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐新版歌曲。
/// </summary>
public class QQMusicSongV2
{
    /// <summary>
    /// 歌曲 ID。
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// 歌曲 MID。
    /// </summary>
    [JsonProperty("mid")]
    public string Mid { get; set; } = "";

    /// <summary>
    /// 歌曲名。
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 副标题。
    /// </summary>
    [JsonProperty("subtitle")]
    public string Subtitle { get; set; } = "";

    /// <summary>
    /// 歌手。
    /// </summary>
    [JsonProperty("singer")]
    public List<QQMusicSinger> Singer { get; set; } = [];

    /// <summary>
    /// 专辑。
    /// </summary>
    [JsonProperty("album")]
    public QQMusicAlbum? Album { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
