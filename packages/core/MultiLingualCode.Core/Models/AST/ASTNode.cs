namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Base class for all AST (Abstract Syntax Tree) nodes.
/// </summary>
public abstract class ASTNode
{
    /// <summary>
    /// Start position (character offset) in the source code.
    /// </summary>
    public int StartPosition { get; set; }

    /// <summary>
    /// End position (character offset) in the source code.
    /// </summary>
    public int EndPosition { get; set; }

    /// <summary>
    /// Start line number in the source code (0-based).
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// End line number in the source code (0-based).
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Parent node in the AST hierarchy. Null for the root node.
    /// </summary>
    public ASTNode? Parent { get; set; }

    /// <summary>
    /// Child nodes in the AST hierarchy.
    /// </summary>
    public List<ASTNode> Children { get; set; } = new();

    /// <summary>
    /// Creates a deep copy of this node and its children.
    /// </summary>
    public abstract ASTNode Clone();

    /// <summary>
    /// Copies base properties (positions, lines) to a target node.
    /// Does not copy Parent (set by the caller) or Children (cloned separately).
    /// </summary>
    protected void CopyBaseTo(ASTNode target)
    {
        target.StartPosition = StartPosition;
        target.EndPosition = EndPosition;
        target.StartLine = StartLine;
        target.EndLine = EndLine;
    }

    /// <summary>
    /// Deep-clones all children and sets their Parent to the given parent node.
    /// </summary>
    protected static List<ASTNode> CloneChildren(List<ASTNode> children, ASTNode newParent)
    {
        var cloned = new List<ASTNode>(children.Count);
        foreach (var child in children)
        {
            var clonedChild = child.Clone();
            clonedChild.Parent = newParent;
            cloned.Add(clonedChild);
        }
        return cloned;
    }
}
