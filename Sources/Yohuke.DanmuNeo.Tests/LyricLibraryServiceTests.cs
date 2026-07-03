using Yohuke.DanmuNeo.Models.State;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class LyricLibraryServiceTests
{
    [Fact]
    public void UpsertUpdatesExistingRemoteLyric()
    {
        var service = new LyricLibraryService();
        var library = new List<LyricLibraryItem>();

        var first = service.Upsert(library, new()
        {
            Title = "歌",
            Source = "wy",
            SourceSongId = "1",
            LyricText = "旧"
        });
        var second = service.Upsert(library, new()
        {
            Title = "歌",
            Source = "wy",
            SourceSongId = "1",
            LyricText = "新"
        });

        Assert.Single(library);
        Assert.Equal(first, second);
        Assert.Equal("新", library[0].LyricText);
    }

    [Fact]
    public void SearchMatchesTitleArtistAndTags()
    {
        var service = new LyricLibraryService();
        var library = new List<LyricLibraryItem>
        {
            new()
            {
                Title = "星空",
                Artist = "歌手A",
                Tags = "开场"
            },
            new()
            {
                Title = "海",
                Artist = "歌手B",
                Tags = "结尾"
            }
        };

        var result = service.Search(library, "歌手A 开场");

        Assert.Single(result);
        Assert.Equal("星空", result[0].Title);
    }

    [Fact]
    public async Task CreateFromLocalFileReadsPythonLocalXmlFormat()
    {
        var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(filePath, """
        <local>
        <name>本地歌</name>
        <artists>歌手</artists>
        <tags>标签A;标签B</tags>
        <type>双语</type>
        <lyric>
        [00:01]歌词
        </lyric>
        </local>
        """);
        var service = new LyricLibraryService();

        var item = await service.CreateFromLocalFileAsync(filePath);

        Assert.Equal("本地歌", item.Title);
        Assert.Equal("歌手", item.Artist);
        Assert.True(item.HasTranslation);
        Assert.Contains("[00:01]歌词", item.LyricText);
    }
}
