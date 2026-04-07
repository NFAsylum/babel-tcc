namespace MultiLingualCode.Core.Services;

/// <summary>
/// Configures how the TextScanTranslator skips non-code regions for a specific language.
/// </summary>
public class LanguageScanRules
{
    /// <summary>Line comment prefix (e.g. "//" for C#, "#" for Python). Empty if none.</summary>
    public string LineComment = "";

    /// <summary>Block comment opening (e.g. "/*" for C#). Empty if none.</summary>
    public string BlockCommentStart = "";

    /// <summary>Block comment closing (e.g. "*/" for C#). Empty if none.</summary>
    public string BlockCommentEnd = "";

    /// <summary>Whether # at start of line is a preprocessor directive (C#) instead of a comment.</summary>
    public bool HasPreprocessor = false;

    /// <summary>Whether @ before a keyword is an escaped identifier (C#).</summary>
    public bool HasEscapedIdentifiers = false;

    /// <summary>Whether triple-quoted strings (""" or ''') are supported.</summary>
    public bool HasTripleQuoteStrings = false;

    /// <summary>Whether single-quoted strings/chars are supported.</summary>
    public bool HasSingleQuoteStrings = true;

    /// <summary>Pre-defined rules for C#.</summary>
    public static LanguageScanRules CSharp = new LanguageScanRules
    {
        LineComment = "//",
        BlockCommentStart = "/*",
        BlockCommentEnd = "*/",
        HasPreprocessor = true,
        HasEscapedIdentifiers = true,
        HasTripleQuoteStrings = true,
        HasSingleQuoteStrings = true,
    };

    /// <summary>Pre-defined rules for Python.</summary>
    public static LanguageScanRules Python = new LanguageScanRules
    {
        LineComment = "#",
        BlockCommentStart = "",
        BlockCommentEnd = "",
        HasPreprocessor = false,
        HasEscapedIdentifiers = false,
        HasTripleQuoteStrings = true,
        HasSingleQuoteStrings = true,
    };
}
