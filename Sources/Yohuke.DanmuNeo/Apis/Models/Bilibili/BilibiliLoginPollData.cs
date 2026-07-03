using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站二维码登录轮询数据。
/// </summary>
public class BilibiliLoginPollData
{
    /// <summary>
    /// 登录状态码。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 登录状态消息。
    /// </summary>
    [JsonProperty("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 已确认登录后的回跳链接。
    /// </summary>
    [JsonProperty("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 刷新令牌。
    /// </summary>
    [JsonProperty("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 平台时间戳。
    /// </summary>
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
