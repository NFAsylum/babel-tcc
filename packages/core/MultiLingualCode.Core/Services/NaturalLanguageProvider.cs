using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Concrete implementation of INaturalLanguageProvider.
/// Loads translation tables from the filesystem and provides keyword/identifier translation.
/// </summary>
public class NaturalLanguageProvider : INaturalLanguageProvider
{
    private readonly string _translationsBasePath;
    private readonly Dictionary<string, LanguageTable> _loadedTables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, KeywordTable> _keywordTables = new(StringComparer.OrdinalIgnoreCase);
    private IdentifierMap? _identifierMap;
    private LanguageTable? _activeTable;
    private KeywordTable? _activeKeywordTable;

    public string LanguageCode { get; }
    public string LanguageName { get; private set; } = "";

    /// <summary>
    /// Creates a new NaturalLanguageProvider.
    /// </summary>
    /// <param name="languageCode">ISO language code (e.g. "pt-br").</param>
    /// <param name="translationsBasePath">Root path of the translations repository.</param>
    public NaturalLanguageProvider(string languageCode, string translationsBasePath)
    {
        ArgumentNullException.ThrowIfNull(languageCode);
        ArgumentNullException.ThrowIfNull(translationsBasePath);

        LanguageCode = languageCode;
        _translationsBasePath = translationsBasePath;
    }

    /// <summary>
    /// Loads translation tables for a specific programming language.
    /// Resolves paths based on the translations repository structure:
    ///   programming-languages/{lang}/keywords-base.json
    ///   natural-languages/{languageCode}/{lang}.json
    /// </summary>
    public async Task LoadTranslationTableAsync(string programmingLanguage)
    {
        ArgumentNullException.ThrowIfNull(programmingLanguage);

        var langKey = programmingLanguage.ToLowerInvariant();

        // Load keyword table if not cached
        if (!_keywordTables.ContainsKey(langKey))
        {
            var keywordsPath = Path.Combine(
                _translationsBasePath,
                "programming-languages",
                langKey,
                "keywords-base.json");

            var keywordTable = await KeywordTable.LoadFromAsync(keywordsPath);
            _keywordTables[langKey] = keywordTable;
        }

        // Load language table if not cached
        if (!_loadedTables.ContainsKey(langKey))
        {
            var translationPath = Path.Combine(
                _translationsBasePath,
                "natural-languages",
                LanguageCode,
                $"{langKey}.json");

            var languageTable = await LanguageTable.LoadFromAsync(translationPath);
            _loadedTables[langKey] = languageTable;
        }

        _activeTable = _loadedTables[langKey];
        _activeKeywordTable = _keywordTables[langKey];
        LanguageName = _activeTable.LanguageName;
    }

    /// <summary>
    /// Translates a keyword by its numeric ID.
    /// Returns the translated text, or null if no translation exists.
    /// </summary>
    public string? TranslateKeyword(int keywordId)
    {
        return _activeTable?.GetTranslation(keywordId);
    }

    /// <summary>
    /// Reverse-translates a keyword from translated text back to its numeric ID.
    /// Returns -1 if the translated keyword is not found.
    /// </summary>
    public int ReverseTranslateKeyword(string translatedKeyword)
    {
        if (string.IsNullOrEmpty(translatedKeyword))
            return -1;

        return _activeTable?.GetKeywordId(translatedKeyword) ?? -1;
    }

    /// <summary>
    /// Translates a user-defined identifier using the loaded identifier map.
    /// Returns null if no translation is available.
    /// </summary>
    public string? TranslateIdentifier(string identifier, IdentifierContext context)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;

        return _identifierMap?.GetTranslated(identifier);
    }

    /// <summary>
    /// Loads an identifier map from a JSON file.
    /// </summary>
    public async Task LoadIdentifierMapAsync(string filePath)
    {
        _identifierMap = await IdentifierMap.LoadFromAsync(filePath);
    }

    /// <summary>
    /// Sets the identifier map directly (useful for testing or runtime updates).
    /// </summary>
    public void SetIdentifierMap(IdentifierMap map)
    {
        _identifierMap = map;
    }

    /// <summary>
    /// Returns the currently active keyword table, or null if none loaded.
    /// </summary>
    public KeywordTable? ActiveKeywordTable => _activeKeywordTable;

    /// <summary>
    /// Returns the currently active language table, or null if none loaded.
    /// </summary>
    public LanguageTable? ActiveLanguageTable => _activeTable;

    /// <summary>
    /// Returns whether a translation table has been loaded for the given programming language.
    /// </summary>
    public bool IsLoaded(string programmingLanguage)
    {
        return _loadedTables.ContainsKey(programmingLanguage.ToLowerInvariant());
    }
}
