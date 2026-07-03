using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yohuke.DanmuNeo.Apis.Models.Bilibili;

/// <summary>
/// B 站二维码登录链接数据。
/// </summary>
public class BilibiliLoginQrCodeData
{
    /// <summary>
    /// 二维码内容链接。
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; } = "";

    /// <summary>
    /// 二维码登录轮询键。
    /// </summary>
    [JsonProperty("qrcode_key")]
    public string QrCodeKey { get; set; } = "";

    /// <summary>
    /// 未显式映射的字段。
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? ExtraData { get; set; }
}
