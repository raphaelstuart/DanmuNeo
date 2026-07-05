using System.Threading;
using Yohuke.DanmuNeo.Apis.Models.Bilibili;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

/// <summary>
/// 测试用直播弹幕连接。
/// </summary>
public class FakeLiveDanmuSocket : ILiveDanmuSocket
{
    public event EventHandler<BilibiliDanmuMessage>? DanmuReceived;

    public event EventHandler<BilibiliSuperChatMessage>? SuperChatReceived;

    public event EventHandler<Exception>? ErrorReceived;

    public event EventHandler? Disconnected;

    public event EventHandler? Recovered;

    public BilibiliDanmuMessage? NextMessage { get; set; }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (NextMessage is not null)
        {
            DanmuReceived?.Invoke(this, NextMessage);
        }

        return Task.CompletedTask;
    }

    private void EmitSuperChat(BilibiliSuperChatMessage message)
    {
        SuperChatReceived?.Invoke(this, message);
    }

    private void EmitError(Exception exception)
    {
        ErrorReceived?.Invoke(this, exception);
    }

    private void EmitDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void EmitRecovered()
    {
        Recovered?.Invoke(this, EventArgs.Empty);
    }
}
