using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Translation;

public class LanguageTable
{
    public Dictionary<int, string> IdToTranslation = new();
    public Dictionary<string, int> TranslationToId = new(StringComparer.OrdinalIgnoreCase);

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
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (KeyValuePair<int, string> kvp in IdToTranslation)
            {
                result[kvp.Key.ToString()] = kvp.Value;
            }
            return result;
        }
        set
        {
            IdToTranslation = new Dictionary<int, string>();
            TranslationToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> kvp in value)
            {
                if (int.TryParse(kvp.Key, out int id) && !string.IsNullOrEmpty(kvp.Value))
                {
                    IdToTranslation[id] = kvp.Value;
                    TranslationToId[kvp.Value] = id;
                }
            }
        }
    }

    public OperationResultGeneric<string> GetTranslation(int keywordId)
    {
        if (IdToTranslation.TryGetValue(keywordId, out string? translation) && translation is not null)
        {
            return OperationResultGeneric<string>.Ok(translation);
        }

        return OperationResultGeneric<string>.Fail($"Translation not found for keyword id: {keywordId}");
    }

    public int GetKeywordId(string translatedKeyword)
    {
        if (string.IsNullOrEmpty(translatedKeyword))
        {
            return -1;
        }

        if (TranslationToId.TryGetValue(translatedKeyword, out int id))
        {
            return id;
        }

        return -1;
    }

    public int Count => IdToTranslation.Count;

    public static OperationResultGeneric<LanguageTable> LoadFrom(string filePath)
    {
        return JsonFileReader.ReadFromFile<LanguageTable>(filePath, JsonOptions.Default);
    }

    public static async Task<OperationResultGeneric<LanguageTable>> LoadFromAsync(string filePath)
    {
        return await JsonFileReader.ReadFromFileAsync<LanguageTable>(filePath, JsonOptions.Default);
    }
}
