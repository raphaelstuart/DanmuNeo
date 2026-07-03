using Yohuke.DanmuNeo.Apis;
using Yohuke.DanmuNeo.Models.State;

namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 从音乐平台搜索并导入歌词。
/// </summary>
public class MusicLyricImportService
{
    /// <summary>
    /// 搜索音乐平台歌曲。
    /// </summary>
    public async Task<List<MusicLyricSearchResult>> SearchAsync(
        string keyword,
        string source,
        string qqMusicCookie,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        return source switch
        {
            "qq" => await SearchQQMusicAsync(keyword.Trim(), qqMusicCookie, cancellationToken),
            _ => await SearchNetEaseAsync(keyword.Trim(), cancellationToken)
        };
    }

    /// <summary>
    /// 导入指定搜索结果的歌词。
    /// </summary>
    public async Task<LyricLibraryItem> ImportAsync(
        MusicLyricSearchResult result,
        string qqMusicCookie,
        CancellationToken cancellationToken = default)
    {
        return result.Source switch
        {
            "qq" => await ImportQQMusicAsync(result, qqMusicCookie, cancellationToken),
            _ => await ImportNetEaseAsync(result, cancellationToken)
        };
    }

    private static async Task<List<MusicLyricSearchResult>> SearchNetEaseAsync(
        string keyword,
        CancellationToken cancellationToken)
    {
        using var api = new NetEaseMusicApi();
        var response = await api.SearchSongsAsync(keyword, cancellationToken: cancellationToken);

        return response.Result?.Songs.Select(song => new MusicLyricSearchResult
        {
            Source = "wy",
            SourceSongId = song.Id.ToString(),
            Title = song.Name,
            Artist = string.Join(" / ", song.Artists.Select(artist => artist.Name).Where(name => !string.IsNullOrWhiteSpace(name))),
            Album = song.Album?.Name ?? ""
        }).ToList() ?? [];
    }

    private static async Task<List<MusicLyricSearchResult>> SearchQQMusicAsync(
        string keyword,
        string qqMusicCookie,
        CancellationToken cancellationToken)
    {
        using var api = new QQMusicApi(qqMusicCookie);
        var response = await api.SearchSongsV1Async(keyword, cancellationToken: cancellationToken);

        return response.Data?.Song?.List.Select(song => new MusicLyricSearchResult
        {
            Source = "qq",
            SourceSongId = song.SongId.ToString(),
            SongMid = song.SongMid,
            Title = song.SongName,
            Artist = string.Join(" / ", song.Singer.Select(singer => singer.Name).Where(name => !string.IsNullOrWhiteSpace(name))),
            Album = song.AlbumName
        }).ToList() ?? [];
    }

    private static async Task<LyricLibraryItem> ImportNetEaseAsync(
        MusicLyricSearchResult result,
        CancellationToken cancellationToken)
    {
        using var api = new NetEaseMusicApi();
        var songId = long.TryParse(result.SourceSongId, out var parsedSongId) ? parsedSongId : 0;
        var lyric = await api.GetLyricAsync(songId, cancellationToken: cancellationToken);

        return CreateLibraryItem(
            result,
            lyric.Lyric?.Lyric ?? "",
            lyric.TranslatedLyric?.Lyric ?? "");
    }

    private static async Task<LyricLibraryItem> ImportQQMusicAsync(
        MusicLyricSearchResult result,
        string qqMusicCookie,
        CancellationToken cancellationToken)
    {
        using var api = new QQMusicApi(qqMusicCookie);
        var lyric = await api.GetLyricAsync(result.SongMid, cancellationToken: cancellationToken);

        return CreateLibraryItem(
            result,
            lyric.Lyric,
            lyric.TranslatedLyric);
    }

    private static LyricLibraryItem CreateLibraryItem(
        MusicLyricSearchResult result,
        string lyricText,
        string translatedLyricText)
    {
        return new()
        {
            Title = result.Title,
            Artist = result.Artist,
            Album = result.Album,
            Source = result.Source,
            SourceSongId = result.SourceSongId,
            Tags = string.Join(" ", result.Title, result.Artist, result.Album, result.SourceLabel),
            HasTranslation = !string.IsNullOrWhiteSpace(translatedLyricText),
            LyricText = lyricText,
            TranslatedLyricText = translatedLyricText
        };
    }
}
