using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Shared helper methods for language adapters.
/// </summary>
public static class AdapterHelpers
{
    /// <summary>
    /// Recursively collects text replacements from the AST for code generation.
    /// Keywords and identifiers are replaced with their current Text/Name.
    /// String literals are only replaced if their value was modified (translated).
    /// The quoteWrapper function determines how to reconstruct the literal with quotes.
    /// </summary>
    /// <param name="node">The current AST node to process.</param>
    /// <param name="replacements">The list to accumulate replacement tuples into.</param>
    /// <param name="originalSource">The original source code (used to detect if literal was modified).</param>
    /// <param name="quoteWrapper">Function that wraps a literal value with appropriate quotes for the language.</param>
    public static void CollectReplacements(
        ASTNode node,
        List<(int Start, int End, string NewText)> replacements,
        string originalSource,
        Func<LiteralNode, string, string> quoteWrapper)
    {
        switch (node)
        {
            case KeywordNode keyword:
                replacements.Add((keyword.StartPosition, keyword.EndPosition, keyword.Text));
                break;
            case IdentifierNode identifier:
                replacements.Add((identifier.StartPosition, identifier.EndPosition, identifier.Name));
                break;
            case LiteralNode literal when literal.Type == LiteralType.String:
                string originalText = "";
                if (literal.StartPosition >= 0 && literal.EndPosition <= originalSource.Length)
                {
                    originalText = originalSource.Substring(literal.StartPosition, literal.EndPosition - literal.StartPosition);
                }
                string wrappedValue = quoteWrapper(literal, originalText);
                replacements.Add((literal.StartPosition, literal.EndPosition, wrappedValue));
                break;
        }

        foreach (ASTNode child in node.Children)
        {
            CollectReplacements(child, replacements, originalSource, quoteWrapper);
        }
    }
}
