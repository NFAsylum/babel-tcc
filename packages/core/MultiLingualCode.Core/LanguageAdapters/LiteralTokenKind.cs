namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Classifies the kind of a Roslyn literal token.
/// </summary>
public enum LiteralTokenKind
{
    /// <summary>
    /// A numeric literal (integer or floating-point).
    /// </summary>
    Numeric,

    /// <summary>
    /// A string literal.
    /// </summary>
    String,

    /// <summary>
    /// A character literal.
    /// </summary>
    Character,

    /// <summary>
    /// Any other literal type not explicitly classified.
    /// </summary>
    Other
}
