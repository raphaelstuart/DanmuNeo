using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class DanmuEchoTrackerTests
{
    [Fact]
    public async Task MatchingObservationCompletesWaiter()
    {
        var tracker = new DanmuEchoTracker();
        var waiting = tracker.WaitAsync(42, "内容", DateTimeOffset.MinValue, TimeSpan.FromSeconds(1));

        tracker.Observe(42, "内容");

        Assert.True(await waiting);
    }

    [Fact]
    public async Task ObservationBeforeWaitIsConsumed()
    {
        var tracker = new DanmuEchoTracker();

        tracker.Observe(42, "内容");

        Assert.True(await tracker.WaitAsync(42, "内容", DateTimeOffset.MinValue, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task WrongUidOrContentDoesNotCompleteWaiter()
    {
        var tracker = new DanmuEchoTracker();
        var waiting = tracker.WaitAsync(42, "内容", DateTimeOffset.MinValue, TimeSpan.FromMilliseconds(30));

        tracker.Observe(43, "内容");
        tracker.Observe(42, "其他内容");

        Assert.False(await waiting);
    }

    [Fact]
    public async Task SameContentWaitersAreConfirmedInFifoOrder()
    {
        var tracker = new DanmuEchoTracker();
        var first = tracker.WaitAsync(42, "内容", DateTimeOffset.MinValue, TimeSpan.FromSeconds(1));
        var second = tracker.WaitAsync(42, "内容", DateTimeOffset.MinValue, TimeSpan.FromSeconds(1));

        tracker.Observe(42, "内容");
        Assert.True(await first);
        Assert.False(second.IsCompleted);

        tracker.Observe(42, "内容");
        Assert.True(await second);
    }

    [Fact]
    public async Task ExpiredObservationIsNotConsumed()
    {
        var currentTime = DateTimeOffset.UtcNow;
        var tracker = new DanmuEchoTracker(TimeSpan.FromSeconds(5), () => currentTime);
        tracker.Observe(42, "内容");
        currentTime += TimeSpan.FromSeconds(6);

        Assert.False(await tracker.WaitAsync(42, "内容", currentTime, TimeSpan.FromMilliseconds(30)));
    }

    [Fact]
    public async Task ObservationBeforeRequestStartIsNotConsumed()
    {
        var currentTime = DateTimeOffset.UtcNow;
        var tracker = new DanmuEchoTracker(TimeSpan.FromSeconds(5), () => currentTime);
        tracker.Observe(42, "内容");
        currentTime += TimeSpan.FromMilliseconds(1);

        Assert.False(await tracker.WaitAsync(42, "内容", currentTime, TimeSpan.FromMilliseconds(30)));
    }
}
