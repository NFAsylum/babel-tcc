using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class CSharpAdapterTests
{
    public CSharpAdapter _adapter = new();

    [Fact]
    public void Properties_AreCorrect()
    {
        Assert.Equal("CSharp", _adapter.LanguageName);
        Assert.Equal(new[] { ".cs" }, _adapter.FileExtensions);
        Assert.Equal("1.0.0", _adapter.Version);
    }

    [Fact]
    public void Parse_HelloWorld_ExtractsKeywordsAndIdentifiers()
    {
        string code = @"
using System;

namespace HelloWorld
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";

        ASTNode ast = _adapter.Parse(code);

        Assert.IsType<StatementNode>(ast);

        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        // Should find keywords: using, namespace, class, static, void
        Assert.Contains(keywords, k => k.OriginalKeyword == "using" && k.KeywordId == 72);
        Assert.Contains(keywords, k => k.OriginalKeyword == "namespace" && k.KeywordId == 39);
        Assert.Contains(keywords, k => k.OriginalKeyword == "class" && k.KeywordId == 10);
        Assert.Contains(keywords, k => k.OriginalKeyword == "static" && k.KeywordId == 58);
        Assert.Contains(keywords, k => k.OriginalKeyword == "void" && k.KeywordId == 75);

        // Should find identifiers: System, HelloWorld, Program, Main, Console, WriteLine
        Assert.Contains(identifiers, i => i.Name == "System");
        Assert.Contains(identifiers, i => i.Name == "HelloWorld");
        Assert.Contains(identifiers, i => i.Name == "Program");
        Assert.Contains(identifiers, i => i.Name == "Main");
        Assert.Contains(identifiers, i => i.Name == "Console");
        Assert.Contains(identifiers, i => i.Name == "WriteLine");
    }

    [Fact]
    public void Parse_IfElse_ExtractsConditionalKeywords()
    {
        string code = @"
if (x > 0)
{
    return true;
}
else
{
    return false;
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "if" && k.KeywordId == 30);
        Assert.Contains(keywords, k => k.OriginalKeyword == "else" && k.KeywordId == 18);
        Assert.Contains(keywords, k => k.OriginalKeyword == "return" && k.KeywordId == 52);
        Assert.Contains(keywords, k => k.OriginalKeyword == "true" && k.KeywordId == 64);
        Assert.Contains(keywords, k => k.OriginalKeyword == "false" && k.KeywordId == 23);
    }

    [Fact]
    public void Parse_ForLoop_ExtractsLoopKeywords()
    {
        string code = "for (int i = 0; i < 10; i++) { break; continue; }";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "for" && k.KeywordId == 27);
        Assert.Contains(keywords, k => k.OriginalKeyword == "int" && k.KeywordId == 33);
        Assert.Contains(keywords, k => k.OriginalKeyword == "break" && k.KeywordId == 4);
        Assert.Contains(keywords, k => k.OriginalKeyword == "continue" && k.KeywordId == 12);
    }

    [Fact]
    public void Parse_ClassDeclaration_ExtractsAccessModifiers()
    {
        string code = @"
public class Calculator
{
    private int result;
    protected readonly double factor;

    public static void Reset() { }
    internal virtual int GetResult() { return 0; }
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "public" && k.KeywordId == 49);
        Assert.Contains(keywords, k => k.OriginalKeyword == "private" && k.KeywordId == 47);
        Assert.Contains(keywords, k => k.OriginalKeyword == "protected" && k.KeywordId == 48);
        Assert.Contains(keywords, k => k.OriginalKeyword == "readonly" && k.KeywordId == 50);
        Assert.Contains(keywords, k => k.OriginalKeyword == "static" && k.KeywordId == 58);
        Assert.Contains(keywords, k => k.OriginalKeyword == "internal" && k.KeywordId == 35);
        Assert.Contains(keywords, k => k.OriginalKeyword == "virtual" && k.KeywordId == 73);
    }

    [Fact]
    public void Parse_PreservesPositions()
    {
        string code = "public class Program { }";
        //          0123456789...

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        KeywordNode publicKw = keywords.First(k => k.OriginalKeyword == "public");
        Assert.Equal(0, publicKw.StartPosition);
        Assert.Equal(6, publicKw.EndPosition);
        Assert.Equal(0, publicKw.StartLine);

        KeywordNode classKw = keywords.First(k => k.OriginalKeyword == "class");
        Assert.Equal(7, classKw.StartPosition);
        Assert.Equal(12, classKw.EndPosition);
    }

    [Fact]
    public void Parse_PreservesLineNumbers()
    {
        string code = "using System;\nnamespace Test\n{\n    class Foo { }\n}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        KeywordNode usingKw = keywords.First(k => k.OriginalKeyword == "using");
        Assert.Equal(0, usingKw.StartLine);

        KeywordNode namespaceKw = keywords.First(k => k.OriginalKeyword == "namespace");
        Assert.Equal(1, namespaceKw.StartLine);

        KeywordNode classKw = keywords.First(k => k.OriginalKeyword == "class");
        Assert.Equal(3, classKw.StartLine);
    }

    [Fact]
    public void Parse_ExtractsLiterals()
    {
        string code = @"string s = ""hello""; int n = 42; char c = 'a';";

        ASTNode ast = _adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        Assert.Contains(literals, l => l.Type == LiteralType.String && (string?)l.Value == "hello");
        Assert.Contains(literals, l => l.Type == LiteralType.Number && Convert.ToInt32(l.Value) == 42);
        Assert.Contains(literals, l => l.Type == LiteralType.Char && (char?)l.Value == 'a');
    }

    [Fact]
    public void Parse_StringLiterals_AreTranslatable()
    {
        string code = @"string msg = ""Hello World"";";

        ASTNode ast = _adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        LiteralNode stringLiteral = literals.First(l => l.Type == LiteralType.String);
        Assert.True(stringLiteral.IsTranslatable);
    }

    [Fact]
    public void Parse_NumericLiterals_AreNotTranslatable()
    {
        string code = "int x = 42;";

        ASTNode ast = _adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        LiteralNode numericLiteral = literals.First(l => l.Type == LiteralType.Number);
        Assert.False(numericLiteral.IsTranslatable);
    }

    [Fact]
    public void Parse_TryCatch_ExtractsExceptionKeywords()
    {
        string code = @"
try { throw new Exception(); }
catch (Exception ex) { }
finally { }";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "try" && k.KeywordId == 65);
        Assert.Contains(keywords, k => k.OriginalKeyword == "throw" && k.KeywordId == 63);
        Assert.Contains(keywords, k => k.OriginalKeyword == "new" && k.KeywordId == 40);
        Assert.Contains(keywords, k => k.OriginalKeyword == "catch" && k.KeywordId == 7);
        Assert.Contains(keywords, k => k.OriginalKeyword == "finally" && k.KeywordId == 24);
    }

    [Fact]
    public void Generate_WithoutChanges_PreservesOriginalCode()
    {
        string code = "public class Program { }";

        ASTNode ast = _adapter.Parse(code);
        string result = _adapter.Generate(ast);

        Assert.Equal(code, result);
    }

    [Fact]
    public void Generate_WithTranslatedKeywords_ReplacesInCode()
    {
        string code = "public class Program { }";

        ASTNode ast = _adapter.Parse(code);

        // Translate keywords
        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw)
            {
                kw.OriginalKeyword = kw.KeywordId switch
                {
                    49 => "publico",  // public
                    10 => "classe",   // class
                    _ => kw.OriginalKeyword
                };
            }
        }

        string result = _adapter.Generate(ast);

        Assert.Equal("publico classe Program { }", result);
    }

    [Fact]
    public void Generate_WithTranslatedIdentifiers_ReplacesInCode()
    {
        string code = "class Calculator { }";

        ASTNode ast = _adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is IdentifierNode id && id.Name == "Calculator")
            {
                id.Name = "Calculadora";
            }
        }

        string result = _adapter.Generate(ast);

        Assert.Equal("class Calculadora { }", result);
    }

    [Fact]
    public void Generate_MultiLine_PreservesStructure()
    {
        string code = "public class Program\n{\n    static void Main()\n    {\n    }\n}";

        ASTNode ast = _adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw && kw.OriginalKeyword == "public")
            {
                kw.OriginalKeyword = "publico";
            }
        }

        string result = _adapter.Generate(ast);

        Assert.StartsWith("publico", result);
        Assert.Contains("class Program", result);
        Assert.Contains("static void Main()", result);
    }

    [Fact]
    public void GetKeywordMap_ReturnsAllCSharpKeywords()
    {
        Dictionary<string, int> map = _adapter.GetKeywordMap();

        Assert.True(map.Count >= 76);
        Assert.Equal(30, map["if"]);
        Assert.Equal(18, map["else"]);
        Assert.Equal(10, map["class"]);
        Assert.Equal(75, map["void"]);
        Assert.Equal(49, map["public"]);
    }

    [Fact]
    public void ValidateSyntax_ValidCode_ReturnsValid()
    {
        string code = "public class Program { static void Main() { } }";

        ValidationResult result = _adapter.ValidateSyntax(code);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void ValidateSyntax_InvalidCode_ReturnsErrors()
    {
        string code = "public class { }"; // missing class name

        ValidationResult result = _adapter.ValidateSyntax(code);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void ExtractIdentifiers_ReturnsUniqueIdentifiers()
    {
        string code = @"
class Calculator
{
    int Add(int a, int b) { return a + b; }
    int result = Add(1, 2);
}";

        List<string> identifiers = _adapter.ExtractIdentifiers(code);

        Assert.Contains("Calculator", identifiers);
        Assert.Contains("Add", identifiers);
        Assert.Contains("a", identifiers);
        Assert.Contains("b", identifiers);
        Assert.Contains("result", identifiers);
        // "Add" should appear only once (distinct)
        Assert.Equal(1, identifiers.Count(i => i == "Add"));
    }

    [Fact]
    public void ExtractIdentifiers_EmptyCode_ReturnsEmpty()
    {
        List<string> identifiers = _adapter.ExtractIdentifiers("");

        Assert.Empty(identifiers);
    }

    [Fact]
    public void Parse_AllIdentifiers_AreTranslatable()
    {
        string code = "int myVariable = 10;";

        ASTNode ast = _adapter.Parse(code);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.All(identifiers, id => Assert.True(id.IsTranslatable));
    }

    public static List<T> GetNodesOfType<T>(ASTNode root) where T : ASTNode
    {
        List<T> result = new List<T>();
        CollectNodes(root, result);
        return result;
    }

    public static void CollectNodes<T>(ASTNode node, List<T> result) where T : ASTNode
    {
        if (node is T typed)
        {
            result.Add(typed);
        }

        foreach (ASTNode child in node.Children)
        {
            CollectNodes(child, result);
        }
    }
}
