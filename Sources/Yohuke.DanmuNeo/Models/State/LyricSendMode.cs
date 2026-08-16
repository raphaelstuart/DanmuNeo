namespace Yohuke.DanmuNeo.Models.State;

/// <summary>
/// 歌词发送内容模式。
/// </summary>
public enum LyricSendMode
{
    /// <summary>
    /// 同时发送原文与翻译。
    /// </summary>
    Bilingual,

    /// <summary>
    /// 仅发送原文。
    /// </summary>
    OriginalOnly,

    /// <summary>
    /// 仅发送翻译。
    /// </summary>
    TranslationOnly
}
