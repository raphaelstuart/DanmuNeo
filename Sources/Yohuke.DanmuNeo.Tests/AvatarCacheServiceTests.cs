using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class AvatarCacheServiceTests
{
    [Fact]
    public void CreateCacheFileNameKeepsRoomPrefixAndStableHash()
    {
        var first = AvatarCacheService.CreateCacheFileName("12345", "https://i0.hdslb.com/avatar.png");
        var second = AvatarCacheService.CreateCacheFileName("12345", "https://i0.hdslb.com/avatar.png");

        Assert.Equal(first, second);
        Assert.StartsWith("12345-", first);
        Assert.EndsWith(".png", first);
    }

    [Fact]
    public void CreateCacheFileNameFallsBackToJpgForUnknownExtension()
    {
        var fileName = AvatarCacheService.CreateCacheFileName("room/42", "https://example.com/avatar");

        Assert.StartsWith("room42-", fileName);
        Assert.EndsWith(".jpg", fileName);
    }

    [Fact]
    public void GetAvatarFilePathUsesAvatarCacheDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var directoryService = new AppDirectoryService(directory);
        var service = new AvatarCacheService(directoryService, new HttpClient());

        var filePath = service.GetAvatarFilePath("100", "https://example.com/avatar.webp");

        Assert.StartsWith(directoryService.AvatarCacheDirectory, filePath);
        Assert.EndsWith(".webp", filePath);
    }
}
