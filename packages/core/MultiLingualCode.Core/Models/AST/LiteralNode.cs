namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a literal value (string, number, boolean, null) in the AST.
/// </summary>
public class LiteralNode : ASTNode
{
    /// <summary>
    /// The literal value. Type depends on <see cref="LiteralType"/>.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The type of this literal.
    /// </summary>
    public LiteralType Type { get; set; }

    /// <summary>
    /// Whether this literal has a translation available (e.g. translatable string).
    /// </summary>
    public bool IsTranslatable { get; set; }

    /// <inheritdoc/>
    public override ASTNode Clone()
    {
        var clone = new LiteralNode
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

/// <summary>
/// Types of literal values.
/// </summary>
public enum LiteralType
{
    String,
    Number,
    Boolean,
    Null
}
