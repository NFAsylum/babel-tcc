using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Defines the contract for a programming language adapter (e.g., C#) that handles parsing, code generation, and keyword mapping.
/// </summary>
public interface ILanguageAdapter
{
    /// <summary>
    /// Gets the name of the programming language this adapter supports.
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// Gets the file extensions associated with this programming language (e.g., ".cs").
    /// </summary>
    string[] FileExtensions { get; }

    /// <summary>
    /// Gets the version of the language or adapter implementation.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Parses source code into an abstract syntax tree (AST).
    /// </summary>
    ASTNode Parse(string sourceCode);

    /// <summary>
    /// Generates source code from the given AST.
    /// </summary>
    string Generate(ASTNode ast);

    /// <summary>
    /// Returns a mapping of language keyword text to their numeric identifiers.
    /// </summary>
    Dictionary<string, int> GetKeywordMap();

    /// <summary>
    /// Converts translated keywords in code back to their original programming language keywords.
    /// </summary>
    string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword);

    /// <summary>
    /// Validates the syntax of the given source code and returns the result.
    /// </summary>
    ValidationResult ValidateSyntax(string sourceCode);

    /// <summary>
    /// Extracts all user-defined identifiers from the given source code.
    /// </summary>
    List<string> ExtractIdentifiers(string sourceCode);
}
