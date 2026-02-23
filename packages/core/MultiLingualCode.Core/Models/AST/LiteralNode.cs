namespace MultiLingualCode.Core.Models.AST;

public class LiteralNode : ASTNode
{
    public object Value { get; set; } = "";
    public LiteralType Type { get; set; }
    public bool IsTranslatable { get; set; }

    public override ASTNode Clone()
    {
        LiteralNode clone = new LiteralNode
        {
            Value = Value,
            Type = Type,
            IsTranslatable = IsTranslatable
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
