using System.Text.Json;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Utilities;

public class JsonFileReader
{
    public static OperationResult<T> ReadFromFile<T>(string filePath, JsonSerializerOptions options)
    {
        if (!File.Exists(filePath))
        {
            return OperationResult<T>.Fail($"File not found: {filePath}");
        }

        try
        {
            string json = File.ReadAllText(filePath);
            T? result = JsonSerializer.Deserialize<T>(json, options);
            if (result is null)
            {
                return OperationResult<T>.Fail($"Failed to deserialize file: {filePath}");
            }

            return OperationResult<T>.Ok(result);
        }
        catch (JsonException ex)
        {
            return OperationResult<T>.Fail($"Invalid JSON in file {filePath}: {ex.Message}");
        }
    }

    public static async Task<OperationResult<T>> ReadFromFileAsync<T>(string filePath, JsonSerializerOptions options)
    {
        if (!File.Exists(filePath))
        {
            return OperationResult<T>.Fail($"File not found: {filePath}");
        }

        try
        {
            await using FileStream stream = File.OpenRead(filePath);
            T? result = await JsonSerializer.DeserializeAsync<T>(stream, options);
            if (result is null)
            {
                return OperationResult<T>.Fail($"Failed to deserialize file: {filePath}");
            }

            return OperationResult<T>.Ok(result);
        }
        catch (JsonException ex)
        {
            return OperationResult<T>.Fail($"Invalid JSON in file {filePath}: {ex.Message}");
        }
    }

    public static OperationResult WriteToFile<T>(string filePath, T data, JsonSerializerOptions options)
    {
        string directory = Path.GetDirectoryName(filePath) ?? "";
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(filePath, json);
        return OperationResult.Ok();
    }
}
