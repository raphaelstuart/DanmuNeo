namespace Yohuke.DanmuNeo.Apis.Models.Music;

/// <summary>
/// QQ 音乐二维码登录图片响应。
/// </summary>
public class QQMusicQrCodeResponse
{
    /// <summary>
    /// 二维码图片字节。
    /// </summary>
    public byte[] ImageBytes { get; set; } = [];

    /// <summary>
    /// 二维码签名 Cookie。
    /// </summary>
    public string QrSig { get; set; } = "";

    /// <summary>
    /// 二维码轮询令牌。
    /// </summary>
    public int PtQrToken { get; set; }
}
