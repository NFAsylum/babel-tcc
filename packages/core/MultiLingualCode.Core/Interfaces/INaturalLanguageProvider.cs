using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Defines the contract for a natural language translation provider that translates programming keywords and identifiers into a target natural language.
/// </summary>
public interface INaturalLanguageProvider
{
    /// <summary>
    /// Gets the ISO language code for the target natural language (e.g., "pt-BR").
    /// </summary>
    string LanguageCode { get; }

    /// <summary>
    /// Gets the display name of the target natural language (e.g., "Portuguese").
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// Loads the keyword translation table for the specified programming language.
    /// </summary>
    Task<OperationResult> LoadTranslationTableAsync(string programmingLanguage);

    /// <summary>
    /// Translates a programming keyword identified by its ID into the target natural language.
    /// </summary>
    OperationResultGeneric<string> TranslateKeyword(int keywordId);

    /// <summary>
    /// Looks up the keyword ID for a previously translated keyword string.
    /// </summary>
    int ReverseTranslateKeyword(string translatedKeyword);

    /// <summary>
    /// Retrieves the original programming language keyword text for the given keyword ID.
    /// </summary>
    OperationResultGeneric<string> GetOriginalKeyword(int keywordId);

    /// <summary>
    /// Translates a user-defined identifier into the target natural language using the provided context.
    /// </summary>
    OperationResultGeneric<string> TranslateIdentifier(string identifier, IdentifierContext context);
}
