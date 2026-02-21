namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Generic container for statement nodes in the AST.
/// Used to group declarations, blocks, and control flow statements.
/// </summary>
public class StatementNode : ASTNode
{
    /// <summary>
    /// The kind of statement (e.g. "IfStatement", "ForLoop", "ClassDeclaration", "Block").
    /// </summary>
    public string StatementKind { get; set; } = string.Empty;

    /// <summary>
    /// Raw source text of this statement (preserved for round-trip fidelity).
    /// </summary>
    public string RawText { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override ASTNode Clone()
    {
        var clone = new StatementNode
        {
            StatementKind = StatementKind,
            RawText = RawText
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
