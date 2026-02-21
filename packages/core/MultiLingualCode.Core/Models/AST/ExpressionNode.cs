namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Generic container for expression nodes in the AST.
/// Used to group sub-expressions (e.g. binary expressions, method calls).
/// </summary>
public class ExpressionNode : ASTNode
{
    /// <summary>
    /// The kind of expression (e.g. "BinaryExpression", "MethodCall", "Assignment").
    /// </summary>
    public string ExpressionKind { get; set; } = string.Empty;

    /// <summary>
    /// Raw source text of this expression (preserved for round-trip fidelity).
    /// </summary>
    public string RawText { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override ASTNode Clone()
    {
        var clone = new ExpressionNode
        {
            ExpressionKind = ExpressionKind,
            RawText = RawText
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
