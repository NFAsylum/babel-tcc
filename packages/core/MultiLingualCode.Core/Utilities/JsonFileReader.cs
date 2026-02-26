using System.Text.Json;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Utilities;

/// <summary>
/// Provides static methods for reading and writing JSON files using the OperationResult pattern.
/// </summary>
public class JsonFileReader
{
    /// <summary>
    /// Reads and deserializes a JSON file into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <param name="options">JSON serializer options to use for deserialization.</param>
    /// <returns>An operation result containing the deserialized object or an error message.</returns>
    public static OperationResultGeneric<T> ReadFromFile<T>(string filePath, JsonSerializerOptions options)
    {
        if (!File.Exists(filePath))
        {
            return OperationResultGeneric<T>.Fail($"File not found: {filePath}");
        }

        try
        {
            string json = File.ReadAllText(filePath);
            if (JsonSerializer.Deserialize<T>(json, options) is T result)
            {
                return OperationResultGeneric<T>.Ok(result);
            }

            return OperationResultGeneric<T>.Fail($"Failed to deserialize file: {filePath}");
        }
        catch (JsonException ex)
        {
            return OperationResultGeneric<T>.Fail($"Invalid JSON in file {filePath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Asynchronously reads and deserializes a JSON file into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <param name="options">JSON serializer options to use for deserialization.</param>
    /// <returns>An operation result containing the deserialized object or an error message.</returns>
    public static async Task<OperationResultGeneric<T>> ReadFromFileAsync<T>(string filePath, JsonSerializerOptions options)
    {
        if (!File.Exists(filePath))
        {
            return OperationResultGeneric<T>.Fail($"File not found: {filePath}");
        }

        try
        {
            await using FileStream stream = File.OpenRead(filePath);
            if (await JsonSerializer.DeserializeAsync<T>(stream, options) is T result)
            {
                return OperationResultGeneric<T>.Ok(result);
            }

            return OperationResultGeneric<T>.Fail($"Failed to deserialize file: {filePath}");
        }
        catch (JsonException ex)
        {
            return OperationResultGeneric<T>.Fail($"Invalid JSON in file {filePath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Deserializes a JSON string into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">JSON serializer options to use for deserialization.</param>
    /// <returns>An operation result containing the deserialized object or an error message.</returns>
    public static OperationResultGeneric<T> ReadFromString<T>(string json, JsonSerializerOptions options)
    {
        try
        {
            if (JsonSerializer.Deserialize(json, typeof(T), options) is T result)
            {
                return OperationResultGeneric<T>.Ok(result);
            }

            return OperationResultGeneric<T>.Fail("Failed to deserialize JSON string.");
        }
        catch (JsonException ex)
        {
            return OperationResultGeneric<T>.Fail($"Invalid JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Serializes an object to JSON and writes it to a file, creating directories as needed.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="data">The object to serialize and write.</param>
    /// <param name="options">JSON serializer options to use for serialization.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public static OperationResult WriteToFile<T>(string filePath, T data, JsonSerializerOptions options)
    {
        string directory = string.Empty;
        if (Path.GetDirectoryName(filePath) is string dir)
        {
            directory = dir;
        }
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(filePath, json);
        return OperationResult.Ok();
    }
}
