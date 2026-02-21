namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a user-defined identifier (variable, method, class name, etc.) in the AST.
/// </summary>
public class IdentifierNode : ASTNode
{
    /// <summary>
    /// Original name of the identifier as it appears in the source code.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this identifier has a translation available (e.g. via // tradu: annotation).
    /// </summary>
    public bool IsTranslatable { get; set; }

    /// <summary>
    /// Translated name for this identifier, if available. Null if not translated.
    /// </summary>
    public string? TranslatedName { get; set; }

    /// <inheritdoc/>
    public override ASTNode Clone()
    {
        var clone = new IdentifierNode
        {
            Name = Name,
            IsTranslatable = IsTranslatable,
            TranslatedName = TranslatedName
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
