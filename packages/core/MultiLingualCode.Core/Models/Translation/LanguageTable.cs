using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Represents a translation table (e.g. pt-br/csharp.json) that maps keyword IDs to translated text.
/// Provides bidirectional lookup between numeric IDs and translated keywords.
/// </summary>
public class LanguageTable
{
    private Dictionary<int, string> _idToTranslation = new();
    private Dictionary<string, int> _translationToId = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = "";

    [JsonPropertyName("languageName")]
    public string LanguageName { get; set; } = "";

    [JsonPropertyName("programmingLanguage")]
    public string ProgrammingLanguage { get; set; } = "";

    [JsonPropertyName("translations")]
    public Dictionary<string, string> RawTranslations
    {
        get
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in _idToTranslation)
                result[kvp.Key.ToString()] = kvp.Value;
            return result;
        }
        set
        {
            _idToTranslation = new Dictionary<int, string>();
            _translationToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in value)
            {
                if (int.TryParse(kvp.Key, out var id) && !string.IsNullOrEmpty(kvp.Value))
                {
                    _idToTranslation[id] = kvp.Value;
                    _translationToId[kvp.Value] = id;
                }
            }
        }
    }

    /// <summary>
    /// Returns the translated keyword for a numeric ID, or null if not found.
    /// </summary>
    public string? GetTranslation(int keywordId)
    {
        return _idToTranslation.TryGetValue(keywordId, out var translation) ? translation : null;
    }

    /// <summary>
    /// Returns the keyword ID for a translated keyword, or -1 if not found.
    /// </summary>
    public int GetKeywordId(string translatedKeyword)
    {
        if (string.IsNullOrEmpty(translatedKeyword))
            return -1;

        return _translationToId.TryGetValue(translatedKeyword, out var id) ? id : -1;
    }

    /// <summary>
    /// Returns the total number of translations in the table.
    /// </summary>
    public int Count => _idToTranslation.Count;

    /// <summary>
    /// Loads a LanguageTable from a JSON file.
    /// </summary>
    public static LanguageTable LoadFrom(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Translation file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<LanguageTable>(json, JsonOptions.Default)
            ?? throw new JsonException($"Failed to deserialize translation file: {filePath}");
    }

    /// <summary>
    /// Loads a LanguageTable from a JSON file asynchronously.
    /// </summary>
    public static async Task<LanguageTable> LoadFromAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Translation file not found: {filePath}", filePath);

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<LanguageTable>(stream, JsonOptions.Default)
            ?? throw new JsonException($"Failed to deserialize translation file: {filePath}");
    }
}
