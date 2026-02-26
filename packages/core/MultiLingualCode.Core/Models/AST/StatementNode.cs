namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a statement in the abstract syntax tree (e.g., if, for, return, variable declaration).
/// </summary>
public class StatementNode : ASTNode
{
    /// <summary>
    /// Gets or sets the kind of statement (e.g., "IfStatement", "ReturnStatement").
    /// </summary>
    public string StatementKind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw source text of this statement.
    /// </summary>
    public string RawText { get; set; } = string.Empty;

    /// <summary>
    /// Creates a deep copy of this statement node and its children.
    /// </summary>
    public override ASTNode Clone()
    {
        StatementNode clone = new StatementNode
        {
            StatementKind = StatementKind,
            RawText = RawText
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
