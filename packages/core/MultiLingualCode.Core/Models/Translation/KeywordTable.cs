using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Represents the keywords-base.json file that maps programming language keywords to numeric IDs.
/// Provides bidirectional lookup between keyword text and ID.
/// </summary>
public class KeywordTable
{
    private Dictionary<string, int> _keywordToId = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<int, string> _idToKeyword = new();

    [JsonPropertyName("keywords")]
    public Dictionary<string, int> Keywords
    {
        get => _keywordToId;
        set
        {
            _keywordToId = new Dictionary<string, int>(value, StringComparer.OrdinalIgnoreCase);
            _idToKeyword = new Dictionary<int, string>(value.Count);
            foreach (var kvp in value)
            {
                _idToKeyword[kvp.Value] = kvp.Key;
            }
        }
    }

    /// <summary>
    /// Returns the numeric ID for a keyword, or -1 if not found.
    /// </summary>
    public int GetKeywordId(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return -1;

        return _keywordToId.TryGetValue(keyword, out var id) ? id : -1;
    }

    /// <summary>
    /// Returns the keyword text for a numeric ID, or null if not found.
    /// </summary>
    public string? GetKeyword(int id)
    {
        return _idToKeyword.TryGetValue(id, out var keyword) ? keyword : null;
    }

    /// <summary>
    /// Returns the total number of keywords in the table.
    /// </summary>
    public int Count => _keywordToId.Count;

    /// <summary>
    /// Loads a KeywordTable from a JSON file.
    /// </summary>
    public static KeywordTable LoadFrom(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Keywords file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<KeywordTable>(json, JsonOptions.Default)
            ?? throw new JsonException($"Failed to deserialize keywords file: {filePath}");
    }

    /// <summary>
    /// Loads a KeywordTable from a JSON file asynchronously.
    /// </summary>
    public static async Task<KeywordTable> LoadFromAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Keywords file not found: {filePath}", filePath);

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<KeywordTable>(stream, JsonOptions.Default)
            ?? throw new JsonException($"Failed to deserialize keywords file: {filePath}");
    }
}
