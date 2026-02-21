namespace MultiLingualCode.Core.Models.AST;

public abstract class ASTNode
{
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public ASTNode Parent { get; set; } = default!;
    public List<ASTNode> Children { get; set; } = new();

    public abstract ASTNode Clone();

    public void CopyBaseTo(ASTNode target)
    {
        target.StartPosition = StartPosition;
        target.EndPosition = EndPosition;
        target.StartLine = StartLine;
        target.EndLine = EndLine;
    }

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
