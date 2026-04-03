using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class CSharpAdapterTests
{
    public CSharpAdapter Adapter = new();

    [Fact]
    public void Properties_AreCorrect()
    {
        Assert.Equal("CSharp", Adapter.LanguageName);
        Assert.Equal(new[] { ".cs" }, Adapter.FileExtensions);
        Assert.Equal("1.0.0", Adapter.Version);
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

        ASTNode ast = Adapter.Parse(code);

        Assert.IsType<StatementNode>(ast);

        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        // Should find keywords: using, namespace, class, static, void
        Assert.Contains(keywords, k => k.Text == "using" && k.KeywordId == 72);
        Assert.Contains(keywords, k => k.Text == "namespace" && k.KeywordId == 39);
        Assert.Contains(keywords, k => k.Text == "class" && k.KeywordId == 10);
        Assert.Contains(keywords, k => k.Text == "static" && k.KeywordId == 58);
        Assert.Contains(keywords, k => k.Text == "void" && k.KeywordId == 75);

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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "if" && k.KeywordId == 30);
        Assert.Contains(keywords, k => k.Text == "else" && k.KeywordId == 18);
        Assert.Contains(keywords, k => k.Text == "return" && k.KeywordId == 52);
        Assert.Contains(keywords, k => k.Text == "true" && k.KeywordId == 64);
        Assert.Contains(keywords, k => k.Text == "false" && k.KeywordId == 23);
    }

    [Fact]
    public void Parse_ForLoop_ExtractsLoopKeywords()
    {
        string code = "for (int i = 0; i < 10; i++) { break; continue; }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "for" && k.KeywordId == 27);
        Assert.Contains(keywords, k => k.Text == "int" && k.KeywordId == 33);
        Assert.Contains(keywords, k => k.Text == "break" && k.KeywordId == 4);
        Assert.Contains(keywords, k => k.Text == "continue" && k.KeywordId == 12);
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "public" && k.KeywordId == 49);
        Assert.Contains(keywords, k => k.Text == "private" && k.KeywordId == 47);
        Assert.Contains(keywords, k => k.Text == "protected" && k.KeywordId == 48);
        Assert.Contains(keywords, k => k.Text == "readonly" && k.KeywordId == 50);
        Assert.Contains(keywords, k => k.Text == "static" && k.KeywordId == 58);
        Assert.Contains(keywords, k => k.Text == "internal" && k.KeywordId == 35);
        Assert.Contains(keywords, k => k.Text == "virtual" && k.KeywordId == 73);
    }

    [Fact]
    public void Parse_PreservesPositions()
    {
        string code = "public class Program { }";
        //          0123456789...

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        KeywordNode publicKw = keywords.First(k => k.Text == "public");
        Assert.Equal(0, publicKw.StartPosition);
        Assert.Equal(6, publicKw.EndPosition);
        Assert.Equal(0, publicKw.StartLine);

        KeywordNode classKw = keywords.First(k => k.Text == "class");
        Assert.Equal(7, classKw.StartPosition);
        Assert.Equal(12, classKw.EndPosition);
    }

    [Fact]
    public void Parse_PreservesLineNumbers()
    {
        string code = "using System;\nnamespace Test\n{\n    class Foo { }\n}";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        KeywordNode usingKw = keywords.First(k => k.Text == "using");
        Assert.Equal(0, usingKw.StartLine);

        KeywordNode namespaceKw = keywords.First(k => k.Text == "namespace");
        Assert.Equal(1, namespaceKw.StartLine);

        KeywordNode classKw = keywords.First(k => k.Text == "class");
        Assert.Equal(3, classKw.StartLine);
    }

    [Fact]
    public void Parse_ExtractsLiterals()
    {
        string code = @"string s = ""hello""; int n = 42; char c = 'a';";

        ASTNode ast = Adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        Assert.Contains(literals, l => l.Type == LiteralType.String && (string?)l.Value == "hello");
        Assert.Contains(literals, l => l.Type == LiteralType.Number && Convert.ToInt32(l.Value) == 42);
        Assert.Contains(literals, l => l.Type == LiteralType.Char && (char?)l.Value == 'a');
    }

    [Fact]
    public void Parse_StringLiterals_AreTranslatable()
    {
        string code = @"string msg = ""Hello World"";";

        ASTNode ast = Adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        LiteralNode stringLiteral = literals.First(l => l.Type == LiteralType.String);
        Assert.True(stringLiteral.IsTranslatable);
    }

    [Fact]
    public void Parse_NumericLiterals_AreNotTranslatable()
    {
        string code = "int x = 42;";

        ASTNode ast = Adapter.Parse(code);
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "try" && k.KeywordId == 65);
        Assert.Contains(keywords, k => k.Text == "throw" && k.KeywordId == 63);
        Assert.Contains(keywords, k => k.Text == "new" && k.KeywordId == 40);
        Assert.Contains(keywords, k => k.Text == "catch" && k.KeywordId == 7);
        Assert.Contains(keywords, k => k.Text == "finally" && k.KeywordId == 24);
    }

    [Fact]
    public void Generate_WithoutChanges_PreservesOriginalCode()
    {
        string code = "public class Program { }";

        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);

        Assert.Equal(code, result);
    }

    [Fact]
    public void Generate_WithTranslatedKeywords_ReplacesInCode()
    {
        string code = "public class Program { }";

        ASTNode ast = Adapter.Parse(code);

        // Translate keywords
        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw)
            {
                kw.Text = kw.KeywordId switch
                {
                    49 => "publico",  // public
                    10 => "classe",   // class
                    _ => kw.Text
                };
            }
        }

        string result = Adapter.Generate(ast);

        Assert.Equal("publico classe Program { }", result);
    }

    [Fact]
    public void Generate_WithTranslatedIdentifiers_ReplacesInCode()
    {
        string code = "class Calculator { }";

        ASTNode ast = Adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is IdentifierNode id && id.Name == "Calculator")
            {
                id.Name = "Calculadora";
            }
        }

        string result = Adapter.Generate(ast);

        Assert.Equal("class Calculadora { }", result);
    }

    [Fact]
    public void Generate_MultiLine_PreservesStructure()
    {
        string code = "public class Program\n{\n    static void Main()\n    {\n    }\n}";

        ASTNode ast = Adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw && kw.Text == "public")
            {
                kw.Text = "publico";
            }
        }

        string result = Adapter.Generate(ast);

        Assert.StartsWith("publico", result);
        Assert.Contains("class Program", result);
        Assert.Contains("static void Main()", result);
    }

    [Fact]
    public void GetKeywordMap_ReturnsAllCSharpKeywords()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();

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

        ValidationResult result = Adapter.ValidateSyntax(code);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void ValidateSyntax_InvalidCode_ReturnsErrors()
    {
        string code = "public class { }"; // missing class name

        ValidationResult result = Adapter.ValidateSyntax(code);

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

        List<string> identifiers = Adapter.ExtractIdentifiers(code);

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
        List<string> identifiers = Adapter.ExtractIdentifiers("");

        Assert.Empty(identifiers);
    }

    [Fact]
    public void Parse_AllIdentifiers_AreTranslatable()
    {
        string code = "int myVariable = 10;";

        ASTNode ast = Adapter.Parse(code);
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.Text == "class");
        Assert.Contains(keywords, k => k.Text == "public");
        Assert.Contains(keywords, k => k.Text == "string");
        Assert.Contains(keywords, k => k.Text == "int");
        Assert.Contains(keywords, k => k.Text == "return");
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.Text == "struct");
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.Text == "enum");
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

        ASTNode ast = Adapter.Parse(code);
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.Contains(keywords, k => k.Text == "using");
        Assert.Contains(keywords, k => k.Text == "namespace");
        Assert.Contains(keywords, k => k.Text == "class");
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

        ASTNode ast = Adapter.Parse(code);
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

        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);

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

        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);

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

        ASTNode ast = Adapter.Parse(code);

        foreach (ASTNode child in ast.Children)
        {
            if (child is KeywordNode kw)
            {
                kw.Text = kw.KeywordId switch
                {
                    49 => "publico",
                    10 => "classe",
                    33 => "inteiro",
                    52 => "retornar",
                    _ => kw.Text
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

        string result = Adapter.Generate(ast);

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
        ASTNode ast = Adapter.Parse("");

        Assert.NotNull(ast);
        Assert.Empty(GetNodesOfType<KeywordNode>(ast));
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEmptyAst()
    {
        ASTNode ast = Adapter.Parse("   \n\t  ");

        Assert.NotNull(ast);
        Assert.Empty(GetNodesOfType<KeywordNode>(ast));
    }

    [Fact]
    public void Parse_FileScopedNamespace_ExtractsKeyword()
    {
        string code = "namespace MyApp;\n\nclass Program { }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "namespace");
        Assert.Contains(keywords, k => k.Text == "class");
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "return");
        Assert.Contains(keywords, k => k.Text == "class");
        Assert.Contains(keywords, k => k.Text == "int");
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "class");
        Assert.Contains(keywords, k => k.Text == "null");
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

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "switch");
        Assert.Contains(keywords, k => k.Text == "return");
    }

    [Fact]
    public void Parse_UnicodeIdentifiers_Extracts()
    {
        string code = @"
class Programa
{
    string nomeDoAluno = ""teste"";
}";

        ASTNode ast = Adapter.Parse(code);
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

        ASTNode ast = Adapter.Parse(code);
        List<LiteralNode> literals = GetNodesOfType<LiteralNode>(ast);

        Assert.NotEmpty(literals);
    }

    [Fact]
    public void ValidateSyntax_EmptyString_IsValid()
    {
        ValidationResult result = Adapter.ValidateSyntax("");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateSyntax_SyntaxErrorCode_ReturnsErrors()
    {
        ValidationResult result = Adapter.ValidateSyntax("class { invalid }}}");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void ExtractIdentifiers_EmptyString_ReturnsEmpty()
    {
        List<string> identifiers = Adapter.ExtractIdentifiers("");

        Assert.Empty(identifiers);
    }

    [Fact]
    public void ExtractIdentifiers_OnlyKeywords_ReturnsNoKeywords()
    {
        List<string> identifiers = Adapter.ExtractIdentifiers("class { }");

        Assert.DoesNotContain("class", identifiers);
    }

    [Fact]
    public void Generate_EmptyStatementNode_HandlesGracefully()
    {
        StatementNode ast = new StatementNode();
        ast.Children = new List<ASTNode>();
        ast.RawText = "";

        string result = Adapter.Generate(ast);

        Assert.NotNull(result);
    }

    [Fact]
    public void Parse_VarDeclaration_CreatesKeywordNode()
    {
        string code = "class C { void M() { var x = 5; } }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "var" && k.KeywordId == 74);
    }

    [Fact]
    public void Parse_VarAsIdentifier_DoesNotCreateKeywordNode()
    {
        string code = "class C { int var = 5; }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);
        List<IdentifierNode> identifiers = GetNodesOfType<IdentifierNode>(ast);

        Assert.DoesNotContain(keywords, k => k.Text == "var");
        Assert.Contains(identifiers, i => i.Name == "var");
    }

    [Fact]
    public void Parse_AsyncMethod_CreatesKeywordNode()
    {
        string code = @"
using System.Threading.Tasks;

class C
{
    async Task M() { }
}";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "async" && k.KeywordId == 78);
    }

    [Fact]
    public void Parse_AwaitExpression_CreatesKeywordNode()
    {
        string code = @"
using System.Threading.Tasks;

class C
{
    async Task M() { await Task.Delay(1); }
}";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "await" && k.KeywordId == 79);
    }

    [Fact]
    public void Parse_YieldReturn_CreatesKeywordNode()
    {
        string code = @"
using System.Collections.Generic;

class C
{
    IEnumerable<int> M() { yield return 1; }
}";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "yield" && k.KeywordId == 80);
    }

    [Fact]
    public void Parse_RecordDeclaration_CreatesKeywordNode()
    {
        string code = "record Point(int X, int Y);";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "record" && k.KeywordId == 81);
    }

    [Fact]
    public void Parse_PartialClass_CreatesKeywordNode()
    {
        string code = "partial class C { }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "partial" && k.KeywordId == 82);
    }

    [Fact]
    public void Parse_WhereConstraint_CreatesKeywordNode()
    {
        string code = "class C<T> where T : class { }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "where" && k.KeywordId == 83);
    }

    [Fact]
    public void Parse_DynamicType_CreatesKeywordNode()
    {
        string code = "class C { void M() { dynamic d = 1; } }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "dynamic" && k.KeywordId == 84);
    }

    [Fact]
    public void Parse_NameofExpression_CreatesKeywordNode()
    {
        string code = "class C { string s = nameof(C); }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "nameof" && k.KeywordId == 85);
    }

    [Fact]
    public void Parse_InitAccessor_CreatesKeywordNode()
    {
        string code = "class C { public int X { get; init; } }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "init" && k.KeywordId == 86);
    }

    [Fact]
    public void Parse_RequiredProperty_CreatesKeywordNode()
    {
        string code = "class C { required public int X { get; set; } }";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "required" && k.KeywordId == 87);
    }

    [Fact]
    public void Parse_GlobalUsing_CreatesKeywordNode()
    {
        string code = "global using System;";

        ASTNode ast = Adapter.Parse(code);
        List<KeywordNode> keywords = GetNodesOfType<KeywordNode>(ast);

        Assert.Contains(keywords, k => k.Text == "global" && k.KeywordId == 88);
    }

    [Fact]
    public void RoundTrip_ContextualKeywords_PreservesCode()
    {
        string code = @"using System.Threading.Tasks;

class C
{
    async Task M()
    {
        var x = 5;
        dynamic d = x;
        string s = nameof(x);
        await Task.Delay(1);
    }
}";

        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);

        Assert.Equal(code, result);
    }

    // --- ReverseSubstituteKeywords tests ---

    public static int MockLookupPtBr(string word)
    {
        Dictionary<string, int> ptBrKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["publico"] = 49,
            ["classe"] = 10,
            ["estatico"] = 58,
            ["vazio"] = 75,
            ["texto"] = 59,
            ["usando"] = 72,
            ["se"] = 30,
            ["senao"] = 18,
            ["retornar"] = 52,
            ["para"] = 27,
            ["enquanto"] = 77,
            ["inteiro"] = 33,
            ["novo"] = 40,
            ["nulo"] = 41,
            ["verdadeiro"] = 64,
            ["falso"] = 23,
            ["espaco_de_nomes"] = 39,
            ["privado"] = 47,
            ["somenteleitura"] = 50,
            ["booleano"] = 3,
            ["espaconome"] = 39,
            ["paracada"] = 28,
            ["em"] = 32,
            ["saida"] = 44,
        };

        if (ptBrKeywords.TryGetValue(word, out int id))
        {
            return id;
        }
        return -1;
    }

    [Fact]
    public void ReverseSubstituteKeywords_SimpleCode_ReplacesAllKeywords()
    {
        string translatedCode = @"usando Sistema;

espaco_de_nomes MeuProjeto
{
    publico classe Programa { }
}";
        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.Contains("using", result);
        Assert.Contains("namespace", result);
        Assert.Contains("public", result);
        Assert.Contains("class", result);
        Assert.DoesNotContain("usando", result);
        Assert.DoesNotContain("espaco_de_nomes", result);
        Assert.DoesNotContain("publico", result);
        Assert.DoesNotContain("classe", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_ManuallyAddedLine_ReplacesNewKeywords()
    {
        string translatedCode = @"usando Sistema;

espaco_de_nomes MeuProjeto
{
    publico classe Programa
    {
        publico estatico vazio Principal()
        {
        }
        publico estatico texto teste = ""oi"";
    }
}";

        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.Contains("public", result);
        Assert.Contains("class", result);
        Assert.Contains("static", result);
        Assert.Contains("void", result);
        Assert.Contains("string", result);
        Assert.DoesNotContain("publico", result);
        Assert.DoesNotContain("classe", result);
        Assert.DoesNotContain("estatico", result);
        Assert.DoesNotContain("vazio", result);
        Assert.DoesNotContain("texto", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_PreservesStringLiterals()
    {
        string translatedCode = @"usando Sistema;

espaco_de_nomes MeuProjeto
{
    publico classe Programa
    {
        publico texto nome = ""publico estatico"";
    }
}";

        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.Contains("public", result);
        Assert.Contains("class", result);
        Assert.Contains("string", result);
        Assert.Contains("\"publico estatico\"", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_PreservesComments()
    {
        string translatedCode = @"usando Sistema;

espaco_de_nomes MeuProjeto
{
    publico classe Programa
    {
        // publico estatico vazio
        /* publico estatico */
        publico vazio Metodo() { }
    }
}";

        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.Contains("public", result);
        Assert.Contains("class", result);
        Assert.Contains("// publico estatico vazio", result);
        Assert.Contains("/* publico estatico */", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_NoTranslatedKeywords_ReturnsUnchanged()
    {
        string code = "using System;\n\nnamespace Test\n{\n    public class Program { }\n}";
        string result = Adapter.ReverseSubstituteKeywords(code, MockLookupPtBr);

        Assert.Equal(code, result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_EmptyString_ReturnsEmpty()
    {
        string result = Adapter.ReverseSubstituteKeywords("", MockLookupPtBr);
        Assert.Equal("", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_MultipleMembers_ReplacesAll()
    {
        string translatedCode = @"usando Sistema;

espaco_de_nomes MeuProjeto
{
    publico classe MinhaClasse
    {
        publico estatico vazio Principal()
        {
            Console.WriteLine(""Ola"");
        }
        publico estatico inteiro valor = 42;
        publico texto nome = ""teste"";
    }
}";

        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.Contains("using", result);
        Assert.Contains("namespace", result);
        Assert.Contains("public", result);
        Assert.Contains("class", result);
        Assert.Contains("static", result);
        Assert.Contains("void", result);
        Assert.Contains("int", result);
        Assert.Contains("string", result);
        Assert.DoesNotContain("usando", result);
        Assert.DoesNotContain("espaco_de_nomes", result);
        Assert.DoesNotContain("publico", result);
        Assert.DoesNotContain("estatico", result);
        Assert.DoesNotContain("inteiro", result);
        Assert.DoesNotContain("texto", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_ComplexFile_ReplacesAllKeywords()
    {
        string translatedCode = @"espaconome MultiLingualCode.Core.LanguageAdapters;

publico estatico classe CSharpKeywordMap
{
    publico estatico somenteleitura Dictionary<texto, inteiro> TextToId = novo(StringComparer.OrdinalIgnoreCase)
    {
        [""abstract""] = 0,
        [""class""] = 10,
        [""int""] = 33,
    };

    publico estatico somenteleitura Dictionary<inteiro, texto> IdToText;

    estatico CSharpKeywordMap()
    {
        IdToText = novo Dictionary<inteiro, texto>(TextToId.Count);
        paracada (KeyValuePair<texto, inteiro> kvp em TextToId)
        {
            IdToText[kvp.Value] = kvp.Key;
        }
    }

    publico estatico inteiro GetId(texto keywordText)
    {
        se (TextToId.TryGetValue(keywordText, saida inteiro id))
        {
            retornar id;
        }
        retornar -1;
    }

    publico estatico booleano IsKeyword(Microsoft.CodeAnalysis.CSharp.SyntaxKind kind)
    {
        retornar RoslynWrapper.IsKeywordKind(kind);
    }
}";

        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.DoesNotContain("espaconome", result);
        Assert.DoesNotContain("publico", result);
        Assert.DoesNotContain("estatico", result);
        Assert.DoesNotContain("classe", result);
        Assert.DoesNotContain("somenteleitura", result);
        Assert.DoesNotContain("texto", result);
        Assert.DoesNotContain("inteiro", result);
        Assert.DoesNotContain("novo", result);
        Assert.DoesNotContain("paracada", result);
        Assert.DoesNotContain("saida", result);
        Assert.DoesNotContain("retornar", result);
        Assert.DoesNotContain("booleano", result);

        Assert.Contains("namespace", result);
        Assert.Contains("public", result);
        Assert.Contains("static", result);
        Assert.Contains("class", result);
        Assert.Contains("readonly", result);
        Assert.Contains("string", result);
        Assert.Contains("int", result);
        Assert.Contains("new", result);
        Assert.Contains("foreach", result);
        Assert.Contains("out", result);
        Assert.Contains("return", result);
        Assert.Contains("bool", result);

        // String literals must be preserved
        Assert.Contains("\"abstract\"", result);
        Assert.Contains("\"class\"", result);
        Assert.Contains("\"int\"", result);
    }

    [Fact]
    public void ReverseSubstituteKeywords_VerbatimStrings_PreservesContent()
    {
        string translatedCode = @"usando Sistema;

espaconome MeuProjeto
{
    publico classe Programa
    {
        publico texto s = @""publico classe"";
    }
}";

        string result = Adapter.ReverseSubstituteKeywords(translatedCode, MockLookupPtBr);

        Assert.Contains("public", result);
        Assert.Contains("string", result);
        Assert.Contains("@\"publico classe\"", result);
    }

    // --- Helper methods ---

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

    [Fact]
    public void RoundTrip_DoubleQuoteString_PreservesQuotes()
    {
        string code = "string x = \"hello\";";
        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);
        Assert.Equal(code, result);
    }

    // Note: verbatim strings (@"...") lose their @ prefix in round-trip because
    // CSharpAdapter.CollectReplacements hardcodes double quotes. Known limitation
    // documented in tarefa 071. CSharpAdapter should migrate to AdapterHelpers
    // like PythonAdapter did.

    [Fact]
    public void RoundTrip_SimpleCode_PreservesStructure()
    {
        string code = "public class Foo { public int Bar() { return 42; } }";
        ASTNode ast = Adapter.Parse(code);
        string result = Adapter.Generate(ast);
        Assert.Equal(code, result);
    }
}
