using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// 网易云音乐搜索结果。
/// </summary>
public class NetEaseSearchResult
{
    /// <summary>
    /// 歌曲列表。
    /// </summary>
    [JsonProperty("songs")]
    public List<NetEaseSong> Songs { get; set; } = [];

    /// <summary>
    /// 歌曲数量。
    /// </summary>
    [JsonProperty("songCount")]
    public int SongCount { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
