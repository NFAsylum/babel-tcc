namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a literal value (e.g., string, number, boolean) in the abstract syntax tree.
/// </summary>
public class LiteralNode : ASTNode
{
    /// <summary>
    /// Gets or sets the actual value of the literal.
    /// </summary>
    public object Value { get; set; } = "";

    /// <summary>
    /// Gets or sets the type classification of this literal.
    /// </summary>
    public LiteralType Type { get; set; }

    /// <summary>
    /// Gets or sets whether this literal's value can be translated to the target natural language.
    /// </summary>
    public bool IsTranslatable { get; set; }

    /// <summary>
    /// Creates a deep copy of this literal node and its children.
    /// </summary>
    public override ASTNode Clone()
    {
        LiteralNode clone = new LiteralNode
        {
            Value = Value,
            Type = Type,
            IsTranslatable = IsTranslatable
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
