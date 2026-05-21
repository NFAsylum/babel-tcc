using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.LanguageAdapters.Portugol;

/// <summary>
/// Language adapter for VisuAlg (Cláudio Morgado dialect, .alg files).
/// VisuAlg is a Pascal-like educational pseudo-language widely used in Brazilian
/// introductory programming courses. This adapter operates in keyword-only mode:
/// the fast Text Scan path translates keywords without a full parser, and reverse
/// translation uses a hand-rolled linear scanner shared with the Portugol Studio adapter.
///
/// tradu annotations are not supported for this dialect: the educational audience
/// rarely uses identifier renaming, and a full parser is unnecessary for that purpose.
/// </summary>
public class VisuAlgAdapter : ILanguageAdapter, ITextScannable
{
    /// <summary>The name of the programming language handled by this adapter.</summary>
    public string LanguageName => "VisuAlg";

    /// <summary>The file extensions associated with VisuAlg source files.</summary>
    public string[] FileExtensions => [".alg"];

    /// <summary>The version of this adapter implementation.</summary>
    public string Version => "1.0.0";

    /// <summary>Returns the scan rules used for VisuAlg (line comments only, case-insensitive).</summary>
    public LanguageScanRules GetScanRules() => LanguageScanRules.VisuAlg;

    /// <summary>
    /// Parses VisuAlg source code into a trivial AST node holding the raw source text.
    /// VisuAlg does not require a structured AST because tradu annotations are not
    /// supported; the orchestrator only invokes Parse on the reverse path, where a
    /// passthrough node is sufficient for round-tripping.
    /// </summary>
    public ASTNode Parse(string sourceCode)
    {
        return new StatementNode
        {
            StatementKind = "VisuAlgUnit",
            RawText = sourceCode,
            StartPosition = 0,
            EndPosition = sourceCode.Length,
            StartLine = 0,
            EndLine = sourceCode.Length == 0 ? 0 : sourceCode.Split('\n').Length - 1
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

    /// <summary>Returns the mapping of VisuAlg keyword text to integer IDs.</summary>
    public Dictionary<string, int> GetKeywordMap() => VisuAlgKeywordMap.GetMap();

    /// <summary>
    /// Reverts translated keywords in VisuAlg code back to their canonical lowercase form.
    /// Delegates to <see cref="PortugolScanner"/> which skips strings and line comments.
    /// </summary>
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword)
    {
        return PortugolScanner.ReverseSubstitute(
            translatedCode,
            LanguageScanRules.VisuAlg,
            lookupTranslatedKeyword,
            VisuAlgKeywordMap.GetText);
    }

    /// <summary>
    /// VisuAlg has no embedded parser so syntax validation always succeeds. End-user
    /// errors surface when the original VisuAlg interpreter runs the file.
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

    /// <summary>Returns an empty list. tradu annotations are unsupported for VisuAlg.</summary>
    public List<TrailingComment> ExtractTrailingComments(string sourceCode)
    {
        return new List<TrailingComment>();
    }

    /// <summary>Returns an empty list. tradu annotations are unsupported for VisuAlg.</summary>
    public List<string> GetIdentifierNamesOnLine(string sourceCode, int line)
    {
        return new List<string>();
    }

    /// <summary>Returns an empty string. tradu annotations are unsupported for VisuAlg.</summary>
    public string GetFirstStringLiteralOnLine(string sourceCode, int line)
    {
        return "";
    }

    /// <summary>
    /// Returns (-1, -1). VisuAlg has no method-scoped identifier mapping because tradu
    /// annotations are unsupported.
    /// </summary>
    public (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line)
    {
        return (-1, -1);
    }
}
