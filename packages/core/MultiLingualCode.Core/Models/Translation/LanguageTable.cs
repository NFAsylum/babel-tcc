using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Translation table that maps keyword IDs to their natural language translations for a specific programming language.
/// </summary>
public class LanguageTable
{
    /// <summary>
    /// Dictionary mapping keyword IDs to their translated natural language strings.
    /// </summary>
    public Dictionary<int, string> IdToTranslation = new();

    /// <summary>
    /// Dictionary mapping translated natural language strings back to their keyword IDs.
    /// Exact match: the table is data and does not decide whether two casings are the same token —
    /// that is a fact about a language's lexer. Callers that need tolerance ask for it explicitly
    /// via <see cref="GetKeywordIdIgnoringCase"/>, mirroring how the forward scanner
    /// (TextScanTranslator.BuildTranslationMap) defaults to case-sensitive and opts in per language.
    /// </summary>
    public Dictionary<string, int> TranslationToId = new(StringComparer.Ordinal);

    /// <summary>
    /// Same mapping as <see cref="TranslationToId"/>, compared without case. Kept separate so the
    /// strict lookup stays strict; used only by languages whose keywords are case-insensitive
    /// (VisuAlg accepts SE, Se, se).
    /// </summary>
    public Dictionary<string, int> TranslationToIdIgnoringCase = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The version of this translation table.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    /// <summary>
    /// The language code for the natural language (e.g., "pt-BR").
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = "";

    /// <summary>
    /// The display name of the natural language (e.g., "Portugues Brasileiro").
    /// </summary>
    [JsonPropertyName("languageName")]
    public string LanguageName { get; set; } = "";

    /// <summary>
    /// The programming language this table provides translations for (e.g., "csharp").
    /// </summary>
    [JsonPropertyName("programmingLanguage")]
    public string ProgrammingLanguage { get; set; } = "";

    /// <summary>
    /// JSON-serializable dictionary of translations keyed by string IDs. Setting this property rebuilds both lookup dictionaries.
    /// </summary>
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
            TranslationToId = new Dictionary<string, int>(StringComparer.Ordinal);
            TranslationToIdIgnoringCase = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> kvp in value)
            {
                if (int.TryParse(kvp.Key, out int id) && !string.IsNullOrEmpty(kvp.Value))
                {
                    IdToTranslation[id] = kvp.Value;
                    TranslationToId[kvp.Value] = id;
                    TranslationToIdIgnoringCase[kvp.Value] = id;
                }
            }
        }
    }

    /// <summary>
    /// Gets the natural language translation for a given keyword ID.
    /// </summary>
    /// <param name="keywordId">The keyword ID to translate.</param>
    /// <returns>An operation result containing the translated string, or a failure if not found.</returns>
    public OperationResultGeneric<string> GetTranslation(int keywordId)
    {
        if (IdToTranslation.ContainsKey(keywordId))
        {
            return OperationResultGeneric<string>.Ok(IdToTranslation[keywordId]);
        }

        return OperationResultGeneric<string>.Fail($"Translation not found for keyword id: {keywordId}");
    }

    /// <summary>
    /// Gets the keyword ID for a given translated natural language string.
    /// </summary>
    /// <param name="translatedKeyword">The translated keyword to reverse-lookup.</param>
    /// <returns>The keyword ID, or -1 if not found.</returns>
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

    /// <summary>
    /// Reverse-lookup ignoring letter case, for languages whose keywords are case-insensitive.
    /// </summary>
    /// <param name="translatedKeyword">The translated keyword to reverse-lookup.</param>
    /// <returns>The keyword ID, or -1 if not found.</returns>
    public int GetKeywordIdIgnoringCase(string translatedKeyword)
    {
        if (string.IsNullOrEmpty(translatedKeyword))
        {
            return -1;
        }

        if (TranslationToIdIgnoringCase.TryGetValue(translatedKeyword, out int id))
        {
            return id;
        }

        return -1;
    }

    /// <summary>
    /// Gets the number of translations in this table.
    /// </summary>
    public int Count => IdToTranslation.Count;

    /// <summary>
    /// Loads a <see cref="LanguageTable"/> from a JSON file synchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the loaded table, or a failure if the file could not be read.</returns>
    public static OperationResultGeneric<LanguageTable> LoadFrom(string filePath)
    {
        return JsonFileReader.ReadFromFile<LanguageTable>(filePath, JsonOptions.Default);
    }

    /// <summary>
    /// Loads a <see cref="LanguageTable"/> from a JSON file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the loaded table, or a failure if the file could not be read.</returns>
    public static async Task<OperationResultGeneric<LanguageTable>> LoadFromAsync(string filePath)
    {
        return await JsonFileReader.ReadFromFileAsync<LanguageTable>(filePath, JsonOptions.Default);
    }
}
