namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a programming language keyword (e.g., "if", "class", "return") in the abstract syntax tree.
/// </summary>
public class KeywordNode : ASTNode
{
    /// <summary>
    /// Gets or sets the numeric identifier that uniquely identifies this keyword within its language adapter.
    /// </summary>
    public int KeywordId { get; set; }

    /// <summary>
    /// Gets or sets the textual representation of the keyword.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Creates a deep copy of this keyword node and its children.
    /// </summary>
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
