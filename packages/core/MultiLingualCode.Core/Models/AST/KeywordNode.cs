namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Represents a programming language keyword (if, class, void, etc.) in the AST.
/// </summary>
public class KeywordNode : ASTNode
{
    /// <summary>
    /// Numeric ID of the keyword (maps to translation tables).
    /// </summary>
    public int KeywordId { get; set; }

    /// <summary>
    /// Original keyword text as it appears in the source language (e.g. "if", "class").
    /// </summary>
    public string OriginalKeyword { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override ASTNode Clone()
    {
        var clone = new KeywordNode
        {
            KeywordId = KeywordId,
            OriginalKeyword = OriginalKeyword
        };
        CopyBaseTo(clone);
        clone.Children = CloneChildren(Children, clone);
        return clone;
    }
}
