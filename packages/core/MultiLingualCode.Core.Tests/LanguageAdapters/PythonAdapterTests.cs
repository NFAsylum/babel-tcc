using MultiLingualCode.Core.LanguageAdapters.Python;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class PythonAdapterTests
{
    public PythonAdapter Adapter = new PythonAdapter();

    [Fact]
    public void Properties_AreCorrect()
    {
        Assert.Equal("Python", Adapter.LanguageName);
        Assert.Equal(new[] { ".py" }, Adapter.FileExtensions);
        Assert.Equal("1.0.0", Adapter.Version);
    }

    [Fact]
    public void Parse_SimpleFunction_ExtractsKeywords()
    {
        ASTNode ast = Adapter.Parse("def foo(x):\n    return x + 1");

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "def");
        Assert.Contains(keywords, k => k.Text == "return");
    }

    [Fact]
    public void Parse_ClassDeclaration_ExtractsKeywords()
    {
        string code = "class MyClass:\n    def __init__(self):\n        pass";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "class");
        Assert.Contains(keywords, k => k.Text == "def");
        Assert.Contains(keywords, k => k.Text == "pass");
    }

    [Fact]
    public void Parse_IfElse_ExtractsConditionalKeywords()
    {
        string code = "if x:\n    pass\nelif y:\n    pass\nelse:\n    pass";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "if");
        Assert.Contains(keywords, k => k.Text == "elif");
        Assert.Contains(keywords, k => k.Text == "else");
    }

    [Fact]
    public void Parse_ForLoop_ExtractsLoopKeywords()
    {
        string code = "for i in range(10):\n    if i == 5:\n        break\n    continue";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "for");
        Assert.Contains(keywords, k => k.Text == "in");
        Assert.Contains(keywords, k => k.Text == "break");
        Assert.Contains(keywords, k => k.Text == "continue");
    }

    [Fact]
    public void Parse_TryExcept_ExtractsExceptionKeywords()
    {
        string code = "try:\n    pass\nexcept Exception:\n    raise\nfinally:\n    pass";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "try");
        Assert.Contains(keywords, k => k.Text == "except");
        Assert.Contains(keywords, k => k.Text == "raise");
        Assert.Contains(keywords, k => k.Text == "finally");
    }

    [Fact]
    public void Parse_AsyncAwait_ExtractsKeywords()
    {
        string code = "async def fetch():\n    await get()";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "async");
        Assert.Contains(keywords, k => k.Text == "def");
        Assert.Contains(keywords, k => k.Text == "await");
    }

    [Fact]
    public void Parse_Import_ExtractsKeywords()
    {
        string code = "from os import path as p";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "from");
        Assert.Contains(keywords, k => k.Text == "import");
        Assert.Contains(keywords, k => k.Text == "as");
    }

    [Fact]
    public void Parse_BooleanOperators_ExtractsKeywords()
    {
        string code = "x = True and False or not None";
        ASTNode ast = Adapter.Parse(code);

        List<KeywordNode> keywords = GetKeywordNodes(ast);
        Assert.Contains(keywords, k => k.Text == "True");
        Assert.Contains(keywords, k => k.Text == "and");
        Assert.Contains(keywords, k => k.Text == "False");
        Assert.Contains(keywords, k => k.Text == "or");
        Assert.Contains(keywords, k => k.Text == "not");
        Assert.Contains(keywords, k => k.Text == "None");
    }

    [Fact]
    public void Parse_ExtractsIdentifiers()
    {
        string code = "def foo(x):\n    return x";
        ASTNode ast = Adapter.Parse(code);

        List<IdentifierNode> identifiers = GetIdentifierNodes(ast);
        Assert.Contains(identifiers, id => id.Name == "foo");
        Assert.Contains(identifiers, id => id.Name == "x");
    }

    [Fact]
    public void Parse_Identifiers_AreTranslatable()
    {
        string code = "x = 1";
        ASTNode ast = Adapter.Parse(code);

        List<IdentifierNode> identifiers = GetIdentifierNodes(ast);
        Assert.All(identifiers, id => Assert.True(id.IsTranslatable));
    }

    [Fact]
    public void Parse_ExtractsStringLiterals()
    {
        string code = "x = \"hello world\"";
        ASTNode ast = Adapter.Parse(code);

        List<LiteralNode> literals = GetLiteralNodes(ast);
        LiteralNode stringLit = literals.First(l => l.Type == LiteralType.String);
        Assert.Equal("hello world", stringLit.Value);
        Assert.True(stringLit.IsTranslatable);
    }

    [Fact]
    public void Parse_ExtractsNumberLiterals()
    {
        string code = "x = 42";
        ASTNode ast = Adapter.Parse(code);

        List<LiteralNode> literals = GetLiteralNodes(ast);
        LiteralNode numLit = literals.First(l => l.Type == LiteralType.Number);
        Assert.Equal("42", numLit.Value);
        Assert.False(numLit.IsTranslatable);
    }

    [Fact]
    public void Parse_PreservesPositions()
    {
        string code = "def foo():\n    pass";
        ASTNode ast = Adapter.Parse(code);

        KeywordNode defNode = GetKeywordNodes(ast).First(k => k.Text == "def");
        Assert.Equal(0, defNode.StartPosition);
        Assert.Equal(3, defNode.EndPosition);
        Assert.Equal(0, defNode.StartLine);
    }

    [Fact]
    public void Parse_PreservesLineNumbers_MultiLine()
    {
        string code = "x = 1\ny = 2";
        ASTNode ast = Adapter.Parse(code);

        List<IdentifierNode> identifiers = GetIdentifierNodes(ast);
        IdentifierNode x = identifiers.First(id => id.Name == "x");
        IdentifierNode y = identifiers.First(id => id.Name == "y");
        Assert.Equal(0, x.StartLine);
        Assert.Equal(1, y.StartLine);
    }

    [Fact]
    public void Generate_WithoutChanges_PreservesOriginalCode()
    {
        string code = "def foo():\n    return 42\n";
        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);
        Assert.Equal(code, result);
    }

    [Fact]
    public void Generate_WithTranslatedKeywords_ReplacesInCode()
    {
        string code = "def foo():\n    pass";
        ASTNode ast = Adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw && kw.Text == "def")
            {
                kw.Text = "definir";
            }
            if (child is KeywordNode kw2 && kw2.Text == "pass")
            {
                kw2.Text = "passar";
            }
        }

        string result = Adapter.Generate(ast);
        Assert.Contains("definir", result);
        Assert.Contains("passar", result);
        Assert.DoesNotContain("def ", result);
    }

    [Fact]
    public void Generate_WithTranslatedIdentifiers_ReplacesInCode()
    {
        string code = "x = 1";
        ASTNode ast = Adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is IdentifierNode id && id.Name == "x")
            {
                id.Name = "variavel";
            }
        }

        string result = Adapter.Generate(ast);
        Assert.Contains("variavel", result);
        Assert.DoesNotContain("x ", result);
    }

    [Fact]
    public void Generate_MultiLine_PreservesIndentation()
    {
        string code = "def foo():\n    x = 1\n    return x";
        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);

        string[] lines = result.Split('\n');
        Assert.StartsWith("def", lines[0]);
        Assert.StartsWith("    ", lines[1]);
        Assert.StartsWith("    ", lines[2]);
    }

    [Fact]
    public void GetKeywordMap_Returns35Keywords()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.Equal(35, map.Count);
    }

    [Fact]
    public void ReverseSubstituteKeywords_SkipsComments()
    {
        string code = "# comentario definir\ndefinir foo():\n    passar";
        Func<string, int> lookup = (string word) =>
        {
            if (word == "definir") return 11;
            if (word == "passar") return 28;
            return -1;
        };

        string result = Adapter.ReverseSubstituteKeywords(code, lookup);
        Assert.StartsWith("# comentario definir", result);
        Assert.Contains("def foo():", result);
        Assert.Contains("pass", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_SkipsSingleQuoteStrings()
    {
        string code = "x = 'definir'";
        Func<string, int> lookup = (string word) => word == "definir" ? 11 : -1;

        string result = Adapter.ReverseSubstituteKeywords(code, lookup);
        Assert.Contains("'definir'", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_SkipsDoubleQuoteStrings()
    {
        string code = "x = \"definir\"";
        Func<string, int> lookup = (string word) => word == "definir" ? 11 : -1;

        string result = Adapter.ReverseSubstituteKeywords(code, lookup);
        Assert.Contains("\"definir\"", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_SkipsTripleQuoteStrings()
    {
        string code = "x = \"\"\"definir\"\"\"";
        Func<string, int> lookup = (string word) => word == "definir" ? 11 : -1;

        string result = Adapter.ReverseSubstituteKeywords(code, lookup);
        Assert.Contains("\"\"\"definir\"\"\"", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_SkipsFStrings()
    {
        string code = "x = f\"definir {y}\"";
        Func<string, int> lookup = (string word) => word == "definir" ? 11 : -1;

        string result = Adapter.ReverseSubstituteKeywords(code, lookup);
        Assert.Contains("f\"definir", result);
    }

    [Fact]
    public void ValidateSyntax_ValidCode_ReturnsValid()
    {
        ValidationResult result = Adapter.ValidateSyntax("def foo():\n    pass");
        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void ValidateSyntax_InvalidCode_ReturnsDiagnostics()
    {
        ValidationResult result = Adapter.ValidateSyntax("x = \"unterminated");
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void ExtractIdentifiers_ReturnsUserDefinedNames()
    {
        List<string> ids = Adapter.ExtractIdentifiers("def foo(x):\n    if x:\n        return True");
        Assert.Contains("foo", ids);
        Assert.Contains("x", ids);
        Assert.DoesNotContain("def", ids);
        Assert.DoesNotContain("if", ids);
        Assert.DoesNotContain("True", ids);
        Assert.DoesNotContain("return", ids);
    }

    [Fact]
    public void ExtractTrailingComments_ExtractsPythonComments()
    {
        string code = "x = 1 # tradu[pt-br]:variavel\ny = 2";
        List<TrailingComment> comments = Adapter.ExtractTrailingComments(code);
        Assert.Single(comments);
        Assert.Equal("tradu[pt-br]:variavel", comments[0].Text);
        Assert.Equal(0, comments[0].Line);
    }

    [Fact]
    public void GetIdentifierNamesOnLine_ReturnsIdentifiersOnSpecifiedLine()
    {
        string code = "def foo(x, y):\n    return x + y";
        List<string> ids = Adapter.GetIdentifierNamesOnLine(code, 0);
        Assert.Contains("foo", ids);
        Assert.Contains("x", ids);
        Assert.Contains("y", ids);
    }

    [Fact]
    public void GetFirstStringLiteralOnLine_ReturnsStringContent()
    {
        string code = "msg = \"hello\"";
        string literal = Adapter.GetFirstStringLiteralOnLine(code, 0);
        Assert.Equal("hello", literal);
    }

    [Fact]
    public void GetFirstStringLiteralOnLine_NoString_ReturnsEmpty()
    {
        string code = "x = 42";
        string literal = Adapter.GetFirstStringLiteralOnLine(code, 0);
        Assert.Equal("", literal);
    }

    [Fact]
    public void GetContainingMethodRange_FindsMethod()
    {
        string code = "def foo():\n    x = 1\n    return x\n\ndef bar():\n    pass";
        (int startLine, int endLine) = Adapter.GetContainingMethodRange(code, 1);
        Assert.Equal(0, startLine);
        Assert.True(endLine >= 2);
    }

    [Fact]
    public void GetContainingMethodRange_NoMethod_ReturnsNegative()
    {
        string code = "x = 1\ny = 2";
        (int startLine, int endLine) = Adapter.GetContainingMethodRange(code, 0);
        Assert.Equal(-1, startLine);
        Assert.Equal(-1, endLine);
    }

    [Fact]
    public void GetContainingMethodRange_IncludesDecorators()
    {
        string code = "@decorator\ndef foo():\n    pass";
        (int startLine, int endLine) = Adapter.GetContainingMethodRange(code, 2);
        Assert.Equal(0, startLine);
    }

    public static List<KeywordNode> GetKeywordNodes(ASTNode ast)
    {
        List<KeywordNode> nodes = new();
        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw)
            {
                nodes.Add(kw);
            }
        }
        return nodes;
    }

    public static List<IdentifierNode> GetIdentifierNodes(ASTNode ast)
    {
        List<IdentifierNode> nodes = new();
        foreach (ASTNode child in ast.Children)
        {
            if (child is IdentifierNode id)
            {
                nodes.Add(id);
            }
        }
        return nodes;
    }

    public static List<LiteralNode> GetLiteralNodes(ASTNode ast)
    {
        List<LiteralNode> nodes = new();
        foreach (ASTNode child in ast.Children)
        {
            if (child is LiteralNode lit)
            {
                nodes.Add(lit);
            }
        }
        return nodes;
    }
}
