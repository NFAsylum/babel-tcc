using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Interfaces;

// NOTE: Methods ExtractTrailingComments, GetIdentifierNamesOnLine, GetFirstStringLiteralOnLine,
// and GetContainingMethodRange were added to support language-agnostic tradu annotation parsing.
// See tarefa 055 for context.

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

    /// <summary>
    /// Extracts all trailing comments from the source code.
    /// </summary>
    /// <param name="sourceCode">The source code to extract comments from.</param>
    /// <returns>A list of trailing comments with their text (without comment prefix) and line numbers.</returns>
    List<TrailingComment> ExtractTrailingComments(string sourceCode);

    /// <summary>
    /// Gets the names of all identifiers on a specific line.
    /// </summary>
    /// <param name="sourceCode">The source code to analyze.</param>
    /// <param name="line">The zero-based line number.</param>
    /// <returns>A list of identifier names found on the line.</returns>
    List<string> GetIdentifierNamesOnLine(string sourceCode, int line);

    /// <summary>
    /// Gets the text of the first string literal on a specific line.
    /// </summary>
    /// <param name="sourceCode">The source code to analyze.</param>
    /// <param name="line">The zero-based line number.</param>
    /// <returns>The string literal value, or an empty string if none found.</returns>
    string GetFirstStringLiteralOnLine(string sourceCode, int line);

    /// <summary>
    /// Gets the line range of the method containing the specified line.
    /// </summary>
    /// <param name="sourceCode">The source code to analyze.</param>
    /// <param name="line">The zero-based line number inside the method.</param>
    /// <returns>A tuple of (StartLine, EndLine) for the containing method, or (-1, -1) if not inside a method.</returns>
    (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line);
}
