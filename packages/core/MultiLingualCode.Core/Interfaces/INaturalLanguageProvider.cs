using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

public interface INaturalLanguageProvider
{
    string LanguageCode { get; }
    string LanguageName { get; }
    Task LoadTranslationTableAsync(string programmingLanguage);
    OperationResult<string> TranslateKeyword(int keywordId);
    int ReverseTranslateKeyword(string translatedKeyword);
    OperationResult<string> TranslateIdentifier(string identifier, IdentifierContext context);
}
