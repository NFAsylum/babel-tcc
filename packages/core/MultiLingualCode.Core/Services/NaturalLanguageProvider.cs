using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Provides keyword and identifier translation using loaded translation tables and identifier maps.
/// </summary>
public class NaturalLanguageProvider : INaturalLanguageProvider
{
    /// <summary>
    /// The base directory path where translation table files are stored.
    /// </summary>
    public string TranslationsBasePath { get; init; } = "";

    /// <summary>
    /// Cache of loaded language translation tables, keyed by programming language name (case-insensitive).
    /// </summary>
    public Dictionary<string, LanguageTable> LoadedTables = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Cache of loaded keyword tables, keyed by programming language name (case-insensitive).
    /// </summary>
    public Dictionary<string, KeywordTable> KeywordTables = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The currently loaded identifier map used for translating user-defined identifiers.
    /// </summary>
    public IdentifierMap IdentifierMapData = new();

    /// <summary>
    /// The currently active language translation table.
    /// </summary>
    public LanguageTable ActiveTable = new();

    /// <summary>
    /// The currently active keyword table.
    /// </summary>
    public KeywordTable ActiveKeywordTable = new();

    /// <summary>
    /// Indicates whether a translation table has been loaded and is active.
    /// </summary>
    public bool HasActiveTable { get; set; }

    /// <summary>
    /// The natural language code this provider translates to (e.g., "pt-BR").
    /// </summary>
    public string LanguageCode { get; init; } = "";

    /// <summary>
    /// The display name of the active natural language.
    /// </summary>
    public string LanguageName { get; set; } = "";

    /// <summary>
    /// Creates a new <see cref="NaturalLanguageProvider"/> with the specified language code and translations path.
    /// </summary>
    /// <param name="languageCode">The target natural language code (e.g., "pt-BR").</param>
    /// <param name="translationsBasePath">The base directory path for translation files.</param>
    /// <returns>An operation result containing the created provider, or a failure if parameters are invalid.</returns>
    public static OperationResultGeneric<NaturalLanguageProvider> Create(string languageCode, string translationsBasePath)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            return OperationResultGeneric<NaturalLanguageProvider>.Fail("Language code cannot be empty.");
        }

        if (string.IsNullOrEmpty(translationsBasePath))
        {
            return OperationResultGeneric<NaturalLanguageProvider>.Fail("Translations base path cannot be empty.");
        }

        return OperationResultGeneric<NaturalLanguageProvider>.Ok(new NaturalLanguageProvider { LanguageCode = languageCode, TranslationsBasePath = translationsBasePath });
    }

    /// <summary>
    /// Loads the keyword table and language translation table for a programming language, setting them as active.
    /// </summary>
    /// <param name="programmingLanguage">The programming language name (e.g., "csharp").</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public async Task<OperationResult> LoadTranslationTableAsync(string programmingLanguage)
    {
        string langKey = programmingLanguage.ToLowerInvariant();

        if (!KeywordTables.ContainsKey(langKey))
        {
            string keywordsPath = Path.Combine(
                TranslationsBasePath,
                "programming-languages",
                langKey,
                "keywords-base.json");

            OperationResultGeneric<KeywordTable> keywordResult = await KeywordTable.LoadFromAsync(keywordsPath);
            if (!keywordResult.IsSuccess)
            {
                return OperationResult.Fail($"Failed to load keyword table: {keywordResult.ErrorMessage}");
            }

            KeywordTables[langKey] = keywordResult.Value;
        }

        if (!LoadedTables.ContainsKey(langKey))
        {
            string translationPath = Path.Combine(
                TranslationsBasePath,
                "natural-languages",
                LanguageCode,
                $"{langKey}.json");

            OperationResultGeneric<LanguageTable> tableResult = await LanguageTable.LoadFromAsync(translationPath);
            if (!tableResult.IsSuccess)
            {
                return OperationResult.Fail($"Failed to load translation table: {tableResult.ErrorMessage}");
            }

            LoadedTables[langKey] = tableResult.Value;
        }

        ActiveTable = LoadedTables[langKey];
        ActiveKeywordTable = KeywordTables[langKey];
        HasActiveTable = true;
        LanguageName = ActiveTable.LanguageName;
        return OperationResult.Ok();
    }

    /// <summary>
    /// Translates a keyword ID to its natural language equivalent using the active translation table.
    /// </summary>
    /// <param name="keywordId">The keyword ID to translate.</param>
    /// <returns>An operation result containing the translated keyword, or a failure if no table is active or the ID is not found.</returns>
    public OperationResultGeneric<string> TranslateKeyword(int keywordId)
    {
        if (!HasActiveTable)
        {
            return OperationResultGeneric<string>.Fail("No active translation table loaded.");
        }

        return ActiveTable.GetTranslation(keywordId);
    }

    /// <summary>
    /// Looks up the keyword ID for a translated natural language keyword string.
    /// </summary>
    /// <param name="translatedKeyword">The translated keyword to reverse-lookup.</param>
    /// <returns>The keyword ID, or -1 if not found or no table is active.</returns>
    public int ReverseTranslateKeyword(string translatedKeyword)
    {
        if (string.IsNullOrEmpty(translatedKeyword))
        {
            return -1;
        }

        if (!HasActiveTable)
        {
            return -1;
        }

        return ActiveTable.GetKeywordId(translatedKeyword);
    }

    /// <summary>
    /// Gets the original programming language keyword string for a given keyword ID.
    /// </summary>
    /// <param name="keywordId">The keyword ID to look up.</param>
    /// <returns>An operation result containing the original keyword, or a failure if no table is active or the ID is not found.</returns>
    public OperationResultGeneric<string> GetOriginalKeyword(int keywordId)
    {
        if (!HasActiveTable)
        {
            return OperationResultGeneric<string>.Fail("No active keyword table loaded.");
        }

        return ActiveKeywordTable.GetKeyword(keywordId);
    }

    /// <summary>
    /// Translates a user-defined identifier using the loaded identifier map.
    /// </summary>
    /// <param name="identifier">The original identifier name to translate.</param>
    /// <param name="context">The context in which the identifier appears.</param>
    /// <returns>An operation result containing the translated identifier, or a failure if not found.</returns>
    public OperationResultGeneric<string> TranslateIdentifier(string identifier, IdentifierContext context)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return OperationResultGeneric<string>.Fail("Identifier is empty.");
        }

        return IdentifierMapData.GetTranslated(identifier);
    }

    /// <summary>
    /// Loads an identifier map from a JSON file asynchronously, replacing the current map if successful.
    /// </summary>
    /// <param name="filePath">The path to the identifier map JSON file.</param>
    public async Task LoadIdentifierMapAsync(string filePath)
    {
        OperationResultGeneric<IdentifierMap> result = await IdentifierMap.LoadFromAsync(filePath);
        if (result.IsSuccess)
        {
            IdentifierMapData = result.Value;
        }
    }

    /// <summary>
    /// Replaces the current identifier map with the provided one.
    /// </summary>
    /// <param name="map">The identifier map to use.</param>
    public void SetIdentifierMap(IdentifierMap map)
    {
        IdentifierMapData = map;
    }

    /// <summary>
    /// Returns the currently active keyword table.
    /// </summary>
    /// <returns>The active <see cref="KeywordTable"/>.</returns>
    public KeywordTable GetActiveKeywordTable()
    {
        return ActiveKeywordTable;
    }

    /// <summary>
    /// Returns the currently active language translation table.
    /// </summary>
    /// <returns>The active <see cref="LanguageTable"/>.</returns>
    public LanguageTable GetActiveLanguageTable()
    {
        return ActiveTable;
    }

    /// <summary>
    /// Checks whether a translation table has been loaded for the specified programming language.
    /// </summary>
    /// <param name="programmingLanguage">The programming language name to check.</param>
    /// <returns>True if a translation table is loaded for that language; false otherwise.</returns>
    public bool IsLoaded(string programmingLanguage)
    {
        return LoadedTables.ContainsKey(programmingLanguage.ToLowerInvariant());
    }
}
