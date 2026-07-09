using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.LanguageAdapters.JavaScript;

/// <summary>
/// Language adapter for JavaScript (.js files). JavaScript is a C-like language, so this
/// adapter operates in keyword-only mode like the Portugol family: the fast Text Scan path
/// translates reserved words without a full parser, and reverse translation uses a hand-rolled
/// linear scanner that understands JavaScript comments and string literals (including backtick
/// template literals).
///
/// tradu annotations are not supported for JavaScript: identifier-level translation would
/// require a full JavaScript parser, which is out of scope. Only reserved keywords are
/// translated, which is sufficient for the educational goal of reading code in a natural language.
/// </summary>
public class JavaScriptAdapter : ILanguageAdapter, ITextScannable
{
    /// <summary>The name of the programming language handled by this adapter.</summary>
    public string LanguageName => "JavaScript";

    /// <summary>The file extensions associated with JavaScript source files.</summary>
    public string[] FileExtensions => [".js"];

    /// <summary>The version of this adapter implementation.</summary>
    public string Version => "1.0.0";

    /// <summary>Returns the scan rules used for JavaScript (C-like comments, three string forms).</summary>
    public LanguageScanRules GetScanRules() => LanguageScanRules.JavaScript;

    /// <summary>
    /// Parses JavaScript source code into a trivial AST node holding the raw source text.
    /// JavaScript does not require a structured AST because tradu annotations are not
    /// supported; the orchestrator only invokes Parse on the reverse path, where a
    /// passthrough node is sufficient for round-tripping.
    /// </summary>
    public ASTNode Parse(string sourceCode)
    {
        int endLine = 0;
        if (sourceCode.Length > 0)
        {
            endLine = sourceCode.Split('\n').Length - 1;
        }

        return new StatementNode
        {
            StatementKind = "JavaScriptUnit",
            RawText = sourceCode,
            StartPosition = 0,
            EndPosition = sourceCode.Length,
            StartLine = 0,
            EndLine = endLine
        };
    }

    /// <summary>
    /// Returns the raw source text held by the AST node. With no structural AST nodes,
    /// no replacements are required during code generation.
    /// </summary>
    public string Generate(ASTNode ast)
    {
        if (ast is StatementNode statement)
        {
            return statement.RawText;
        }
        return "";
    }

    /// <summary>Returns the mapping of JavaScript keyword text to integer IDs.</summary>
    public Dictionary<string, int> GetKeywordMap() => JavaScriptKeywordMap.GetMap();

    /// <summary>
    /// Reverts translated keywords in JavaScript code back to their canonical English form.
    /// Delegates to <see cref="JavaScriptScanner"/> which skips strings and comments.
    /// </summary>
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword)
    {
        return JavaScriptScanner.ReverseSubstitute(
            translatedCode,
            LanguageScanRules.JavaScript,
            lookupTranslatedKeyword,
            JavaScriptKeywordMap.GetText);
    }

    /// <summary>
    /// JavaScript has no embedded parser so syntax validation always succeeds. End-user
    /// errors surface when the JavaScript runtime executes the file.
    /// </summary>
    public ValidationResult ValidateSyntax(string sourceCode)
    {
        return new ValidationResult { IsValid = true, Diagnostics = new List<Diagnostic>() };
    }

    /// <summary>
    /// Returns an empty list. Identifier extraction would require a real parser;
    /// the adapter relies on the keyword-only fast path and does not expose identifiers.
    /// </summary>
    public List<string> ExtractIdentifiers(string sourceCode)
    {
        return new List<string>();
    }

    /// <summary>Returns an empty list. tradu annotations are unsupported for JavaScript.</summary>
    public List<TrailingComment> ExtractTrailingComments(string sourceCode)
    {
        return new List<TrailingComment>();
    }

    /// <summary>Returns an empty list. tradu annotations are unsupported for JavaScript.</summary>
    public List<string> GetIdentifierNamesOnLine(string sourceCode, int line)
    {
        return new List<string>();
    }

    /// <summary>Returns an empty string. tradu annotations are unsupported for JavaScript.</summary>
    public string GetFirstStringLiteralOnLine(string sourceCode, int line)
    {
        return "";
    }

    /// <summary>
    /// Returns (-1, -1). JavaScript has no method-scoped identifier mapping because tradu
    /// annotations are unsupported.
    /// </summary>
    public (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line)
    {
        return (-1, -1);
    }
}
