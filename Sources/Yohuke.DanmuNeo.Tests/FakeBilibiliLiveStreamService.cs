using System.Threading;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class FakeBilibiliLiveStreamService : IBilibiliLiveStreamService
{
    public LiveStreamPlaySource Source { get; set; } = new()
    {
        RoomId = "27016766",
        LiveStatus = 1,
        LiveTime = 1783076297,
        StreamUrl = "https://cdn.example.com/live/test.flv?token=abc",
        CurrentQuality = 10000,
        CurrentQualityDescription = "原画",
        QualityOptions =
        [
            new()
            {
                Quality = 10000,
                Description = "原画"
            }
        ]
    };

    public List<int> RequestedQualities { get; } = [];

    public Task<LiveStreamPlaySource> GetPlayableSourceAsync(
        string roomId,
        int quality,
        string? cookie,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        RequestedQualities.Add(quality);
        return Task.FromResult(Source);
    }
}
