namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents an expression in the abstract syntax tree (e.g., arithmetic, method call, assignment).
/// </summary>
public class ExpressionNode : ASTNode
{
    /// <summary>
    /// Gets or sets the kind of expression (e.g., "BinaryExpression", "InvocationExpression").
    /// </summary>
    public string ExpressionKind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw source text of this expression.
    /// </summary>
    public string RawText { get; set; } = string.Empty;

    /// <summary>
    /// Creates a deep copy of this expression node and its children.
    /// </summary>
    public override ASTNode Clone()
    {
        ExpressionNode clone = new ExpressionNode
        {
            ExpressionKind = ExpressionKind,
            RawText = RawText
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
