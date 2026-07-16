using MultiLingualCode.Core.LanguageAdapters.Portugol;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.Portugol;

public class VisuAlgAdapterTests
{
    public VisuAlgAdapter Adapter = new VisuAlgAdapter();

    [Fact]
    public void Properties_WhenAccessed_ReturnExpectedValues()
    {
        Assert.Equal("VisuAlg", Adapter.LanguageName);
        Assert.Equal(new[] { ".alg" }, Adapter.FileExtensions);
        Assert.Equal("1.0.0", Adapter.Version);
    }

    [Fact]
    public void GetScanRules_WhenCalled_ReturnsVisuAlgPreset()
    {
        LanguageScanRules rules = Adapter.GetScanRules();
        Assert.Equal("//", rules.LineComment);
        Assert.Equal("", rules.BlockCommentStart);
        Assert.Equal("", rules.BlockCommentEnd);
        Assert.True(rules.CaseInsensitiveKeywords);
        Assert.False(rules.HasSingleQuoteStrings);
        Assert.False(rules.HasTripleQuoteStrings);
        Assert.False(rules.HasPreprocessor);
    }

    [Fact]
    public void GetKeywordMap_WhenCalled_ContainsAll48Keywords()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.Equal(48, map.Count);
        Assert.Equal(0, map["algoritmo"]);
        Assert.Equal(12, map["se"]);
        Assert.Equal(47, map["debug"]);
    }

    [Fact]
    public void GetKeywordMap_IsCaseInsensitive()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.Equal(12, map["se"]);
        Assert.Equal(12, map["SE"]);
        Assert.Equal(12, map["Se"]);
    }

    [Fact]
    public void Parse_AnySource_ReturnsPassthroughStatementNode()
    {
        string source = "algoritmo \"teste\"\nvar\n   x: inteiro\ninicio\nfimalgoritmo";
        ASTNode ast = Adapter.Parse(source);
        StatementNode statement = Assert.IsType<StatementNode>(ast);
        Assert.Equal("VisuAlgUnit", statement.StatementKind);
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
    public void Generate_PassthroughAst_ReturnsOriginalText()
    {
        string source = "algoritmo \"x\"\ninicio\n  escreva(\"oi\")\nfimalgoritmo";
        ASTNode ast = Adapter.Parse(source);
        string regenerated = Adapter.Generate(ast);
        Assert.Equal(source, regenerated);
    }

    [Fact]
    public void RoundTrip_ParseThenGenerate_PreservesSource()
    {
        string source = "algoritmo \"hello\"\nvar\n   nome: caractere\ninicio\n   escreval(\"Ola, \" + nome)\nfimalgoritmo";
        ASTNode ast = Adapter.Parse(source);
        string regenerated = Adapter.Generate(ast);
        Assert.Equal(source, regenerated);
    }

    [Fact]
    public void ValidateSyntax_AlwaysReturnsValid()
    {
        ValidationResult result = Adapter.ValidateSyntax("algoritmo \"x\"");
        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);

        ValidationResult invalidLooking = Adapter.ValidateSyntax("nonsense !!! @@");
        Assert.True(invalidLooking.IsValid);
    }

    [Fact]
    public void ExtractIdentifiers_ReturnsEmpty()
    {
        List<string> ids = Adapter.ExtractIdentifiers("algoritmo \"x\"\nvar nome, idade: inteiro\ninicio\nfimalgoritmo");
        Assert.Empty(ids);
    }

    [Fact]
    public void TraduMethods_ReturnEmpty_BecauseTraduUnsupported()
    {
        string source = "algoritmo \"x\"\ninicio\n   nome: caractere\nfimalgoritmo";
        Assert.Empty(Adapter.ExtractTrailingComments(source));
        Assert.Empty(Adapter.GetIdentifierNamesOnLine(source, 2));
        Assert.Equal("", Adapter.GetFirstStringLiteralOnLine(source, 0));
        (int start, int end) = Adapter.GetContainingMethodRange(source, 2);
        Assert.Equal(-1, start);
        Assert.Equal(-1, end);
    }

    [Fact]
    public void ReverseSubstituteKeywords_TranslatedSource_RestoresCanonicalKeywords()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = 12,        // se
            ["then"] = 13,      // entao
            ["endif"] = 15,     // fimse
            ["begin"] = 3,      // inicio
            ["endalgorithm"] = 1, // fimalgoritmo
            ["algorithm"] = 0,  // algoritmo
        };
        string translated = "algorithm \"x\"\nbegin\n   if x > 0 then\n      write(\"ok\")\n   endif\nendalgorithm";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("algoritmo", original);
        Assert.Contains("inicio", original);
        Assert.Contains("se ", original);
        Assert.Contains("entao", original);
        Assert.Contains("fimse", original);
        Assert.Contains("fimalgoritmo", original);
        Assert.Contains("\"x\"", original);
        Assert.Contains("\"ok\"", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideStrings_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = 12,
            ["write"] = -1,
        };
        string translated = "write(\"if you see this it stays\")";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("if you see this it stays", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideComments_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = 12,
            ["then"] = 13,
        };
        string translated = "// if then\nvar x";
        string original = Adapter.ReverseSubstituteKeywords(translated, AdapterTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("// if then", original);
    }
}
