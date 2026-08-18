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
    /// The value as it was when the literal was parsed, before any translation.
    /// Comparing <see cref="Value"/> against this tells the generator whether the literal was
    /// actually changed. Without it the generator has to compare the decoded value against the raw
    /// source text, which never matches when the literal contains escape sequences, so an untouched
    /// literal gets rebuilt from its decoded value and the escapes are lost.
    /// </summary>
    public object OriginalValue { get; set; } = "";

    /// <summary>
    /// Creates a deep copy of this literal node and its children.
    /// </summary>
    public override ASTNode Clone()
    {
        LiteralNode clone = new LiteralNode
        {
            Value = Value,
            OriginalValue = OriginalValue,
            Type = Type,
            IsTranslatable = IsTranslatable
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
