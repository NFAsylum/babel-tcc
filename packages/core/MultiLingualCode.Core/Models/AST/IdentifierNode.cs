namespace MultiLingualCode.Core.Models.AST;

public class IdentifierNode : ASTNode
{
    public string Name { get; set; } = string.Empty;
    public bool IsTranslatable { get; set; }
    public string TranslatedName { get; set; } = "";

    public override ASTNode Clone()
    {
        IdentifierNode clone = new IdentifierNode
        {
            Name = Name,
            IsTranslatable = IsTranslatable,
            TranslatedName = TranslatedName
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
