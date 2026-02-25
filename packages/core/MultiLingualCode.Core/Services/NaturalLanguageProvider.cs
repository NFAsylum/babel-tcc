using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Services;

public class NaturalLanguageProvider : INaturalLanguageProvider
{
    public string TranslationsBasePath { get; }
    public Dictionary<string, LanguageTable> LoadedTables = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, KeywordTable> KeywordTables = new(StringComparer.OrdinalIgnoreCase);
    public IdentifierMap IdentifierMapData = new();
    public LanguageTable ActiveTable = new();
    public KeywordTable ActiveKeywordTable = new();
    public bool HasActiveTable { get; set; }

    public string LanguageCode { get; }
    public string LanguageName { get; set; } = "";

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

        return OperationResultGeneric<NaturalLanguageProvider>.Ok(new NaturalLanguageProvider(languageCode, translationsBasePath));
    }

    public NaturalLanguageProvider(string languageCode, string translationsBasePath)
    {
        LanguageCode = languageCode;
        TranslationsBasePath = translationsBasePath;
    }

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

    public OperationResultGeneric<string> TranslateKeyword(int keywordId)
    {
        if (!HasActiveTable)
        {
            return OperationResultGeneric<string>.Fail("No active translation table loaded.");
        }

        return ActiveTable.GetTranslation(keywordId);
    }

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

    public OperationResultGeneric<string> GetOriginalKeyword(int keywordId)
    {
        if (!HasActiveTable)
        {
            return OperationResultGeneric<string>.Fail("No active keyword table loaded.");
        }

        return ActiveKeywordTable.GetKeyword(keywordId);
    }

    public OperationResultGeneric<string> TranslateIdentifier(string identifier, IdentifierContext context)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return OperationResultGeneric<string>.Fail("Identifier is empty.");
        }

        return IdentifierMapData.GetTranslated(identifier);
    }

    public async Task LoadIdentifierMapAsync(string filePath)
    {
        OperationResultGeneric<IdentifierMap> result = await IdentifierMap.LoadFromAsync(filePath);
        if (result.IsSuccess)
        {
            IdentifierMapData = result.Value;
        }
    }

    public void SetIdentifierMap(IdentifierMap map)
    {
        IdentifierMapData = map;
    }

    public KeywordTable GetActiveKeywordTable()
    {
        return ActiveKeywordTable;
    }

    public LanguageTable GetActiveLanguageTable()
    {
        return ActiveTable;
    }

    public bool IsLoaded(string programmingLanguage)
    {
        return LoadedTables.ContainsKey(programmingLanguage.ToLowerInvariant());
    }
}
