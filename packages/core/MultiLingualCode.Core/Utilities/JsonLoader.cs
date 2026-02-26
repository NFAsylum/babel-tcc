using System.Collections.Concurrent;
using System.Text.Json;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Utilities;

/// <summary>
/// Loads and caches deserialized JSON files, avoiding repeated disk reads for the same file.
/// </summary>
public class JsonLoader
{
    /// <summary>
    /// Thread-safe cache mapping absolute file paths to their deserialized objects.
    /// </summary>
    public ConcurrentDictionary<string, object> Cache = new();

    /// <summary>
    /// JSON serializer options used for deserialization (case-insensitive, allows comments).
    /// </summary>
    public JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Loads and deserializes a JSON file, returning a cached result if available.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the deserialized object or an error message.</returns>
    public OperationResultGeneric<T> Load<T>(string filePath) where T : class
    {
        string fullPath = Path.GetFullPath(filePath);

        if (Cache.ContainsKey(fullPath))
        {
            return OperationResultGeneric<T>.Ok((T)Cache[fullPath]);
        }

        OperationResultGeneric<T> result = JsonFileReader.ReadFromFile<T>(fullPath, Options);
        if (!result.IsSuccess)
        {
            return result;
        }

        Cache.TryAdd(fullPath, result.Value);
        return result;
    }

    /// <summary>
    /// Asynchronously loads and deserializes a JSON file, returning a cached result if available.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the deserialized object or an error message.</returns>
    public async Task<OperationResultGeneric<T>> LoadAsync<T>(string filePath) where T : class
    {
        string fullPath = Path.GetFullPath(filePath);

        if (Cache.ContainsKey(fullPath))
        {
            return OperationResultGeneric<T>.Ok((T)Cache[fullPath]);
        }

        OperationResultGeneric<T> result = await JsonFileReader.ReadFromFileAsync<T>(fullPath, Options);
        if (!result.IsSuccess)
        {
            return result;
        }

        Cache.TryAdd(fullPath, result.Value);
        return result;
    }

    /// <summary>
    /// Removes a specific file from the cache, forcing a fresh read on the next load.
    /// </summary>
    /// <param name="filePath">The path of the file to invalidate.</param>
    public void Invalidate(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        Cache.TryRemove(fullPath, out _);
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public void ClearCache()
    {
        Cache.Clear();
    }
}
