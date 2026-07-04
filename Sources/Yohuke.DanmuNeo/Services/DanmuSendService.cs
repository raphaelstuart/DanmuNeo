using Yohuke.DanmuNeo.Apis;
using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 统一管理弹幕发送队列。
/// </summary>
public class DanmuSendService : IDanmuSendService
{
    private readonly SemaphoreSlim sendLock = new(1, 1);
    private DateTimeOffset lastSentAt = DateTimeOffset.MinValue;

    /// <summary>
    /// 发送记录产生时触发。
    /// </summary>
    public event EventHandler<DanmuFeedItem>? RecordCreated;

    /// <summary>
    /// 发送弹幕。
    /// </summary>
    public async Task<bool> SendAsync(
        string roomId,
        string message,
        BilibiliAccount? account,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (account is null || string.IsNullOrWhiteSpace(account.Cookie))
        {
            Record(roomId, message, "未配置账号");
            return false;
        }

        await sendLock.WaitAsync(cancellationToken);

        try
        {
            var waitTime = TimeSpan.FromMilliseconds(settings.SendIntervalMs) - (DateTimeOffset.Now - lastSentAt);

            if (waitTime > TimeSpan.Zero)
            {
                await Task.Delay(waitTime, cancellationToken);
            }

            var api = new BilibiliApi(account.Cookie, TimeSpan.FromSeconds(settings.TimeoutSeconds));
            var response = await api.SendDanmuAsync(long.Parse(roomId), message, cancellationToken: cancellationToken);
            lastSentAt = DateTimeOffset.Now;

            if (response.Code == 0)
            {
                Record(roomId, message, "已发送");
                return true;
            }

            Record(roomId, message, response.Message ?? response.Msg ?? $"发送失败：{response.Code}");
            return false;
        }
        catch (Exception exception)
        {
            Record(roomId, message, exception.Message);
            return false;
        }
        finally
        {
            sendLock.Release();
        }
    }

    /// <summary>
    /// 按最大长度切分弹幕。
    /// </summary>
    public List<string> SplitMessage(string message, int maxLength)
    {
        if (message.Length <= maxLength)
        {
            return [message];
        }

        var result = new List<string>();
        var remaining = message;

        while (remaining.Length > maxLength)
        {
            var cutIndex = FindCutIndex(remaining, maxLength);
            result.Add(remaining[..cutIndex]);
            remaining = "…" + remaining[cutIndex..];
        }

        if (remaining.Length > 0)
        {
            result.Add(remaining);
        }

        return result;
    }

    private void Record(string roomId, string message, string status)
    {
        RecordCreated?.Invoke(this, new()
        {
            Time = DateTimeOffset.Now,
            UserName = roomId,
            Content = message,
            IsLocalRecord = true,
            Status = status
        });
    }

    private static int FindCutIndex(string message, int maxLength)
    {
        for (var index = maxLength - 1; index > Math.Max(1, maxLength / 2); index--)
        {
            if (" 　/，。：！？,.:!?)）】".Contains(message[index]))
            {
                return index + 1;
            }
        }

        return maxLength;
    }
}
