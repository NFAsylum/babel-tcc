using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Models.Configuration;

/// <summary>
/// User preferences for the translation engine.
/// Loaded from user settings or workspace configuration.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// The natural language code for translations (e.g. "pt-br", "es-es").
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = "pt-br";

    /// <summary>
    /// Whether to translate keywords (if, else, class, etc.).
    /// </summary>
    [JsonPropertyName("translateKeywords")]
    public bool TranslateKeywords { get; set; } = true;

    /// <summary>
    /// Whether to translate user-defined identifiers (method names, variables, etc.).
    /// </summary>
    [JsonPropertyName("translateIdentifiers")]
    public bool TranslateIdentifiers { get; set; } = true;

    /// <summary>
    /// Path to the translations repository or directory.
    /// </summary>
    [JsonPropertyName("translationsPath")]
    public string TranslationsPath { get; set; } = "";

    /// <summary>
    /// Whether translation is enabled globally.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Loads UserPreferences from a JSON file.
    /// </summary>
    public static UserPreferences LoadFrom(string filePath)
    {
        if (!File.Exists(filePath))
            return new UserPreferences();

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<UserPreferences>(json, _options)
            ?? new UserPreferences();
    }

    /// <summary>
    /// Loads UserPreferences from a JSON file asynchronously.
    /// </summary>
    public static async Task<UserPreferences> LoadFromAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return new UserPreferences();

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<UserPreferences>(stream, _options)
            ?? new UserPreferences();
    }

    /// <summary>
    /// Saves the current preferences to a JSON file.
    /// </summary>
    public void SaveTo(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(this, _writeOptions);
        File.WriteAllText(filePath, json);
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
