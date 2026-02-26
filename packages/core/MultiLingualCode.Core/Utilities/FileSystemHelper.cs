namespace MultiLingualCode.Core.Utilities;

/// <summary>
/// Provides static helper methods for common file system operations.
/// </summary>
public static class FileSystemHelper
{
    /// <summary>
    /// Resolves a file path to its full absolute form, optionally relative to a base path.
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <param name="basePath">Optional base directory for relative paths. Defaults to current directory.</param>
    /// <returns>The fully resolved absolute path.</returns>
    public static string ResolvePath(string path, string basePath = "")
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        string baseDir = string.IsNullOrEmpty(basePath) ? Directory.GetCurrentDirectory() : basePath;
        return Path.GetFullPath(Path.Combine(baseDir, path));
    }

    /// <summary>
    /// Gets the lowercase file extension for the given file path.
    /// </summary>
    /// <param name="filePath">The file path to extract the extension from.</param>
    /// <returns>The file extension in lowercase (e.g. ".cs").</returns>
    public static string GetExtension(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant();
    }

    /// <summary>
    /// Checks whether the file has one of the specified extensions (case-insensitive).
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="extensions">The extensions to match against (e.g. ".cs", ".js").</param>
    /// <returns>True if the file extension matches any of the provided extensions.</returns>
    public static bool HasExtension(string filePath, params string[] extensions)
    {
        string ext = GetExtension(filePath);
        return extensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Walks up the directory tree from the start path to find the project root,
    /// identified by the presence of a marker file or directory.
    /// </summary>
    /// <param name="startPath">The path to start searching from (file or directory).</param>
    /// <param name="markerName">The name of the marker file or directory that identifies the project root.</param>
    /// <returns>The project root directory path, or an empty string if not found.</returns>
    public static string FindProjectRoot(string startPath, string markerName = ".multilingual")
    {
        string dir = File.Exists(startPath)
            ? (Path.GetDirectoryName(startPath) is string parentDir ? parentDir : string.Empty)
            : startPath;

        while (!string.IsNullOrEmpty(dir))
        {
            if (Directory.Exists(Path.Combine(dir, markerName)) ||
                File.Exists(Path.Combine(dir, markerName)))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir) is string nextDir ? nextDir : string.Empty;
        }

        return "";
    }
}
