namespace MultiLingualCode.Core.Models.AST;

public class StatementNode : ASTNode
{
    public string StatementKind { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;

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
