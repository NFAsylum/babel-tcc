using System.Text.Json;
using System.Text.Json.Serialization;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Configuration;

/// <summary>
/// User preferences for translation behavior, loaded from and saved to a JSON configuration file.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// Gets or sets the target language code for translations (e.g., "pt-br").
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = "pt-br";

    /// <summary>
    /// Gets or sets a value indicating whether C# keywords should be translated to the target language.
    /// </summary>
    [JsonPropertyName("translateKeywords")]
    public bool TranslateKeywords { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether identifiers should be translated to the target language.
    /// </summary>
    [JsonPropertyName("translateIdentifiers")]
    public bool TranslateIdentifiers { get; set; } = true;

    /// <summary>
    /// Gets or sets the file path to the directory containing translation map files.
    /// </summary>
    [JsonPropertyName("translationsPath")]
    public string TranslationsPath { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether the translation feature is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Loads user preferences synchronously from the specified JSON file. Returns default preferences if the file cannot be read.
    /// </summary>
    /// <param name="filePath">The path to the JSON preferences file.</param>
    /// <returns>The loaded <see cref="UserPreferences"/>, or a default instance if loading fails.</returns>
    public static UserPreferences LoadFrom(string filePath)
    {
        OperationResultGeneric<UserPreferences> result = JsonFileReader.ReadFromFile<UserPreferences>(filePath, ReadOptions);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return new UserPreferences();
    }

    /// <summary>
    /// Loads user preferences asynchronously from the specified JSON file. Returns default preferences if the file cannot be read.
    /// </summary>
    /// <param name="filePath">The path to the JSON preferences file.</param>
    /// <returns>A task that resolves to the loaded <see cref="UserPreferences"/>, or a default instance if loading fails.</returns>
    public static async Task<UserPreferences> LoadFromAsync(string filePath)
    {
        OperationResultGeneric<UserPreferences> result = await JsonFileReader.ReadFromFileAsync<UserPreferences>(filePath, ReadOptions);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return new UserPreferences();
    }

    /// <summary>
    /// Saves the current preferences to the specified JSON file.
    /// </summary>
    /// <param name="filePath">The path to write the JSON preferences file.</param>
    public void SaveTo(string filePath)
    {
        JsonFileReader.WriteToFile(filePath, this, WriteOptions);
    }

    /// <summary>
    /// JSON serializer options used when reading preference files, with case-insensitive properties and comment skipping.
    /// </summary>
    public static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// JSON serializer options used when writing preference files, with indented formatting.
    /// </summary>
    public static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
