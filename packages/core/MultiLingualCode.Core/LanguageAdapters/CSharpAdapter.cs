using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.LanguageAdapters;

public class CSharpAdapter : ILanguageAdapter
{
    public string LanguageName => "CSharp";
    public string[] FileExtensions => [".cs"];
    public string Version => "1.0.0";

    public ASTNode Parse(string sourceCode)
    {
        SyntaxTree tree = RoslynWrapper.ParseSourceCode(sourceCode);
        SyntaxNode root = RoslynWrapper.GetRoot(tree);

        StatementNode compilationUnit = new StatementNode
        {
            StatementKind = "CompilationUnit",
            RawText = sourceCode,
            StartPosition = 0,
            EndPosition = sourceCode.Length,
            StartLine = 0,
            EndLine = sourceCode.Split('\n').Length - 1
        };

        foreach (SyntaxToken token in root.DescendantTokens())
        {
            TextSpan span = token.Span;
            FileLinePositionSpan lineSpan = token.GetLocation().GetLineSpan();

            if (CSharpKeywordMap.IsKeyword(token.Kind()))
            {
                int keywordId = CSharpKeywordMap.GetId(token.Text);
                if (keywordId >= 0)
                {
                    compilationUnit.Children.Add(new KeywordNode
                    {
                        KeywordId = keywordId,
                        Text = token.Text,
                        StartPosition = span.Start,
                        EndPosition = span.End,
                        StartLine = lineSpan.StartLinePosition.Line,
                        EndLine = lineSpan.EndLinePosition.Line,
                        Parent = compilationUnit
                    });
                }
            }
            else if (RoslynWrapper.IsIdentifierToken(token))
            {
                compilationUnit.Children.Add(new IdentifierNode
                {
                    Name = token.Text,
                    IsTranslatable = true,
                    StartPosition = span.Start,
                    EndPosition = span.End,
                    StartLine = lineSpan.StartLinePosition.Line,
                    EndLine = lineSpan.EndLinePosition.Line,
                    Parent = compilationUnit
                });
            }
            else if (RoslynWrapper.IsLiteralToken(token))
            {
                compilationUnit.Children.Add(new LiteralNode
                {
                    Value = token.Value ?? "",
                    Type = ConvertLiteralKind(RoslynWrapper.GetLiteralKind(token)),
                    IsTranslatable = RoslynWrapper.IsStringLiteralToken(token),
                    StartPosition = span.Start,
                    EndPosition = span.End,
                    StartLine = lineSpan.StartLinePosition.Line,
                    EndLine = lineSpan.EndLinePosition.Line,
                    Parent = compilationUnit
                });
            }
        }

        return compilationUnit;
    }

    public string Generate(ASTNode ast)
    {
        StatementNode root = (StatementNode)ast;
        string originalSource = root.RawText;

        if (string.IsNullOrEmpty(originalSource) || ast.Children.Count == 0)
        {
            return originalSource;
        }

        List<(int Start, int End, string NewText)> replacements = new List<(int, int, string)>();

        CollectReplacements(ast, replacements);

        if (replacements.Count == 0)
        {
            return originalSource;
        }

        replacements.Sort((a, b) => b.Start.CompareTo(a.Start));

        string result = originalSource;
        foreach ((int start, int end, string newText) in replacements)
        {
            if (start >= 0 && end <= result.Length && start < end)
            {
                result = string.Concat(result.AsSpan(0, start), newText, result.AsSpan(end));
            }
        }

        return result;
    }

    public Dictionary<string, int> GetKeywordMap() => CSharpKeywordMap.GetMap();

    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword)
    {
        SyntaxTree tree = RoslynWrapper.ParseSourceCode(translatedCode);
        SyntaxNode root = RoslynWrapper.GetRoot(tree);

        List<(int Start, int End, string OriginalKeyword)> replacements = new();

        foreach (SyntaxToken token in root.DescendantTokens())
        {
            if (!token.IsKind(SyntaxKind.IdentifierToken))
            {
                continue;
            }

            int keywordId = lookupTranslatedKeyword(token.Text);
            if (keywordId < 0)
            {
                continue;
            }

            string originalKeyword = CSharpKeywordMap.GetText(keywordId);
            if (!string.IsNullOrEmpty(originalKeyword))
            {
                replacements.Add((token.Span.Start, token.Span.End, originalKeyword));
            }
        }

        if (replacements.Count == 0)
        {
            return translatedCode;
        }

        replacements.Sort((a, b) => b.Start.CompareTo(a.Start));

        string result = translatedCode;
        foreach ((int start, int end, string originalKeyword) in replacements)
        {
            result = string.Concat(result.AsSpan(0, start), originalKeyword, result.AsSpan(end));
        }

        return result;
    }

    public ValidationResult ValidateSyntax(string sourceCode)
    {
        SyntaxTree tree = RoslynWrapper.ParseSourceCode(sourceCode);
        List<Models.Diagnostic> diagnosticsList = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .Select(d =>
            {
                FileLinePositionSpan lineSpan = d.Location.GetLineSpan();
                return new Models.Diagnostic
                {
                    Severity = Models.DiagnosticSeverity.Error,
                    Message = d.GetMessage(),
                    Line = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character
                };
            })
            .ToList();

        return new ValidationResult
        {
            IsValid = diagnosticsList.Count == 0,
            Diagnostics = diagnosticsList
        };
    }

    public List<string> ExtractIdentifiers(string sourceCode)
    {
        SyntaxTree tree = RoslynWrapper.ParseSourceCode(sourceCode);
        SyntaxNode root = RoslynWrapper.GetRoot(tree);

        return root.DescendantTokens()
            .Where(t => RoslynWrapper.IsIdentifierToken(t))
            .Select(t => t.Text)
            .Distinct()
            .ToList();
    }

    public static void CollectReplacements(ASTNode node, List<(int Start, int End, string NewText)> replacements)
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
                string quotedValue = "\"" + literal.Value + "\"";
                replacements.Add((literal.StartPosition, literal.EndPosition, quotedValue));
                break;
        }

        foreach (ASTNode child in node.Children)
        {
            CollectReplacements(child, replacements);
        }
    }

    public static LiteralType ConvertLiteralKind(LiteralTokenKind kind)
    {
        return kind switch
        {
            LiteralTokenKind.Numeric => LiteralType.Number,
            LiteralTokenKind.String => LiteralType.String,
            LiteralTokenKind.Character => LiteralType.Char,
            _ => LiteralType.Other
        };
    }
}
