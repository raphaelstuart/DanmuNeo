using System.Threading;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class FakeDanmuSendService : IDanmuSendService
{
    public event EventHandler<DanmuFeedItem>? RecordCreated;

    public List<string> SentMessages { get; } = [];

    public List<BilibiliAccount?> SentAccounts { get; } = [];

    public Task<bool> SendAsync(
        string roomId,
        string message,
        BilibiliAccount? account,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        SentMessages.Add(message);
        SentAccounts.Add(account);
        RecordCreated?.Invoke(this, new()
        {
            Time = DateTimeOffset.Now,
            UserName = roomId,
            Content = message,
            IsLocalRecord = true,
            Status = "已发送"
        });
        return Task.FromResult(true);
    }

    public List<string> SplitMessage(string message, int maxLength)
    {
        return [message];
    }
}
