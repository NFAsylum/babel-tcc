using MultiLingualCode.Core.LanguageAdapters.JavaScript;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.JavaScript;

public class JavaScriptAdapterTests
{
    public JavaScriptAdapter Adapter = new JavaScriptAdapter();

    [Fact]
    public void Properties_WhenAccessed_ReturnExpectedValues()
    {
        Assert.Equal("JavaScript", Adapter.LanguageName);
        Assert.Equal(new[] { ".js" }, Adapter.FileExtensions);
        Assert.Equal("1.0.0", Adapter.Version);
    }

    [Fact]
    public void GetScanRules_WhenCalled_ReturnsJavaScriptPreset()
    {
        LanguageScanRules rules = Adapter.GetScanRules();
        Assert.Equal("//", rules.LineComment);
        Assert.Equal("/*", rules.BlockCommentStart);
        Assert.Equal("*/", rules.BlockCommentEnd);
        Assert.True(rules.HasSingleQuoteStrings);
        Assert.True(rules.HasBacktickStrings);
        Assert.False(rules.HasTripleQuoteStrings);
        Assert.False(rules.HasPreprocessor);
        Assert.False(rules.CaseInsensitiveKeywords);
    }

    [Fact]
    public void GetKeywordMap_WhenCalled_ContainsAll38Keywords()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.Equal(38, map.Count);
        Assert.Equal(5, map["class"]);
        Assert.Equal(17, map["function"]);
        Assert.Equal(37, map["yield"]);
    }

    [Fact]
    public void Parse_AnySource_ReturnsPassthroughStatementNode()
    {
        string source = "function add(a, b) { return a + b; }";
        ASTNode ast = Adapter.Parse(source);
        StatementNode statement = Assert.IsType<StatementNode>(ast);
        Assert.Equal("JavaScriptUnit", statement.StatementKind);
        Assert.Equal(source, statement.RawText);
        Assert.Empty(statement.Children);
    }

    [Fact]
    public void Parse_EmptySource_ReturnsValidNode()
    {
        ASTNode ast = Adapter.Parse("");
        StatementNode statement = Assert.IsType<StatementNode>(ast);
        Assert.Equal("", statement.RawText);
    }

    [Fact]
    public void RoundTrip_ParseThenGenerate_PreservesSource()
    {
        string source = "const x = 1;\nfunction f() { return x; }\n";
        ASTNode ast = Adapter.Parse(source);
        string regenerated = Adapter.Generate(ast);
        Assert.Equal(source, regenerated);
    }

    [Fact]
    public void ValidateSyntax_AlwaysReturnsValid()
    {
        ValidationResult result = Adapter.ValidateSyntax("function f() {}");
        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);

        ValidationResult invalidLooking = Adapter.ValidateSyntax("nonsense !!! @@");
        Assert.True(invalidLooking.IsValid);
    }

    [Fact]
    public void TraduMethods_ReturnEmpty_BecauseTraduUnsupported()
    {
        string source = "const nome = 'x';\nfunction f() {}\n";
        Assert.Empty(Adapter.ExtractIdentifiers(source));
        Assert.Empty(Adapter.ExtractTrailingComments(source));
        Assert.Empty(Adapter.GetIdentifierNamesOnLine(source, 0));
        Assert.Equal("", Adapter.GetFirstStringLiteralOnLine(source, 0));
        (int start, int end) = Adapter.GetContainingMethodRange(source, 0);
        Assert.Equal(-1, start);
        Assert.Equal(-1, end);
    }

    [Fact]
    public void ReverseSubstituteKeywords_TranslatedSource_RestoresCanonicalKeywords()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.Ordinal)
        {
            ["funcao"] = 17,     // function
            ["constante"] = 6,   // const
            ["se"] = 18,         // if
            ["retornar"] = 25,   // return
            ["verdadeiro"] = 31, // true
        };
        string translated = "funcao add(a) {\n  constante ok = verdadeiro;\n  se (a) { retornar ok; }\n}";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("function add", original);
        Assert.Contains("const ok", original);
        Assert.Contains("if (a)", original);
        Assert.Contains("return ok", original);
        Assert.Contains("true", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideStrings_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.Ordinal)
        {
            ["se"] = 18, // if
        };
        string translated = "const msg = \"se isto aparecer, fica\";";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("\"se isto aparecer, fica\"", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideTemplateLiterals_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.Ordinal)
        {
            ["se"] = 18, // if
        };
        string translated = "const msg = `valor se ${a} fim`;";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("`valor se ${a} fim`", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideComments_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.Ordinal)
        {
            ["se"] = 18, // if
        };
        string translated = "// se aparece aqui fica\nconst x = 1;";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("// se aparece aqui fica", original);
    }
}
