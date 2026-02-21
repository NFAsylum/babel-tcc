using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Provider for a human language (Portuguese, Spanish, etc.).
/// Handles loading translation tables and translating keywords/identifiers.
/// </summary>
public interface INaturalLanguageProvider
{
    /// <summary>
    /// Language code in ISO format (e.g. "pt-br", "es-es").
    /// </summary>
    string LanguageCode { get; }

    /// <summary>
    /// Human-readable language name (e.g. "Portugues Brasileiro").
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// Loads translation tables for a specific programming language.
    /// </summary>
    /// <param name="programmingLanguage">The programming language name (e.g. "CSharp").</param>
    Task LoadTranslationTableAsync(string programmingLanguage);

    /// <summary>
    /// Translates a keyword by its numeric ID to the human language.
    /// </summary>
    /// <param name="keywordId">Numeric ID of the keyword.</param>
    /// <returns>Translated keyword text, or null if not found.</returns>
    string? TranslateKeyword(int keywordId);

    /// <summary>
    /// Reverse-translates a keyword from human language back to its numeric ID.
    /// </summary>
    /// <param name="translatedKeyword">The translated keyword text.</param>
    /// <returns>Numeric ID of the keyword, or -1 if not found.</returns>
    int ReverseTranslateKeyword(string translatedKeyword);

    /// <summary>
    /// Translates a custom identifier based on context.
    /// </summary>
    /// <param name="identifier">The original identifier name.</param>
    /// <param name="context">Context information about the identifier.</param>
    /// <returns>Translated identifier name, or null if no translation available.</returns>
    string? TranslateIdentifier(string identifier, IdentifierContext context);
}
