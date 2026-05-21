using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.LanguageAdapters.Portugol;

/// <summary>
/// Language adapter for Portugol Studio (UNIVALI dialect, .por files).
/// Portugol Studio is a C-like educational language used in undergraduate CS courses,
/// with explicit block delimiters ({, }) and both line and block comments. This adapter
/// operates in keyword-only mode using the fast Text Scan path; tradu annotations are
/// not supported.
/// </summary>
public class PortugolStudioAdapter : ILanguageAdapter, ITextScannable
{
    /// <summary>The name of the programming language handled by this adapter.</summary>
    public string LanguageName => "PortugolStudio";

    /// <summary>The file extensions associated with Portugol Studio source files.</summary>
    public string[] FileExtensions => [".por"];

    /// <summary>The version of this adapter implementation.</summary>
    public string Version => "1.0.0";

    /// <summary>Returns the scan rules for Portugol Studio (line + block comments, case-sensitive).</summary>
    public LanguageScanRules GetScanRules() => LanguageScanRules.PortugolStudio;

    /// <summary>
    /// Parses Portugol Studio source code into a trivial AST node holding the raw source text.
    /// A structured AST is not built because tradu annotations are not supported for this
    /// dialect; the passthrough node is sufficient for round-tripping on the reverse path.
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
            StatementKind = "PortugolStudioUnit",
            RawText = sourceCode,
            StartPosition = 0,
            EndPosition = sourceCode.Length,
            StartLine = 0,
            EndLine = endLine
        };
    }

    /// <summary>Returns the raw source text from the AST node.</summary>
    public string Generate(ASTNode ast)
    {
        if (ast is StatementNode statement)
        {
            return statement.RawText;
        }
        return "";
    }

    /// <summary>Returns the mapping of Portugol Studio keyword text to integer IDs.</summary>
    public Dictionary<string, int> GetKeywordMap() => PortugolStudioKeywordMap.GetMap();

    /// <summary>
    /// Reverts translated keywords in Portugol Studio code back to their canonical form.
    /// Delegates to <see cref="PortugolScanner"/> which skips strings, char literals,
    /// line comments and block comments.
    /// </summary>
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword)
    {
        return PortugolScanner.ReverseSubstitute(
            translatedCode,
            LanguageScanRules.PortugolStudio,
            lookupTranslatedKeyword,
            PortugolStudioKeywordMap.GetText);
    }

    /// <summary>
    /// Portugol Studio has no embedded parser so syntax validation always succeeds.
    /// End-user errors surface when the original Portugol Studio interpreter runs the file.
    /// </summary>
    public ValidationResult ValidateSyntax(string sourceCode)
    {
        return new ValidationResult { IsValid = true, Diagnostics = new List<Diagnostic>() };
    }

    /// <summary>Returns an empty list. tradu annotations are unsupported.</summary>
    public List<string> ExtractIdentifiers(string sourceCode)
    {
        return new List<string>();
    }

    /// <summary>Returns an empty list. tradu annotations are unsupported.</summary>
    public List<TrailingComment> ExtractTrailingComments(string sourceCode)
    {
        return new List<TrailingComment>();
    }

    /// <summary>Returns an empty list. tradu annotations are unsupported.</summary>
    public List<string> GetIdentifierNamesOnLine(string sourceCode, int line)
    {
        return new List<string>();
    }

    /// <summary>Returns an empty string. tradu annotations are unsupported.</summary>
    public string GetFirstStringLiteralOnLine(string sourceCode, int line)
    {
        return "";
    }

    /// <summary>Returns (-1, -1). Method scoping is unsupported because tradu annotations are unsupported.</summary>
    public (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line)
    {
        return (-1, -1);
    }
}
