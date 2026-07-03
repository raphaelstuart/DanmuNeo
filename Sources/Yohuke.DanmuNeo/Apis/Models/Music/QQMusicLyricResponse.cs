using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐歌词响应。
/// </summary>
public class QQMusicLyricResponse
{
    /// <summary>
    /// 状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 原文歌词。
    /// </summary>
    [JsonProperty("lyric")]
    public string Lyric { get; set; } = "";

    /// <summary>
    /// 翻译歌词。
    /// </summary>
    [JsonProperty("trans")]
    public string TranslatedLyric { get; set; } = "";

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
