using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐新版搜索正文。
/// </summary>
public class QQMusicSearchV2Body
{
    /// <summary>
    /// 歌曲容器。
    /// </summary>
    [JsonProperty("song")]
    public QQMusicSearchV2SongContainer? Song { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
