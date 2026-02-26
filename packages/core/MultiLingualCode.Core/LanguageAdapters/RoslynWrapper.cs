using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Thin wrapper around Roslyn C# parser APIs, providing simplified access to syntax tree operations.
/// </summary>
public class RoslynWrapper
{
    /// <summary>
    /// Parses C# source code into a Roslyn syntax tree.
    /// </summary>
    /// <param name="source">The C# source code to parse.</param>
    /// <returns>The parsed syntax tree.</returns>
    public static SyntaxTree ParseSourceCode(string source)
    {
        return CSharpSyntaxTree.ParseText(source);
    }

    /// <summary>
    /// Gets the root syntax node from a syntax tree.
    /// </summary>
    /// <param name="tree">The syntax tree to get the root from.</param>
    /// <returns>The root syntax node.</returns>
    public static SyntaxNode GetRoot(SyntaxTree tree)
    {
        return tree.GetRoot();
    }

    /// <summary>
    /// Determines whether the given syntax kind represents a C# keyword token.
    /// </summary>
    /// <param name="kind">The syntax kind to check.</param>
    /// <returns>True if the syntax kind is a C# keyword; otherwise false.</returns>
    public static bool IsKeywordKind(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.AbstractKeyword or SyntaxKind.AsKeyword or SyntaxKind.BaseKeyword or
            SyntaxKind.BoolKeyword or SyntaxKind.BreakKeyword or SyntaxKind.ByteKeyword or
            SyntaxKind.CaseKeyword or SyntaxKind.CatchKeyword or SyntaxKind.CharKeyword or
            SyntaxKind.CheckedKeyword or SyntaxKind.ClassKeyword or SyntaxKind.ConstKeyword or
            SyntaxKind.ContinueKeyword or SyntaxKind.DecimalKeyword or SyntaxKind.DefaultKeyword or
            SyntaxKind.DelegateKeyword or SyntaxKind.DoKeyword or SyntaxKind.DoubleKeyword or
            SyntaxKind.ElseKeyword or SyntaxKind.EnumKeyword or SyntaxKind.EventKeyword or
            SyntaxKind.ExplicitKeyword or SyntaxKind.ExternKeyword or SyntaxKind.FalseKeyword or
            SyntaxKind.FinallyKeyword or SyntaxKind.FixedKeyword or SyntaxKind.FloatKeyword or
            SyntaxKind.ForKeyword or SyntaxKind.ForEachKeyword or SyntaxKind.GotoKeyword or
            SyntaxKind.IfKeyword or SyntaxKind.ImplicitKeyword or SyntaxKind.InKeyword or
            SyntaxKind.IntKeyword or SyntaxKind.InterfaceKeyword or SyntaxKind.InternalKeyword or
            SyntaxKind.IsKeyword or SyntaxKind.LockKeyword or SyntaxKind.LongKeyword or
            SyntaxKind.NamespaceKeyword or SyntaxKind.NewKeyword or SyntaxKind.NullKeyword or
            SyntaxKind.ObjectKeyword or SyntaxKind.OperatorKeyword or SyntaxKind.OutKeyword or
            SyntaxKind.OverrideKeyword or SyntaxKind.ParamsKeyword or SyntaxKind.PrivateKeyword or
            SyntaxKind.ProtectedKeyword or SyntaxKind.PublicKeyword or SyntaxKind.ReadOnlyKeyword or
            SyntaxKind.RefKeyword or SyntaxKind.ReturnKeyword or SyntaxKind.SByteKeyword or
            SyntaxKind.SealedKeyword or SyntaxKind.ShortKeyword or SyntaxKind.SizeOfKeyword or
            SyntaxKind.StackAllocKeyword or SyntaxKind.StaticKeyword or SyntaxKind.StringKeyword or
            SyntaxKind.StructKeyword or SyntaxKind.SwitchKeyword or SyntaxKind.ThisKeyword or
            SyntaxKind.ThrowKeyword or SyntaxKind.TrueKeyword or SyntaxKind.TryKeyword or
            SyntaxKind.TypeOfKeyword or SyntaxKind.UIntKeyword or SyntaxKind.ULongKeyword or
            SyntaxKind.UncheckedKeyword or SyntaxKind.UnsafeKeyword or SyntaxKind.UShortKeyword or
            SyntaxKind.UsingKeyword or SyntaxKind.VirtualKeyword or SyntaxKind.VoidKeyword or
            SyntaxKind.VolatileKeyword or SyntaxKind.WhileKeyword => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the token is an identifier token.
    /// </summary>
    /// <param name="token">The syntax token to check.</param>
    /// <returns>True if the token is an identifier; otherwise false.</returns>
    public static bool IsIdentifierToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.IdentifierToken);
    }

    /// <summary>
    /// Determines whether the token is a string literal token.
    /// </summary>
    /// <param name="token">The syntax token to check.</param>
    /// <returns>True if the token is a string literal; otherwise false.</returns>
    public static bool IsStringLiteralToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.StringLiteralToken);
    }

    /// <summary>
    /// Determines whether the token is a literal token (numeric, string, or character).
    /// </summary>
    /// <param name="token">The syntax token to check.</param>
    /// <returns>True if the token is a numeric, string, or character literal; otherwise false.</returns>
    public static bool IsLiteralToken(SyntaxToken token)
    {
        return token.Kind() switch
        {
            SyntaxKind.NumericLiteralToken => true,
            SyntaxKind.StringLiteralToken => true,
            SyntaxKind.CharacterLiteralToken => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the literal kind classification for a syntax token.
    /// </summary>
    /// <param name="token">The syntax token to classify.</param>
    /// <returns>The literal token kind (Numeric, String, Character, or Other).</returns>
    public static LiteralTokenKind GetLiteralKind(SyntaxToken token)
    {
        return token.Kind() switch
        {
            SyntaxKind.NumericLiteralToken => LiteralTokenKind.Numeric,
            SyntaxKind.StringLiteralToken => LiteralTokenKind.String,
            SyntaxKind.CharacterLiteralToken => LiteralTokenKind.Character,
            _ => LiteralTokenKind.Other
        };
    }

    /// <summary>
    /// Extracts the runtime value of a literal token.
    /// </summary>
    /// <param name="token">The literal syntax token.</param>
    /// <returns>The literal value, or an empty string if no value is available.</returns>
    public static object GetLiteralValue(SyntaxToken token)
    {
        if (token.Value is object value)
        {
            return value;
        }
        return "";
    }

    /// <summary>
    /// Gets the text content of a single-line trailing comment (after "//"), if present.
    /// </summary>
    /// <param name="token">The syntax token to inspect for trailing comments.</param>
    /// <returns>The trimmed comment text without the "//" prefix, or an empty string if none found.</returns>
    public static string GetTrailingCommentText(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string commentText = trivia.ToString();
                if (commentText.StartsWith("//"))
                {
                    return commentText.Substring(2).TrimStart();
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Returns all syntax tokens located on the specified source line.
    /// </summary>
    /// <param name="root">The root syntax node to search within.</param>
    /// <param name="line">The zero-based line number to match.</param>
    /// <returns>A list of all tokens on the specified line.</returns>
    public static List<SyntaxToken> GetAllTokensOnLine(SyntaxNode root, int line)
    {
        List<SyntaxToken> tokensOnLine = new List<SyntaxToken>();
        foreach (SyntaxToken token in root.DescendantTokens())
        {
            FileLinePositionSpan lineSpan = token.GetLocation().GetLineSpan();
            if (lineSpan.StartLinePosition.Line == line)
            {
                tokensOnLine.Add(token);
            }
        }

        return tokensOnLine;
    }

    /// <summary>
    /// Returns all identifier tokens located on the specified source line.
    /// </summary>
    /// <param name="root">The root syntax node to search within.</param>
    /// <param name="line">The zero-based line number to match.</param>
    /// <returns>A list of identifier tokens on the specified line.</returns>
    public static List<SyntaxToken> GetIdentifierTokensOnLine(SyntaxNode root, int line)
    {
        List<SyntaxToken> identifiersOnLine = new List<SyntaxToken>();
        foreach (SyntaxToken token in root.DescendantTokens())
        {
            FileLinePositionSpan lineSpan = token.GetLocation().GetLineSpan();
            if (lineSpan.StartLinePosition.Line == line && IsIdentifierToken(token))
            {
                identifiersOnLine.Add(token);
            }
        }

        return identifiersOnLine;
    }
}
