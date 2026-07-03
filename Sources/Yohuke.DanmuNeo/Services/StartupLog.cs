namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 记录应用启动过程。
/// </summary>
public static class StartupLog
{
    private static readonly object LOCKER = new();

    /// <summary>
    /// 追加启动日志。
    /// </summary>
    public static void Append(string message)
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Yohuke.DanmuNeo");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "startup.log");

            lock (LOCKER)
            {
                File.AppendAllText(path, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
            }
        }
        catch
        {
        }
    }
}
