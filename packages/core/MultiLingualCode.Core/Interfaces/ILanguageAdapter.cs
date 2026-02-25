using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Interfaces;

public interface ILanguageAdapter
{
    string LanguageName { get; }
    string[] FileExtensions { get; }
    string Version { get; }
    ASTNode Parse(string sourceCode);
    string Generate(ASTNode ast);
    Dictionary<string, int> GetKeywordMap();
    string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword);
    ValidationResult ValidateSyntax(string sourceCode);
    List<string> ExtractIdentifiers(string sourceCode);
}
