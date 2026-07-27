using System.Net.WebSockets;
using System.Threading;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class LiveDanmuReconnectCoordinatorTests
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 4)]
    [InlineData(2, 8)]
    [InlineData(3, 16)]
    [InlineData(4, 30)]
    [InlineData(5, 30)]
    public void GetReconnectDelayUsesExponentialBackoff(int failureCount, double expectedSeconds)
    {
        Assert.Equal(
            TimeSpan.FromSeconds(expectedSeconds),
            LiveDanmuReconnectCoordinator.GetReconnectDelay(failureCount));
    }

    [Fact]
    public async Task InitialFailureRetriesAndRaisesRecoveredOnConnection()
    {
        using var tokenSource = new CancellationTokenSource();
        var attempts = 0;
        var disconnectedCount = 0;
        var recoveredCount = 0;
        var errorCount = 0;
        var coordinator = new LiveDanmuReconnectCoordinator(
            (onConnected, _) =>
            {
                attempts++;

                if (attempts == 1)
                {
                    throw new WebSocketException("首次连接失败");
                }

                onConnected();
                tokenSource.Cancel();
                return Task.CompletedTask;
            },
            (_, _) => Task.CompletedTask);
        coordinator.Disconnected += (_, _) => disconnectedCount++;
        coordinator.Recovered += (_, _) => recoveredCount++;
        coordinator.ErrorReceived += (_, _) => errorCount++;

        await coordinator.RunAsync(tokenSource.Token);

        Assert.Equal(2, attempts);
        Assert.Equal(1, disconnectedCount);
        Assert.Equal(1, recoveredCount);
        Assert.Equal(1, errorCount);
    }

    [Fact]
    public async Task NormalConnectionReturnIsTreatedAsDisconnectAndRetried()
    {
        using var tokenSource = new CancellationTokenSource();
        var attempts = 0;
        var disconnectedCount = 0;
        var recoveredCount = 0;
        var coordinator = new LiveDanmuReconnectCoordinator(
            (onConnected, _) =>
            {
                attempts++;
                onConnected();

                if (attempts == 2)
                {
                    tokenSource.Cancel();
                }

                return Task.CompletedTask;
            },
            (_, _) => Task.CompletedTask);
        coordinator.Disconnected += (_, _) => disconnectedCount++;
        coordinator.Recovered += (_, _) => recoveredCount++;

        await coordinator.RunAsync(tokenSource.Token);

        Assert.Equal(2, attempts);
        Assert.Equal(1, disconnectedCount);
        Assert.Equal(1, recoveredCount);
    }

    [Theory]
    [MemberData(nameof(ConnectionFailureCases))]
    public async Task ConnectionFailureAfterConnectedUsesReconnectPath(Exception connectionFailure)
    {
        using var tokenSource = new CancellationTokenSource();
        var attempts = 0;
        var disconnectedCount = 0;
        var recoveredCount = 0;
        var coordinator = new LiveDanmuReconnectCoordinator(
            (onConnected, _) =>
            {
                attempts++;
                onConnected();

                if (attempts == 1)
                {
                    throw connectionFailure;
                }

                tokenSource.Cancel();
                return Task.CompletedTask;
            },
            (_, _) => Task.CompletedTask);
        coordinator.Disconnected += (_, _) => disconnectedCount++;
        coordinator.Recovered += (_, _) => recoveredCount++;

        await coordinator.RunAsync(tokenSource.Token);

        Assert.Equal(2, attempts);
        Assert.Equal(1, disconnectedCount);
        Assert.Equal(1, recoveredCount);
    }

    [Fact]
    public async Task ConsecutiveFailuresRaiseDisconnectedOnceAndCapDelay()
    {
        using var tokenSource = new CancellationTokenSource();
        var delays = new List<TimeSpan>();
        var disconnectedCount = 0;
        var errorCount = 0;
        var coordinator = new LiveDanmuReconnectCoordinator(
            (_, _) => throw new WebSocketException("连接失败"),
            (delay, _) =>
            {
                delays.Add(delay);

                if (delays.Count == 6)
                {
                    tokenSource.Cancel();
                }

                return Task.CompletedTask;
            });
        coordinator.Disconnected += (_, _) => disconnectedCount++;
        coordinator.ErrorReceived += (_, _) => errorCount++;

        await coordinator.RunAsync(tokenSource.Token);

        Assert.Equal(
            [2, 4, 8, 16, 30, 30],
            delays.Select(delay => (int)delay.TotalSeconds).ToArray());
        Assert.Equal(1, disconnectedCount);
        Assert.Equal(6, errorCount);
    }

    [Fact]
    public async Task CancellationStopsWithoutDisconnectOrRetry()
    {
        using var tokenSource = new CancellationTokenSource();
        var attempts = 0;
        var disconnectedCount = 0;
        var errorCount = 0;
        var delayCount = 0;
        var coordinator = new LiveDanmuReconnectCoordinator(
            (_, cancellationToken) =>
            {
                attempts++;
                tokenSource.Cancel();
                return Task.FromCanceled(cancellationToken);
            },
            (_, _) =>
            {
                delayCount++;
                return Task.CompletedTask;
            });
        coordinator.Disconnected += (_, _) => disconnectedCount++;
        coordinator.ErrorReceived += (_, _) => errorCount++;

        await coordinator.RunAsync(tokenSource.Token);

        Assert.Equal(1, attempts);
        Assert.Equal(0, disconnectedCount);
        Assert.Equal(0, errorCount);
        Assert.Equal(0, delayCount);
    }

    public static IEnumerable<object[]> ConnectionFailureCases()
    {
        yield return [new WebSocketException("心跳发送失败")];
        yield return [new TimeoutException("心跳响应超时")];
    }
}
