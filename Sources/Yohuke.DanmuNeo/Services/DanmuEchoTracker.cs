namespace Yohuke.DanmuNeo.Services;

internal sealed class DanmuEchoTracker
{
    private const int MAX_RECENT_OBSERVATIONS = 128;
    private readonly object syncRoot = new();
    private readonly TimeSpan observationLifetime;
    private readonly Func<DateTimeOffset> now;
    private readonly List<KeyValuePair<string, DateTimeOffset>> recentObservations = [];
    private readonly Dictionary<string, Queue<TaskCompletionSource<bool>>> waiters = [];

    public DanmuEchoTracker(
        TimeSpan? observationLifetime = null,
        Func<DateTimeOffset>? now = null)
    {
        this.observationLifetime = observationLifetime ?? TimeSpan.FromSeconds(5);
        this.now = now ?? (() => DateTimeOffset.UtcNow);
    }

    public void Observe(long uid, string content)
    {
        var key = CreateKey(uid, content);
        TaskCompletionSource<bool>? waiter = null;

        lock (syncRoot)
        {
            RemoveExpiredObservations();

            if (waiters.TryGetValue(key, out var pending) && pending.Count > 0)
            {
                waiter = pending.Dequeue();

                if (pending.Count == 0)
                {
                    waiters.Remove(key);
                }
            }
            else
            {
                recentObservations.Add(new(key, now()));

                while (recentObservations.Count > MAX_RECENT_OBSERVATIONS)
                {
                    recentObservations.RemoveAt(0);
                }
            }
        }

        waiter?.TrySetResult(true);
    }

    public async Task<bool> WaitAsync(
        long uid,
        string content,
        DateTimeOffset notBefore,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var key = CreateKey(uid, content);
        TaskCompletionSource<bool> waiter;

        lock (syncRoot)
        {
            RemoveExpiredObservations();

            var recentIndex = recentObservations.FindIndex(item => item.Key == key && item.Value >= notBefore);

            if (recentIndex >= 0)
            {
                recentObservations.RemoveAt(recentIndex);
                return true;
            }

            waiter = new(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!waiters.TryGetValue(key, out var pending))
            {
                pending = [];
                waiters[key] = pending;
            }

            pending.Enqueue(waiter);
        }

        try
        {
            return await waiter.Task.WaitAsync(timeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            RemoveWaiter(key, waiter);
            return false;
        }
        catch (OperationCanceledException)
        {
            RemoveWaiter(key, waiter);
            throw;
        }
    }

    private void RemoveWaiter(string key, TaskCompletionSource<bool> waiter)
    {
        lock (syncRoot)
        {
            if (!waiters.TryGetValue(key, out var pending))
            {
                return;
            }

            var remaining = new Queue<TaskCompletionSource<bool>>();

            while (pending.Count > 0)
            {
                var current = pending.Dequeue();

                if (current != waiter)
                {
                    remaining.Enqueue(current);
                }
            }

            if (remaining.Count == 0)
            {
                waiters.Remove(key);
            }
            else
            {
                waiters[key] = remaining;
            }
        }
    }

    private void RemoveExpiredObservations()
    {
        var threshold = now() - observationLifetime;
        recentObservations.RemoveAll(item => item.Value < threshold);
    }

    private static string CreateKey(long uid, string content)
    {
        return $"{uid}\n{content}";
    }
}
