using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MultiLingualCode.Core.LanguageAdapters;

public class RoslynWrapper
{
    public static SyntaxTree ParseSourceCode(string source)
    {
        return CSharpSyntaxTree.ParseText(source);
    }

    public static SyntaxNode GetRoot(SyntaxTree tree)
    {
        return tree.GetRoot();
    }

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

    public static bool IsIdentifierToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.IdentifierToken);
    }

    public static bool IsStringLiteralToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.StringLiteralToken);
    }

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
}

public enum LiteralTokenKind
{
    Numeric,
    String,
    Character,
    Other
}
