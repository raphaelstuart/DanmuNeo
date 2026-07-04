using Newtonsoft.Json;
using Yohuke.DanmuNeo.Apis.Models.Bilibili;
using Yohuke.DanmuNeo.Apis.Models.Common;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class BilibiliLivePlayInfoSelectorTests
{
    [Fact]
    public void SelectParsesLiveJsonAndPrefersFlvAvc()
    {
        var response = JsonConvert.DeserializeObject<ApiResponse<BilibiliLivePlayInfoData>>(LiveJson);

        var source = BilibiliLivePlayInfoSelector.Select(response?.Data, "27016766");

        Assert.Equal("27016766", source.RoomId);
        Assert.Equal(1, source.LiveStatus);
        Assert.Equal("https://cdn.example.com/live/test.flv?token=abc", source.StreamUrl);
        Assert.Equal(10000, source.CurrentQuality);
        Assert.Equal("原画", source.CurrentQualityDescription);
        Assert.Equal([10000, 400], source.QualityOptions.Select(option => option.Quality));
    }

    [Fact]
    public void SelectRejectsOfflineRoom()
    {
        var response = JsonConvert.DeserializeObject<ApiResponse<BilibiliLivePlayInfoData>>(OfflineJson);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            BilibiliLivePlayInfoSelector.Select(response?.Data, "22347054"));

        Assert.Equal("当前直播间未开播", exception.Message);
    }

    [Fact]
    public void SelectRejectsMissingPlayUrl()
    {
        var response = JsonConvert.DeserializeObject<ApiResponse<BilibiliLivePlayInfoData>>(MissingPlayUrlJson);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            BilibiliLivePlayInfoSelector.Select(response?.Data, "27016766"));

        Assert.Equal("没有可用直播流", exception.Message);
    }

    private const string LiveJson = """
                                    {
                                      "code": 0,
                                      "message": "0",
                                      "data": {
                                        "room_id": 27016766,
                                        "uid": 3493105972021270,
                                        "live_status": 1,
                                        "live_time": 1783076297,
                                        "playurl_info": {
                                          "playurl": {
                                            "g_qn_desc": [
                                              { "qn": 10000, "desc": "原画" },
                                              { "qn": 400, "desc": "蓝光" },
                                              { "qn": 250, "desc": "超清" }
                                            ],
                                            "stream": [
                                              {
                                                "protocol_name": "http_hls",
                                                "format": [
                                                  {
                                                    "format_name": "ts",
                                                    "codec": [
                                                      {
                                                        "codec_name": "avc",
                                                        "current_qn": 10000,
                                                        "accept_qn": [10000, 400],
                                                        "base_url": "/live/test.m3u8?",
                                                        "url_info": [
                                                          { "host": "https://hls.example.com", "extra": "token=abc" }
                                                        ]
                                                      }
                                                    ]
                                                  }
                                                ]
                                              },
                                              {
                                                "protocol_name": "http_stream",
                                                "format": [
                                                  {
                                                    "format_name": "flv",
                                                    "codec": [
                                                      {
                                                        "codec_name": "avc",
                                                        "current_qn": 10000,
                                                        "accept_qn": [10000, 400],
                                                        "base_url": "/live/test.flv?",
                                                        "url_info": [
                                                          { "host": "https://cdn.example.com", "extra": "token=abc" }
                                                        ]
                                                      }
                                                    ]
                                                  }
                                                ]
                                              }
                                            ]
                                          }
                                        }
                                      }
                                    }
                                    """;

    private const string OfflineJson = """
                                       {
                                         "code": 0,
                                         "message": "0",
                                         "data": {
                                           "room_id": 22347054,
                                           "live_status": 0,
                                           "live_time": 0,
                                           "playurl_info": null
                                         }
                                       }
                                       """;

    private const string MissingPlayUrlJson = """
                                              {
                                                "code": 0,
                                                "message": "0",
                                                "data": {
                                                  "room_id": 27016766,
                                                  "live_status": 1,
                                                  "live_time": 1783076297,
                                                  "playurl_info": null
                                                }
                                              }
                                              """;
}