using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 弹幕发送队列。
/// </summary>
public interface IDanmuSendService
{
    /// <summary>
    /// 发送记录产生时触发。
    /// </summary>
    event EventHandler<DanmuFeedItem>? RecordCreated;

    /// <summary>
    /// 发送弹幕。
    /// </summary>
    Task<bool> SendAsync(
        string roomId,
        string message,
        BilibiliAccount? account,
        AppSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 按最大长度切分弹幕。
    /// </summary>
    List<string> SplitMessage(string message, int maxLength);
}
