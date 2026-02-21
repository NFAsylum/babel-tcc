using System.Collections.Concurrent;
using System.Text.Json;

namespace MultiLingualCode.Core.Utilities;

/// <summary>
/// Loads and caches JSON files from the filesystem.
/// </summary>
public class JsonLoader
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly JsonSerializerOptions _options;

    public JsonLoader()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    /// <summary>
    /// Loads and deserializes a JSON file, caching the result.
    /// Subsequent calls with the same path return the cached instance.
    /// </summary>
    /// <typeparam name="T">Type to deserialize into.</typeparam>
    /// <param name="filePath">Absolute or relative path to the JSON file.</param>
    /// <returns>Deserialized object.</returns>
    /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
    /// <exception cref="JsonException">If the JSON is invalid or cannot be deserialized.</exception>
    public T Load<T>(string filePath) where T : class
    {
        var fullPath = Path.GetFullPath(filePath);

        if (_cache.TryGetValue(fullPath, out var cached))
            return (T)cached;

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"JSON file not found: {fullPath}", fullPath);

        var json = File.ReadAllText(fullPath);
        var result = JsonSerializer.Deserialize<T>(json, _options)
            ?? throw new JsonException($"Failed to deserialize JSON file: {fullPath}");

        _cache.TryAdd(fullPath, result);
        return result;
    }

    /// <summary>
    /// Loads and deserializes a JSON file asynchronously, caching the result.
    /// </summary>
    public async Task<T> LoadAsync<T>(string filePath) where T : class
    {
        var fullPath = Path.GetFullPath(filePath);

        if (_cache.TryGetValue(fullPath, out var cached))
            return (T)cached;

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"JSON file not found: {fullPath}", fullPath);

        await using var stream = File.OpenRead(fullPath);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, _options)
            ?? throw new JsonException($"Failed to deserialize JSON file: {fullPath}");

        _cache.TryAdd(fullPath, result);
        return result;
    }

    /// <summary>
    /// Removes a file from the cache, forcing a reload on next access.
    /// </summary>
    public void Invalidate(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        _cache.TryRemove(fullPath, out _);
    }

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}
