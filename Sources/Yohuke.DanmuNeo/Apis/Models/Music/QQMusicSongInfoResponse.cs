using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐歌曲详情响应。
/// </summary>
public class QQMusicSongInfoResponse
{
    /// <summary>
    /// 状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 歌曲数据。
    /// </summary>
    [JsonProperty("data")]
    public List<QQMusicSongV2> Data { get; set; } = [];

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
