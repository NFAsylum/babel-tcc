namespace MultiLingualCode.Core.Models.AST;

public class KeywordNode : ASTNode
{
    public int KeywordId { get; set; }
    public string OriginalKeyword { get; set; } = string.Empty;

    public override ASTNode Clone()
    {
        KeywordNode clone = new KeywordNode
        {
            KeywordId = KeywordId,
            OriginalKeyword = OriginalKeyword
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
