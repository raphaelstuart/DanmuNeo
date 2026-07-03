using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class AppDirectoryServiceTests
{
    [Fact]
    public void DefaultConfigDirectoryUsesApplicationData()
    {
        var service = new AppDirectoryService();
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yohuke.DanmuNeo");

        Assert.Equal(expected, service.ConfigDirectory);
    }

    [Fact]
    public void EnsureDirectoriesCreatesCacheAndAvatarDirectories()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppDirectoryService(directory);

        service.EnsureDirectories();

        Assert.True(Directory.Exists(service.ConfigDirectory));
        Assert.True(Directory.Exists(service.CacheDirectory));
        Assert.True(Directory.Exists(service.AvatarCacheDirectory));
    }
}
