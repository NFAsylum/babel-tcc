namespace MultiLingualCode.Core.Utilities;

public static class FileSystemHelper
{
    public static string ResolvePath(string path, string basePath = "")
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        string baseDir = string.IsNullOrEmpty(basePath) ? Directory.GetCurrentDirectory() : basePath;
        return Path.GetFullPath(Path.Combine(baseDir, path));
    }

    public static string GetExtension(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant();
    }

    public static bool HasExtension(string filePath, params string[] extensions)
    {
        string ext = GetExtension(filePath);
        return extensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase));
    }

    public static string FindProjectRoot(string startPath, string markerName = ".multilingual")
    {
        string dir = File.Exists(startPath) ? Path.GetDirectoryName(startPath) ?? "" : startPath;

        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, markerName)) ||
                File.Exists(Path.Combine(dir, markerName)))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir) ?? "";
        }

        return "";
    }
}
