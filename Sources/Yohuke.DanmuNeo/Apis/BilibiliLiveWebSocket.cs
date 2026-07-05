using System.Buffers;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yohuke.DanmuNeo.Apis.Models.Bilibili;
using Yohuke.DanmuNeo.Apis.Models.Common;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Apis;

/// <summary>
/// B 站直播弹幕 WebSocket 客户端。
/// </summary>
public class BilibiliLiveWebSocket : BaseApi, ILiveDanmuSocket
{
    private const string URL_GET_DANMU_INFO = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";
    private static readonly Regex TL_PATTERN1 = new(@"^【(?<speaker>[^:：]{1,5})[:：](?<content>[^】]+)", RegexOptions.Compiled);
    private static readonly Regex TL_PATTERN2 = new(@"^(?<speaker>[^\u0592✉【][^【]{0,4})?【(?<content>[^】]+)", RegexOptions.Compiled);
    private static readonly byte[] HEARTBEAT_PACKET = Convert.FromHexString("00000010001000010000000200000001");

    private readonly string roomId;
    private readonly BilibiliLoginCookie cookie;
    private readonly BilibiliWbiSigner signer = new();

    /// <summary>
    /// 接收到翻译弹幕时触发。
    /// </summary>
    public event EventHandler<BilibiliTranslatedDanmuMessage>? TranslatedDanmuReceived;

    /// <summary>
    /// 接收到普通弹幕时触发。
    /// </summary>
    public event EventHandler<BilibiliDanmuMessage>? DanmuReceived;

    /// <summary>
    /// 接收到 Super Chat 时触发。
    /// </summary>
    public event EventHandler<BilibiliSuperChatMessage>? SuperChatReceived;

    /// <summary>
    /// 监听发生错误时触发。
    /// </summary>
    public event EventHandler<Exception>? ErrorReceived;

    /// <summary>
    /// 连接中断时触发。
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// 连接恢复时触发。
    /// </summary>
    public event EventHandler? Recovered;

    /// <summary>
    /// 初始化直播弹幕 WebSocket。
    /// </summary>
    public BilibiliLiveWebSocket(long roomId, string cookie, TimeSpan? timeout = null)
        : this(roomId.ToString(), CreateCookie(cookie), timeout)
    {
    }

    /// <summary>
    /// 初始化直播弹幕 WebSocket。
    /// </summary>
    public BilibiliLiveWebSocket(string roomId, BilibiliLoginCookie cookie, TimeSpan? timeout = null)
        : base(timeout: timeout ?? TimeSpan.FromSeconds(5))
    {
        this.roomId = roomId;
        this.cookie = cookie;
    }

    /// <summary>
    /// 开始监听直播间弹幕，直到取消令牌被取消。
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var hadError = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ConnectOnceAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                StartupLog.Append($"Bilibili live websocket error roomId={roomId} exception={exception}");
                ErrorReceived?.Invoke(this, exception);

                if (!hadError)
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }

                hadError = true;
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                continue;
            }

            if (hadError)
            {
                hadError = false;
                Recovered?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// 获取直播间弹幕服务器信息。
    /// </summary>
    public async Task<ApiResponse<BilibiliDanmuInfoData>> GetDanmuInfoAsync(CancellationToken cancellationToken = default)
    {
        var parameters = await signer.FillAsync(new Dictionary<string, object?>
        {
            ["id"] = roomId,
            ["type"] = 0,
            ["web_location"] = 444.8
        }, cancellationToken);

        return await GetJsonAsync<ApiResponse<BilibiliDanmuInfoData>>(
            URL_GET_DANMU_INFO,
            parameters,
            CreateHeaders(),
            cancellationToken: cancellationToken);
    }

    private async Task ConnectOnceAsync(CancellationToken cancellationToken)
    {
        var danmuInfo = await GetDanmuInfoAsync(cancellationToken);
        var data = danmuInfo.Data ?? throw new InvalidOperationException("没有获取到弹幕服务器数据。");
        var host = data.HostList.FirstOrDefault() ?? throw new InvalidOperationException("没有可用的弹幕服务器。");
        var uri = CreateWebSocketUri(host);

        using var websocket = new ClientWebSocket();
        websocket.Options.SetRequestHeader("User-Agent", DEFAULT_USER_AGENT);
        websocket.Options.SetRequestHeader("Origin", "https://live.bilibili.com");
        websocket.Options.SetRequestHeader("Cookie", cookie.ToCookieString());
        await websocket.ConnectAsync(uri, cancellationToken);

        var enterRoomPacket = CreateEnterRoomPacket(data.Token);
        await websocket.SendAsync(enterRoomPacket, WebSocketMessageType.Binary, true, cancellationToken);
        var heartbeatTask = SendHeartbeatAsync(websocket, cancellationToken);

        try
        {
            while (websocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var packet = await ReceivePacketAsync(websocket, cancellationToken);

                if (packet.Length == 0)
                {
                    break;
                }

                AnalysePackage(packet);
            }
        }
        finally
        {
            try
            {
                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch
            {
            }

            await heartbeatTask;
        }
    }

    private async Task SendHeartbeatAsync(ClientWebSocket websocket, CancellationToken cancellationToken)
    {
        while (websocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await websocket.SendAsync(HEARTBEAT_PACKET, WebSocketMessageType.Binary, true, cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch
            {
                break;
            }
        }
    }

    private static async Task<byte[]> ReceivePacketAsync(ClientWebSocket websocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        using var stream = new MemoryStream();

        while (true)
        {
            var result = await websocket.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return [];
            }

            stream.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                return stream.ToArray();
            }
        }
    }

    private void AnalysePackage(byte[] rawData)
    {
        if (rawData.Length < 16)
        {
            return;
        }

        var packageLength = BinaryPrimitives.ReadInt32BigEndian(rawData.AsSpan(0, 4));
        var version = BinaryPrimitives.ReadInt16BigEndian(rawData.AsSpan(6, 2));
        var operation = BinaryPrimitives.ReadInt32BigEndian(rawData.AsSpan(8, 4));

        if (operation == 3)
        {
            return;
        }

        if (rawData.Length > packageLength && packageLength > 0)
        {
            AnalysePackage(rawData[..packageLength]);
            AnalysePackage(rawData[packageLength..]);
            return;
        }

        if (version == 3)
        {
            AnalysePackage(DecompressBrotli(rawData[16..]));
            return;
        }

        if ((version == 0 || version == 1) && operation == 5)
        {
            AnalyseBusinessPackage(rawData[16..]);
        }
    }

    private void AnalyseBusinessPackage(byte[] body)
    {
        var text = Encoding.UTF8.GetString(body);
        var json = JObject.Parse(text);
        var command = json.Value<string>("cmd");

        if (IsCommand(command, "DANMU_MSG"))
        {
            var danmu = ParseDanmuMessage(roomId, json);

            if (danmu is null)
            {
                return;
            }

            DanmuReceived?.Invoke(this, danmu);
            PublishTranslatedDanmu(danmu.Content);
            return;
        }

        if (IsCommand(command, "SUPER_CHAT_MESSAGE"))
        {
            var superChat = ParseSuperChatMessage(roomId, json);

            if (superChat is not null)
            {
                SuperChatReceived?.Invoke(this, superChat);
            }
        }
    }

    /// <summary>
    /// 从业务 JSON 中解析普通弹幕。
    /// </summary>
    public static BilibiliDanmuMessage? ParseDanmuMessage(string roomId, JObject json)
    {
        var info = json["info"] as JArray;

        if (info is null)
        {
            return null;
        }

        return new()
        {
            RoomId = roomId,
            Uid = info[2]?[0]?.Value<long>() ?? 0,
            UserName = info[2]?[1]?.Value<string>() ?? "",
            Content = info[1]?.Value<string>() ?? ""
        };
    }

    /// <summary>
    /// 从业务 JSON 中解析 Super Chat。
    /// </summary>
    public static BilibiliSuperChatMessage? ParseSuperChatMessage(string roomId, JObject json)
    {
        var data = json["data"];

        if (data is null)
        {
            return null;
        }

        var price = data["price"]?.Value<decimal>() ?? 0;
        var priceText = data["price_text"]?.Value<string>();

        return new()
        {
            RoomId = roomId,
            UserName = data["user_info"]?["uname"]?.Value<string>() ?? "",
            Price = price,
            PriceText = string.IsNullOrWhiteSpace(priceText) ? price.ToString("0.##") : priceText,
            Content = data["message"]?.Value<string>() ?? "",
            Timestamp = data["ts"]?.Value<long>() ?? DateTimeOffset.Now.ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// 根据弹幕服务器信息创建 WebSocket 地址。
    /// </summary>
    public static Uri CreateWebSocketUri(BilibiliDanmuHost host)
    {
        var port = host.WssPort > 0 ? $":{host.WssPort}" : "";
        return new($"wss://{host.Host}{port}/sub");
    }

    /// <summary>
    /// 判断业务命令是否匹配指定前缀。
    /// </summary>
    public static bool IsCommand(string? command, string expectedCommand)
    {
        return command?.StartsWith(expectedCommand, StringComparison.Ordinal) == true;
    }

    private void PublishTranslatedDanmu(string rawContent)
    {
        var match = TL_PATTERN1.Match(rawContent);

        if (!match.Success)
        {
            match = TL_PATTERN2.Match(rawContent);
        }

        if (match.Success)
        {
            TranslatedDanmuReceived?.Invoke(this, new()
            {
                RoomId = roomId,
                Speaker = match.Groups["speaker"].Success ? match.Groups["speaker"].Value : "",
                Content = match.Groups["content"].Value,
                RawContent = rawContent
            });
        }
    }

    private static byte[] DecompressBrotli(byte[] data)
    {
        var outputBuffer = new byte[Math.Max(data.Length * 8, 8192)];

        while (true)
        {
            var decoder = new BrotliDecoder();
            var status = decoder.Decompress(data, outputBuffer, out _, out var bytesWritten);

            if (status == OperationStatus.Done)
            {
                return outputBuffer[..bytesWritten];
            }

            if (status == OperationStatus.DestinationTooSmall)
            {
                outputBuffer = new byte[outputBuffer.Length * 2];
                continue;
            }

            throw new InvalidDataException("Brotli 弹幕包解压失败。");
        }
    }

    private byte[] CreateEnterRoomPacket(string token)
    {
        var payload = new
        {
            uid = long.TryParse(cookie.DedeUserId, out var uid) ? uid : 0,
            roomid = long.TryParse(roomId, out var parsedRoomId) ? parsedRoomId : 0,
            protover = 3,
            platform = "web",
            buvid = cookie.Buvid3,
            type = 2,
            key = token
        };
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
        return CreatePacket(body, 7);
    }

    private static byte[] CreatePacket(byte[] body, int operation)
    {
        var packet = new byte[16 + body.Length];
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(0, 4), packet.Length);
        BinaryPrimitives.WriteInt16BigEndian(packet.AsSpan(4, 2), 16);
        BinaryPrimitives.WriteInt16BigEndian(packet.AsSpan(6, 2), 1);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(8, 4), operation);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(12, 4), 1);
        body.CopyTo(packet.AsSpan(16));
        return packet;
    }

    private Dictionary<string, string> CreateHeaders()
    {
        var headers = CreateDefaultHeaders();
        headers["Cookie"] = cookie.ToCookieString();
        return headers;
    }

    private static BilibiliLoginCookie CreateCookie(string cookie)
    {
        var normalizedCookie = ApiCookieHelper.CreateBilibiliCookie(cookie);

        return new()
        {
            Buvid3 = ApiCookieHelper.GetValue(normalizedCookie, "buvid3"),
            SessData = ApiCookieHelper.GetValue(normalizedCookie, "SESSDATA"),
            BiliJct = ApiCookieHelper.GetValue(normalizedCookie, "bili_jct"),
            DedeUserId = ApiCookieHelper.GetValue(normalizedCookie, "DedeUserId")
        };
    }
}
