namespace MultiLingualCode.Core.Models.AST;

public class ExpressionNode : ASTNode
{
    public string ExpressionKind { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;

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
