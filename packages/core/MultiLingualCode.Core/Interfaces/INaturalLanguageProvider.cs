using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

public interface INaturalLanguageProvider
{
    string LanguageCode { get; }
    string LanguageName { get; }
    Task LoadTranslationTableAsync(string programmingLanguage);
    OperationResultGeneric<string> TranslateKeyword(int keywordId);
    int ReverseTranslateKeyword(string translatedKeyword);
    OperationResultGeneric<string> TranslateIdentifier(string identifier, IdentifierContext context);
}
