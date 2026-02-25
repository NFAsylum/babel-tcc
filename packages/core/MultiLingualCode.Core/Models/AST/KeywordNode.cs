namespace MultiLingualCode.Core.Models.AST;

public class KeywordNode : ASTNode
{
    public int KeywordId { get; set; }
    public string Text { get; set; } = string.Empty;

    public override ASTNode Clone()
    {
        KeywordNode clone = new KeywordNode
        {
            KeywordId = KeywordId,
            Text = Text
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
