namespace MultiLingualCode.Core.Utilities;

/// <summary>
/// Helper methods for filesystem operations.
/// </summary>
public static class FileSystemHelper
{
    /// <summary>
    /// Resolves a path to an absolute path. If already absolute, returns as-is.
    /// If relative, resolves against the given base directory (or current directory).
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <param name="basePath">Base directory for relative paths. Defaults to current directory.</param>
    /// <returns>Absolute path.</returns>
    public static string ResolvePath(string path, string? basePath = null)
    {
        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        var baseDir = basePath ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(baseDir, path));
    }

    /// <summary>
    /// Gets the file extension in lowercase (e.g. ".cs", ".py").
    /// Returns empty string if no extension.
    /// </summary>
    public static string GetExtension(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant();
    }

    /// <summary>
    /// Checks whether a file has one of the given extensions.
    /// Comparison is case-insensitive.
    /// </summary>
    /// <param name="filePath">Path to check.</param>
    /// <param name="extensions">Extensions to match (e.g. ".cs", ".py").</param>
    public static bool HasExtension(string filePath, params string[] extensions)
    {
        var ext = GetExtension(filePath);
        return extensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds the project root by searching upward for a marker file (e.g. ".multilingual" directory).
    /// </summary>
    /// <param name="startPath">Starting file or directory path.</param>
    /// <param name="markerName">Name of the marker file or directory to search for.</param>
    /// <returns>Path to the directory containing the marker, or null if not found.</returns>
    public static string? FindProjectRoot(string startPath, string markerName = ".multilingual")
    {
        var dir = File.Exists(startPath) ? Path.GetDirectoryName(startPath) : startPath;

        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir, markerName)) ||
                File.Exists(Path.Combine(dir, markerName)))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
