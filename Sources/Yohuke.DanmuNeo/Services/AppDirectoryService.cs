namespace Yohuke.DanmuNeo.Services;

/// <summary>
/// 提供应用目录路径。
/// </summary>
public class AppDirectoryService
{
    /// <summary>
    /// 初始化应用目录服务。
    /// </summary>
    public AppDirectoryService(string? configDirectory = null)
    {
        ConfigDirectory = configDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Yohuke.DanmuNeo");
        CacheDirectory = Path.Combine(ConfigDirectory, "Cache");
        AvatarCacheDirectory = Path.Combine(CacheDirectory, "Avatars");
    }

    /// <summary>
    /// 配置目录。
    /// </summary>
    public string ConfigDirectory { get; }

    /// <summary>
    /// 缓存目录。
    /// </summary>
    public string CacheDirectory { get; }

    /// <summary>
    /// 主播头像缓存目录。
    /// </summary>
    public string AvatarCacheDirectory { get; }

    /// <summary>
    /// 确保应用目录存在。
    /// </summary>
    public void EnsureDirectories()
    {
        Directory.CreateDirectory(ConfigDirectory);
        Directory.CreateDirectory(CacheDirectory);
        Directory.CreateDirectory(AvatarCacheDirectory);
    }
}
