using System.IO.Compression;
using Yohuke.DanmuNeo.Models.Workspace;
using Yohuke.DanmuNeo.Services;

namespace Yohuke.DanmuNeo.Tests;

public class TranslateHistoryExportServiceTests
{
    [Fact]
    public async Task ExportCreatesExcelWorkbookWithWorksheet()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new TranslateHistoryExportService();

        var filePath = await service.ExportAsync(
            [
                new()
                {
                    Time = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero),
                    Status = "已发送",
                    Content = "测试"
                }
            ],
            directory,
            "history");

        using var archive = ZipFile.OpenRead(filePath);
        var names = archive.Entries.Select(entry => entry.FullName).ToHashSet();

        Assert.Contains("[Content_Types].xml", names);
        Assert.Contains("xl/workbook.xml", names);
        Assert.Contains("xl/worksheets/sheet1.xml", names);
    }

    [Fact]
    public async Task ExportToFileCreatesExcelWorkbookAtSelectedPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "selected-history.xlsx");
        var service = new TranslateHistoryExportService();

        var exportedFilePath = await service.ExportToFileAsync(
            [
                new()
                {
                    Time = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero),
                    Status = "已发送",
                    Content = "指定位置"
                }
            ],
            filePath);

        Assert.Equal(filePath, exportedFilePath);
        Assert.True(File.Exists(filePath));

        using var archive = ZipFile.OpenRead(filePath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "xl/worksheets/sheet1.xml");
    }
}
