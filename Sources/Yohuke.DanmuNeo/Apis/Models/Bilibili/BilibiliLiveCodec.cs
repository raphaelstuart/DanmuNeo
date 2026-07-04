using Newtonsoft.Json;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站直播编码播放地址。
/// </summary>
public class BilibiliLiveCodec
{
    /// <summary>
    /// 编码名称。
    /// </summary>
    [JsonProperty("codec_name")]
    public string CodecName { get; set; } = "";

    /// <summary>
    /// 当前清晰度。
    /// </summary>
    [JsonProperty("current_qn")]
    public int CurrentQuality { get; set; }

    /// <summary>
    /// 可用清晰度。
    /// </summary>
    [JsonProperty("accept_qn")]
    public List<int> AcceptQualities { get; set; } = [];

    /// <summary>
    /// 基础路径。
    /// </summary>
    [JsonProperty("base_url")]
    public string BaseUrl { get; set; } = "";

    /// <summary>
    /// CDN 地址列表。
    /// </summary>
    [JsonProperty("url_info")]
    public List<BilibiliLiveUrlInfo> UrlInfos { get; set; } = [];
}
