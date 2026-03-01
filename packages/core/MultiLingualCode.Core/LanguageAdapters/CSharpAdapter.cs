using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Language adapter for C# that uses Roslyn to parse and generate translated source code.
/// </summary>
public class CSharpAdapter : ILanguageAdapter
{
    /// <summary>
    /// The name of the programming language handled by this adapter.
    /// </summary>
    public string LanguageName => "CSharp";

    /// <summary>
    /// The file extensions associated with C# source files.
    /// </summary>
    public string[] FileExtensions => [".cs"];

    /// <summary>
    /// The version of this adapter implementation.
    /// </summary>
    public string Version => "1.0.0";

    /// <summary>
    /// Parses C# source code into an AST with keyword, identifier, and literal nodes.
    /// </summary>
    /// <param name="sourceCode">The C# source code to parse.</param>
    /// <returns>The root AST node representing the parsed compilation unit.</returns>
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
            else if (RoslynWrapper.IsContextualKeywordToken(token))
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
                    Value = RoslynWrapper.GetLiteralValue(token),
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

    /// <summary>
    /// Generates source code from an AST by applying text replacements to the original source.
    /// </summary>
    /// <param name="ast">The root AST node containing the translated tokens.</param>
    /// <returns>The regenerated source code with all replacements applied.</returns>
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

    /// <summary>
    /// Returns a copy of the C# keyword-to-ID mapping dictionary.
    /// </summary>
    /// <returns>A dictionary mapping keyword text to integer IDs.</returns>
    public Dictionary<string, int> GetKeywordMap() => CSharpKeywordMap.GetMap();

    /// <summary>
    /// Replaces translated keywords back with their original C# keyword text.
    /// </summary>
    /// <param name="translatedCode">The source code containing translated keywords.</param>
    /// <param name="lookupTranslatedKeyword">Function that returns a keyword ID for a translated keyword text, or -1 if not found.</param>
    /// <returns>The source code with original C# keywords restored.</returns>
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword)
    {
        SyntaxTree tree = RoslynWrapper.ParseSourceCode(translatedCode);
        SyntaxNode root = RoslynWrapper.GetRoot(tree);

        List<(int Start, int End, string Text)> replacements = new();

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

    /// <summary>
    /// Validates C# source code syntax and returns any error diagnostics.
    /// </summary>
    /// <param name="sourceCode">The C# source code to validate.</param>
    /// <returns>A validation result indicating whether the syntax is valid and any diagnostics found.</returns>
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

    /// <summary>
    /// Extracts all distinct identifier names from C# source code.
    /// </summary>
    /// <param name="sourceCode">The C# source code to extract identifiers from.</param>
    /// <returns>A list of unique identifier names found in the source code.</returns>
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

    /// <summary>
    /// Recursively collects text replacements from the AST for code generation.
    /// </summary>
    /// <param name="node">The current AST node to process.</param>
    /// <param name="replacements">The list to accumulate replacement tuples (start, end, new text) into.</param>
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

    /// <summary>
    /// Converts a Roslyn-based <see cref="LiteralTokenKind"/> to the AST model's <see cref="LiteralType"/>.
    /// </summary>
    /// <param name="kind">The Roslyn literal token kind.</param>
    /// <returns>The corresponding AST literal type.</returns>
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
