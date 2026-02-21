using System.Text.Json;
using System.Text.Json.Serialization;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Configuration;

public class UserPreferences
{
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = "pt-br";

    [JsonPropertyName("translateKeywords")]
    public bool TranslateKeywords { get; set; } = true;

    [JsonPropertyName("translateIdentifiers")]
    public bool TranslateIdentifiers { get; set; } = true;

    [JsonPropertyName("translationsPath")]
    public string TranslationsPath { get; set; } = "";

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    public static UserPreferences LoadFrom(string filePath)
    {
        OperationResult<UserPreferences> result = JsonFileReader.ReadFromFile<UserPreferences>(filePath, ReadOptions);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return new UserPreferences();
    }

    public static async Task<UserPreferences> LoadFromAsync(string filePath)
    {
        OperationResult<UserPreferences> result = await JsonFileReader.ReadFromFileAsync<UserPreferences>(filePath, ReadOptions);
        if (result.IsSuccess)
        {
            return result.Value;
        }

        return new UserPreferences();
    }

    public void SaveTo(string filePath)
    {
        JsonFileReader.WriteToFile(filePath, this, WriteOptions);
    }

    public static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
