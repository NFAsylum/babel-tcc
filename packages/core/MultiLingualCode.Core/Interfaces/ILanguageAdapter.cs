using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Adapter for a programming language. Each supported language (C#, Python, etc.)
/// implements this interface to provide parsing, code generation, and keyword mapping.
/// </summary>
public interface ILanguageAdapter
{
    /// <summary>
    /// Name of the programming language (e.g. "CSharp", "Python").
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// File extensions supported by this adapter (e.g. [".cs"], [".py"]).
    /// </summary>
    string[] FileExtensions { get; }

    /// <summary>
    /// Version of this adapter.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Parses source code and returns an AST representation.
    /// </summary>
    /// <param name="sourceCode">The source code to parse.</param>
    /// <returns>Root node of the parsed AST.</returns>
    ASTNode Parse(string sourceCode);

    /// <summary>
    /// Generates source code from an AST.
    /// </summary>
    /// <param name="ast">Root node of the AST.</param>
    /// <returns>Generated source code string.</returns>
    string Generate(ASTNode ast);

    /// <summary>
    /// Returns the keyword map for this language (keyword text -> numeric ID).
    /// </summary>
    Dictionary<string, int> GetKeywordMap();

    /// <summary>
    /// Validates the syntax of the given source code.
    /// </summary>
    /// <param name="sourceCode">The source code to validate.</param>
    /// <returns>Validation result with diagnostics.</returns>
    ValidationResult ValidateSyntax(string sourceCode);

    /// <summary>
    /// Extracts user-defined identifiers from source code.
    /// </summary>
    /// <param name="sourceCode">The source code to analyze.</param>
    /// <returns>List of identifier names found in the code.</returns>
    List<string> ExtractIdentifiers(string sourceCode);
}
