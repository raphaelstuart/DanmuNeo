using System.Threading;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class FakeLivePlayerService : ILivePlayerService
{
    private int index;

    public List<string> RevokedTokens { get; } = [];

    public Task<LivePlayerSession> CreatePlayerAsync(
        LiveStreamPlaySource source,
        string? cookie,
        CancellationToken cancellationToken = default)
    {
        index++;
        return Task.FromResult(new LivePlayerSession
        {
            Token = $"token-{index}",
            PlayerUrl = $"http://127.0.0.1/player?token=token-{index}"
        });
    }

    public void Revoke(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            RevokedTokens.Add(token);
        }
    }
}
