namespace MultiLingualCode.Core.Models;

/// <summary>
/// Provides contextual information about an identifier being translated, including its name, kind, and location.
/// </summary>
public class IdentifierContext
{
    /// <summary>
    /// Gets or sets the original name of the identifier in the source code.
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the syntactic kind of the identifier (e.g., variable, method, class).
    /// </summary>
    public IdentifierKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified name of the type that contains this identifier.
    /// </summary>
    public string ContainingType { get; set; } = "";

    /// <summary>
    /// Gets or sets the file path where this identifier is declared.
    /// </summary>
    public string FilePath { get; set; } = "";
}
