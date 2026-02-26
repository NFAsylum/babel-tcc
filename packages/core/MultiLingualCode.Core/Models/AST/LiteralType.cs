namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Specifies the type of a literal value in the AST.
/// </summary>
public enum LiteralType
{
    /// <summary>
    /// A string literal (e.g., "hello").
    /// </summary>
    String,

    /// <summary>
    /// A numeric literal (e.g., 42, 3.14).
    /// </summary>
    Number,

    /// <summary>
    /// A boolean literal (true or false).
    /// </summary>
    Boolean,

    /// <summary>
    /// A null literal.
    /// </summary>
    Null,

    /// <summary>
    /// A character literal (e.g., 'a').
    /// </summary>
    Char,

    /// <summary>
    /// A literal type not covered by the other categories.
    /// </summary>
    Other
}
