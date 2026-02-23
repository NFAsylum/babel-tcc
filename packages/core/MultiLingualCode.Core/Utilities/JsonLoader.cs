using System.Collections.Concurrent;
using System.Text.Json;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Utilities;

public class JsonLoader
{
    public ConcurrentDictionary<string, object> Cache = new();
    public JsonSerializerOptions Options;

    public JsonLoader()
    {
        Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    public OperationResultGeneric<T> Load<T>(string filePath) where T : class
    {
        string fullPath = Path.GetFullPath(filePath);

        if (Cache.TryGetValue(fullPath, out object? cached) && cached is not null)
        {
            return OperationResultGeneric<T>.Ok((T)cached);
        }

        OperationResultGeneric<T> result = JsonFileReader.ReadFromFile<T>(fullPath, Options);
        if (!result.IsSuccess)
        {
            return result;
        }

        Cache.TryAdd(fullPath, result.Value);
        return result;
    }

    public async Task<OperationResultGeneric<T>> LoadAsync<T>(string filePath) where T : class
    {
        string fullPath = Path.GetFullPath(filePath);

        if (Cache.TryGetValue(fullPath, out object? cached) && cached is not null)
        {
            return OperationResultGeneric<T>.Ok((T)cached);
        }

        OperationResultGeneric<T> result = await JsonFileReader.ReadFromFileAsync<T>(fullPath, Options);
        if (!result.IsSuccess)
        {
            return result;
        }

        Cache.TryAdd(fullPath, result.Value);
        return result;
    }

    public void Invalidate(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        Cache.TryRemove(fullPath, out _);
    }

    public void ClearCache()
    {
        Cache.Clear();
    }
}
