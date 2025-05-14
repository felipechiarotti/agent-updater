namespace Updater;
public static class PathExtensions
{
    public static string GetFullPath(this string path)
    {
        return Path.Combine(AppContext.BaseDirectory, path);
    }
}
