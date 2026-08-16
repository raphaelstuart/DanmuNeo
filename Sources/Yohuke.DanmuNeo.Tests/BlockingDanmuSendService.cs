using System.Threading;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

internal class BlockingDanmuSendService : IDanmuSendService
{
    private readonly TaskCompletionSource firstSendStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource firstSendRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int sendCount;

    public event EventHandler<DanmuFeedItem>? RecordCreated;

    public Task FirstSendStarted => firstSendStarted.Task;

    public void ReleaseFirstSend()
    {
        firstSendRelease.TrySetResult();
    }

    public async Task<DanmuSendResult> SendAsync(
        string roomId,
        string message,
        BilibiliAccount? account,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Increment(ref sendCount) == 1)
        {
            firstSendStarted.TrySetResult();
            await firstSendRelease.Task;
        }

        var record = new DanmuFeedItem
        {
            UserName = roomId,
            Content = message,
            IsLocalRecord = true,
            IsSendAccepted = true,
            Status = "接口已接受"
        };
        RecordCreated?.Invoke(this, record);
        return new()
        {
            IsAccepted = true,
            RequestedAt = DateTimeOffset.UtcNow,
            Record = record
        };
    }

    public List<string> SplitMessage(string message, int maxLength)
    {
        return [message];
    }
}
