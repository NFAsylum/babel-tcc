namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Abstract base class for all nodes in the abstract syntax tree, providing position tracking, parent-child relationships, and cloning support.
/// </summary>
public abstract class ASTNode
{
    /// <summary>
    /// Gets or sets the zero-based character offset where this node begins in the source code.
    /// </summary>
    public int StartPosition { get; set; }

    /// <summary>
    /// Gets or sets the zero-based character offset where this node ends in the source code.
    /// </summary>
    public int EndPosition { get; set; }

    /// <summary>
    /// Gets or sets the line number where this node begins.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Gets or sets the line number where this node ends.
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Gets or sets the parent node in the AST hierarchy.
    /// </summary>
    public ASTNode Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of child nodes belonging to this node.
    /// </summary>
    public List<ASTNode> Children { get; set; } = new();

    /// <summary>
    /// Creates a deep copy of this node and its children.
    /// </summary>
    public abstract ASTNode Clone();

    /// <summary>
    /// Copies the base position properties (start/end position and line) from this node to the target node.
    /// </summary>
    public void CopyBaseTo(ASTNode target)
    {
        target.StartPosition = StartPosition;
        target.EndPosition = EndPosition;
        target.StartLine = StartLine;
        target.EndLine = EndLine;
    }

    /// <summary>
    /// Clones a list of child nodes and assigns the specified node as their new parent.
    /// </summary>
    public static List<ASTNode> CloneChildren(List<ASTNode> children, ASTNode newParent)
    {
        List<ASTNode> cloned = new List<ASTNode>(children.Count);
        foreach (ASTNode child in children)
        {
            ASTNode clonedChild = child.Clone();
            clonedChild.Parent = newParent;
            cloned.Add(clonedChild);
        }
        return cloned;
    }
}
