namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a user-defined identifier (e.g., variable name, method name, class name) in the abstract syntax tree.
/// </summary>
public class IdentifierNode : ASTNode
{
    /// <summary>
    /// Gets or sets the original name of the identifier as it appears in source code.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this identifier can be translated to the target natural language.
    /// </summary>
    public bool IsTranslatable { get; set; }

    /// <summary>
    /// Gets or sets the translated name of this identifier in the target natural language.
    /// </summary>
    public string TranslatedName { get; set; } = "";

    /// <summary>
    /// Creates a deep copy of this identifier node and its children.
    /// </summary>
    public override ASTNode Clone()
    {
        IdentifierNode clone = new IdentifierNode
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
