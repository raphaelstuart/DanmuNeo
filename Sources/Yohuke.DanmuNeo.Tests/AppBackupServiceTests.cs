using System.IO.Compression;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class AppBackupServiceTests
{
    [Fact]
    public void GetDirectorySizeReturnsNestedFileBytes()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(directory, "Cache", "Avatars");
        Directory.CreateDirectory(nested);
        File.WriteAllBytes(Path.Combine(nested, "a.bin"), [1, 2, 3, 4]);
        var service = new AppBackupService(new AppDirectoryService(directory));

        var size = service.GetDirectorySize(directory);

        Assert.Equal(4, size);
    }

    [Fact]
    public void ClearCacheDeletesAndRecreatesCacheDirectories()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var directoryService = new AppDirectoryService(directory);
        directoryService.EnsureDirectories();
        File.WriteAllText(Path.Combine(directoryService.AvatarCacheDirectory, "avatar.jpg"), "data");
        var service = new AppBackupService(directoryService);

        service.ClearCache();

        Assert.True(Directory.Exists(directoryService.CacheDirectory));
        Assert.True(Directory.Exists(directoryService.AvatarCacheDirectory));
        Assert.Empty(Directory.EnumerateFiles(directoryService.AvatarCacheDirectory));
    }

    [Fact]
    public async Task ExportBackupZipIncludesSplitStorageFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var directoryService = new AppDirectoryService(directory);
        directoryService.EnsureDirectories();
        await File.WriteAllTextAsync(Path.Combine(directory, "settings.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(directory, "workspaces.json"), "{}");
        var service = new AppBackupService(directoryService);

        var filePath = await service.ExportBackupZipAsync(Path.Combine(directory, "Exports"));

        using var archive = ZipFile.OpenRead(filePath);
        var names = archive.Entries.Select(entry => entry.FullName).ToHashSet();
        Assert.Contains("settings.json", names);
        Assert.Contains("workspaces.json", names);
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    public void FormatSizeUsesReadableUnits(long bytes, string expected)
    {
        Assert.Equal(expected, AppBackupService.FormatSize(bytes));
    }
}
