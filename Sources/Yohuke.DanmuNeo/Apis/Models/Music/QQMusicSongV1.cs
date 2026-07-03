using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐旧版歌曲。
/// </summary>
public class QQMusicSongV1
{
    /// <summary>
    /// 歌曲 ID。
    /// </summary>
    [JsonProperty("songid")]
    public long SongId { get; set; }

    /// <summary>
    /// 歌曲 MID。
    /// </summary>
    [JsonProperty("songmid")]
    public string SongMid { get; set; } = "";

    /// <summary>
    /// 歌曲名。
    /// </summary>
    [JsonProperty("songname")]
    public string SongName { get; set; } = "";

    /// <summary>
    /// 歌手。
    /// </summary>
    [JsonProperty("singer")]
    public List<QQMusicSinger> Singer { get; set; } = [];

    /// <summary>
    /// 专辑名。
    /// </summary>
    [JsonProperty("albumname")]
    public string AlbumName { get; set; } = "";

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
