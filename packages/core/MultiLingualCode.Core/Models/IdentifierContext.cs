namespace MultiLingualCode.Core.Models;

/// <summary>
/// Context information for translating an identifier.
/// </summary>
public class IdentifierContext
{
    /// <summary>
    /// The original identifier name.
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// The kind of identifier (variable, method, class, etc.).
    /// </summary>
    public IdentifierKind Kind { get; set; }

    /// <summary>
    /// Fully qualified name of the containing type or namespace (if available).
    /// </summary>
    public string? ContainingType { get; set; }

    /// <summary>
    /// Path of the source file containing this identifier.
    /// </summary>
    public string? FilePath { get; set; }
}

/// <summary>
/// Kinds of identifiers in source code.
/// </summary>
public enum IdentifierKind
{
    Variable,
    Parameter,
    Method,
    Property,
    Class,
    Interface,
    Namespace,
    Enum,
    Field,
    Other
}
