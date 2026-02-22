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

    [Fact]
    public void Parse_ClassWithProperties_ExtractsAllMembers()
    {
        string code = @"
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }

    public string GetFullName()
    {
        return FirstName + "" "" + LastName;
    }
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "class");
        Assert.Contains(keywords, k => k.OriginalKeyword == "public");
        Assert.Contains(keywords, k => k.OriginalKeyword == "string");
        Assert.Contains(keywords, k => k.OriginalKeyword == "int");
        Assert.Contains(keywords, k => k.OriginalKeyword == "return");
        Assert.Contains(identifiers, i => i.Name == "Person");
        Assert.Contains(identifiers, i => i.Name == "FirstName");
        Assert.Contains(identifiers, i => i.Name == "LastName");
        Assert.Contains(identifiers, i => i.Name == "Age");
        Assert.Contains(identifiers, i => i.Name == "GetFullName");
    }

    [Fact]
    public void Parse_StructDeclaration_ExtractsKeywordsAndFields()
    {
        string code = @"
public struct Point
{
    public int X;
    public int Y;
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "struct");
        Assert.Contains(identifiers, i => i.Name == "Point");
        Assert.Contains(identifiers, i => i.Name == "X");
        Assert.Contains(identifiers, i => i.Name == "Y");
    }

    [Fact]
    public void Parse_EnumDeclaration_ExtractsEnumMembers()
    {
        string code = @"
public enum Color
{
    Red,
    Green,
    Blue
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "enum");
        Assert.Contains(identifiers, i => i.Name == "Color");
        Assert.Contains(identifiers, i => i.Name == "Red");
        Assert.Contains(identifiers, i => i.Name == "Green");
        Assert.Contains(identifiers, i => i.Name == "Blue");
    }

    [Fact]
    public void Parse_GenericTypes_ExtractsTypeParameters()
    {
        string code = @"
public class Repository
{
    public List<string> items = new List<string>();
    public Dictionary<string, int> lookup = new Dictionary<string, int>();
}";

        ASTNode ast = _adapter.Parse(code);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(identifiers, i => i.Name == "Repository");
        Assert.Contains(identifiers, i => i.Name == "List");
        Assert.Contains(identifiers, i => i.Name == "Dictionary");
        Assert.Contains(identifiers, i => i.Name == "items");
        Assert.Contains(identifiers, i => i.Name == "lookup");
    }

    [Fact]
    public void Parse_NamespaceWithUsings_ExtractsAll()
    {
        string code = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.Models
{
    public class Item
    {
        public string Name { get; set; }
    }
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "using");
        Assert.Contains(keywords, k => k.OriginalKeyword == "namespace");
        Assert.Contains(keywords, k => k.OriginalKeyword == "class");
        Assert.Contains(identifiers, i => i.Name == "System");
        Assert.Contains(identifiers, i => i.Name == "Collections");
        Assert.Contains(identifiers, i => i.Name == "Generic");
        Assert.Contains(identifiers, i => i.Name == "Linq");
        Assert.Contains(identifiers, i => i.Name == "MyApp");
        Assert.Contains(identifiers, i => i.Name == "Models");
        Assert.Contains(identifiers, i => i.Name == "Item");
    }

    [Fact]
    public void Parse_LinqBasic_ExtractsLinqIdentifiers()
    {
        string code = @"
using System.Linq;

public class DataProcessor
{
    public List<int> FilterAndSort(List<int> numbers)
    {
        return numbers.Where(n => n > 0).OrderBy(n => n).Select(n => n * 2).ToList();
    }
}";

        ASTNode ast = _adapter.Parse(code);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(identifiers, i => i.Name == "DataProcessor");
        Assert.Contains(identifiers, i => i.Name == "FilterAndSort");
        Assert.Contains(identifiers, i => i.Name == "Where");
        Assert.Contains(identifiers, i => i.Name == "OrderBy");
        Assert.Contains(identifiers, i => i.Name == "Select");
        Assert.Contains(identifiers, i => i.Name == "ToList");
    }

    [Fact]
    public void RoundTrip_ComplexClass_PreservesCode()
    {
        string code = @"using System;
using System.Collections.Generic;

namespace Calculator.Core
{
    public class Calculator
    {
        public int OperationCount = 0;

        public int Add(int a, int b)
        {
            OperationCount++;
            return a + b;
        }

        public int Subtract(int a, int b)
        {
            OperationCount++;
            return a - b;
        }

        public string GetSummary()
        {
            return ""Total: "" + OperationCount;
        }
    }
}";

        ASTNode ast = _adapter.Parse(code);
        string result = _adapter.Generate(ast);

        Assert.Equal(code, result);
    }

    [Fact]
    public void RoundTrip_StructAndEnum_PreservesCode()
    {
        string code = @"public struct Point
{
    public int X;
    public int Y;
}

public enum Direction
{
    North,
    South,
    East,
    West
}";

        ASTNode ast = _adapter.Parse(code);
        string result = _adapter.Generate(ast);

        Assert.Equal(code, result);
    }

    [Fact]
    public void Generate_TranslatedComplexClass_ReplacesCorrectly()
    {
        string code = @"public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}";

        ASTNode ast = _adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw)
            {
                kw.OriginalKeyword = kw.KeywordId switch
                {
                    49 => "publico",
                    10 => "classe",
                    33 => "inteiro",
                    52 => "retornar",
                    _ => kw.OriginalKeyword
                };
            }

            if (child is IdentifierNode id)
            {
                id.Name = id.Name switch
                {
                    "Calculator" => "Calculadora",
                    "Add" => "Somar",
                    _ => id.Name
                };
            }
        }

        string result = _adapter.Generate(ast);

        Assert.Contains("publico", result);
        Assert.Contains("classe", result);
        Assert.Contains("Calculadora", result);
        Assert.Contains("inteiro", result);
        Assert.Contains("Somar", result);
        Assert.Contains("retornar", result);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyAst()
    {
        ASTNode ast = _adapter.Parse("");

        Assert.NotNull(ast);
        Assert.Empty(GetNodesOfType<KeywordNode>(ast));
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEmptyAst()
    {
        ASTNode ast = _adapter.Parse("   \n\t  ");

        Assert.NotNull(ast);
        Assert.Empty(GetNodesOfType<KeywordNode>(ast));
    }

    [Fact]
    public void Parse_FileScopedNamespace_ExtractsKeyword()
    {
        string code = "namespace MyApp;\n\nclass Program { }";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "namespace");
        Assert.Contains(keywords, k => k.OriginalKeyword == "class");
    }

    [Fact]
    public void Parse_AsyncAwait_ExtractsReturnKeyword()
    {
        string code = @"
using System.Threading.Tasks;

class Program
{
    async Task<int> GetValueAsync()
    {
        await Task.Delay(100);
        return 42;
    }
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "return");
        Assert.Contains(keywords, k => k.OriginalKeyword == "class");
        Assert.Contains(keywords, k => k.OriginalKeyword == "int");
    }

    [Fact]
    public void Parse_NullableTypes_ExtractsKeywords()
    {
        string code = @"
class Program
{
    string? name = null;
    int? count = null;
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "class");
        Assert.Contains(keywords, k => k.OriginalKeyword == "null");
    }

    [Fact]
    public void Parse_SwitchExpression_ExtractsKeywords()
    {
        string code = @"
class Program
{
    string GetName(int value)
    {
        return value switch
        {
            1 => ""one"",
            2 => ""two"",
            _ => ""other""
        };
    }
}";

        ASTNode ast = _adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.OriginalKeyword == "switch");
        Assert.Contains(keywords, k => k.OriginalKeyword == "return");
    }

    [Fact]
    public void Parse_UnicodeIdentifiers_Extracts()
    {
        string code = @"
class Programa
{
    string nomeDoAluno = ""teste"";
}";

        ASTNode ast = _adapter.Parse(code);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(identifiers, i => i.Name == "Programa");
        Assert.Contains(identifiers, i => i.Name == "nomeDoAluno");
    }

    [Fact]
    public void Parse_VerbatimString_ExtractsLiteral()
    {
        string code = @"
class Program
{
    string path = @""C:\Users\test"";
}";

        ASTNode ast = _adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        Assert.NotEmpty(literals);
    }

    [Fact]
    public void ValidateSyntax_EmptyString_IsValid()
    {
        ValidationResult result = _adapter.ValidateSyntax("");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateSyntax_SyntaxErrorCode_ReturnsErrors()
    {
        ValidationResult result = _adapter.ValidateSyntax("class { invalid }}}");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void ExtractIdentifiers_EmptyString_ReturnsEmpty()
    {
        List<string> identifiers = _adapter.ExtractIdentifiers("");

        Assert.Empty(identifiers);
    }

    [Fact]
    public void ExtractIdentifiers_OnlyKeywords_ReturnsNoKeywords()
    {
        List<string> identifiers = _adapter.ExtractIdentifiers("class { }");

        Assert.DoesNotContain("class", identifiers);
    }

    [Fact]
    public void Generate_EmptyStatementNode_HandlesGracefully()
    {
        StatementNode ast = new StatementNode();
        ast.Children = new List<ASTNode>();
        ast.RawText = "";

        string result = _adapter.Generate(ast);

        Assert.NotNull(result);
    }

    public static List<T> GetNodesOfType<T>(ASTNode root) where T : ASTNode
    {
        List<T> result = new List<T>();
        CollectNodes(root, result);
        return result;
    }

    static void CollectNodes<T>(ASTNode node, List<T> result) where T : ASTNode
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
