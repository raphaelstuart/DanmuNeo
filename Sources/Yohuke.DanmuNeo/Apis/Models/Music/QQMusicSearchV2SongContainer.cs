using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐新版搜索歌曲容器。
/// </summary>
public class QQMusicSearchV2SongContainer
{
    /// <summary>
    /// 歌曲列表。
    /// </summary>
    [JsonProperty("list")]
    public List<QQMusicSongV2> List { get; set; } = [];

    /// <summary>
    /// 总数。
    /// </summary>
    [JsonProperty("totalnum")]
    public int TotalNum { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
