using Yohuke.DanmuNeo.Apis.Models.Bilibili;
using Yohuke.DanmuNeo.Models.Workspace;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 从 B 站播放信息中选择 App 支持的低延迟直播流。
/// </summary>
public static class BilibiliLivePlayInfoSelector
{
    /// <summary>
    /// 选择优先的 FLV/AVC 直播流。
    /// </summary>
    public static LiveStreamPlaySource Select(BilibiliLivePlayInfoData? data, string fallbackRoomId)
    {
        if (data is null)
        {
            throw new InvalidOperationException("直播流响应为空");
        }

        if (data.LiveStatus != 1)
        {
            throw new InvalidOperationException(data.LiveStatus == 2 ? "当前直播间轮播中" : "当前直播间未开播");
        }

        var playUrl = data.PlayUrlInfo?.PlayUrl;

        if (playUrl is null)
        {
            throw new InvalidOperationException("没有可用直播流");
        }

        var source = FindPreferredSource(playUrl);

        if (source is null)
        {
            throw new InvalidOperationException("当前直播流不支持 App 内播放");
        }

        var (codec, urlInfo) = source.Value;
        var currentQuality = codec.CurrentQuality;
        var qualityOptions = playUrl.QualityDescriptions
            .Where(item => codec.AcceptQualities.Contains(item.Quality))
            .Select(item => new LiveQualityOption
            {
                Quality = item.Quality,
                Description = item.Description
            })
            .ToList();
        var currentQualityDescription = qualityOptions
            .FirstOrDefault(item => item.Quality == currentQuality)
            ?.Description ?? currentQuality.ToString();

        return new()
        {
            RoomId = data.RoomId > 0 ? data.RoomId.ToString() : fallbackRoomId,
            LiveStatus = data.LiveStatus,
            LiveTime = data.LiveTime,
            StreamUrl = $"{urlInfo.Host}{codec.BaseUrl}{urlInfo.Extra}",
            CurrentQuality = currentQuality,
            CurrentQualityDescription = currentQualityDescription,
            QualityOptions = qualityOptions
        };
    }

    private static (BilibiliLiveCodec Codec, BilibiliLiveUrlInfo UrlInfo)? FindPreferredSource(
        BilibiliLivePlayUrl playUrl)
    {
        foreach (var stream in playUrl.Streams.Where(stream => stream.ProtocolName == "http_stream"))
        {
            foreach (var format in stream.Formats.Where(format => format.FormatName == "flv"))
            {
                foreach (var codec in format.Codecs.Where(codec => codec.CodecName == "avc"))
                {
                    var urlInfo = codec.UrlInfos.FirstOrDefault(item =>
                        !string.IsNullOrWhiteSpace(item.Host) &&
                        !string.IsNullOrWhiteSpace(codec.BaseUrl));

                    if (urlInfo is not null)
                    {
                        return (codec, urlInfo);
                    }
                }
            }
        }

        return null;
    }
}