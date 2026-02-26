using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Maps programming language keywords to unique integer IDs, enabling fast bidirectional lookups.
/// </summary>
public class KeywordTable
{
    /// <summary>
    /// Dictionary mapping keyword strings to their integer IDs (case-insensitive).
    /// </summary>
    public Dictionary<string, int> KeywordToId = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Dictionary mapping integer IDs back to their keyword strings.
    /// </summary>
    public Dictionary<int, string> IdToKeyword = new();

    /// <summary>
    /// JSON-serializable dictionary of keyword-to-ID mappings. Setting this property rebuilds both lookup dictionaries.
    /// </summary>
    [JsonPropertyName("keywords")]
    public Dictionary<string, int> Keywords
    {
        get => KeywordToId;
        set
        {
            KeywordToId = new Dictionary<string, int>(value, StringComparer.OrdinalIgnoreCase);
            IdToKeyword = new Dictionary<int, string>(value.Count);
            foreach (KeyValuePair<string, int> kvp in value)
            {
                IdToKeyword[kvp.Value] = kvp.Key;
            }
        }
    }

    /// <summary>
    /// Gets the integer ID for a keyword string.
    /// </summary>
    /// <param name="keyword">The keyword to look up.</param>
    /// <returns>The keyword ID, or -1 if not found.</returns>
    public int GetKeywordId(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return -1;
        }

        if (KeywordToId.TryGetValue(keyword, out int id))
        {
            return id;
        }

        return -1;
    }

    /// <summary>
    /// Gets the keyword string for a given integer ID.
    /// </summary>
    /// <param name="id">The keyword ID to look up.</param>
    /// <returns>An operation result containing the keyword string, or a failure if the ID is not found.</returns>
    public OperationResultGeneric<string> GetKeyword(int id)
    {
        if (IdToKeyword.ContainsKey(id))
        {
            return OperationResultGeneric<string>.Ok(IdToKeyword[id]);
        }

        return OperationResultGeneric<string>.Fail($"Keyword not found for id: {id}");
    }

    /// <summary>
    /// Gets the number of keywords in the table.
    /// </summary>
    public int Count => KeywordToId.Count;

    /// <summary>
    /// Loads a <see cref="KeywordTable"/> from a JSON file synchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the loaded table, or a failure if the file could not be read.</returns>
    public static OperationResultGeneric<KeywordTable> LoadFrom(string filePath)
    {
        return JsonFileReader.ReadFromFile<KeywordTable>(filePath, JsonOptions.Default);
    }

    /// <summary>
    /// Loads a <see cref="KeywordTable"/> from a JSON file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the loaded table, or a failure if the file could not be read.</returns>
    public static async Task<OperationResultGeneric<KeywordTable>> LoadFromAsync(string filePath)
    {
        return await JsonFileReader.ReadFromFileAsync<KeywordTable>(filePath, JsonOptions.Default);
    }
}
