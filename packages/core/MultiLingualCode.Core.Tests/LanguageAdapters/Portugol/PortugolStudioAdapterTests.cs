using MultiLingualCode.Core.LanguageAdapters.Portugol;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.Portugol;

public class PortugolStudioAdapterTests
{
    public PortugolStudioAdapter Adapter = new PortugolStudioAdapter();

    [Fact]
    public void Properties_WhenAccessed_ReturnExpectedValues()
    {
        Assert.Equal("PortugolStudio", Adapter.LanguageName);
        Assert.Equal(new[] { ".por" }, Adapter.FileExtensions);
        Assert.Equal("1.0.0", Adapter.Version);
    }

    [Fact]
    public void GetScanRules_WhenCalled_ReturnsPortugolStudioPreset()
    {
        LanguageScanRules rules = Adapter.GetScanRules();
        Assert.Equal("//", rules.LineComment);
        Assert.Equal("/*", rules.BlockCommentStart);
        Assert.Equal("*/", rules.BlockCommentEnd);
        Assert.False(rules.CaseInsensitiveKeywords);
        Assert.True(rules.HasSingleQuoteStrings);
        Assert.False(rules.HasTripleQuoteStrings);
        Assert.False(rules.HasPreprocessor);
    }

    [Fact]
    public void GetKeywordMap_WhenCalled_ContainsAll26Keywords()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.Equal(26, map.Count);
        Assert.Equal(0, map["inteiro"]);
        Assert.Equal(16, map["programa"]);
        Assert.Equal(25, map["falso"]);
    }

    [Fact]
    public void GetKeywordMap_IsCaseSensitive()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.True(map.ContainsKey("se"));
        Assert.False(map.ContainsKey("SE"));
        Assert.False(map.ContainsKey("Se"));
    }

    [Fact]
    public void Parse_AnySource_ReturnsPassthroughStatementNode()
    {
        string source = "programa {\n  funcao inicio() {\n    inteiro x = 5\n  }\n}";
        ASTNode ast = Adapter.Parse(source);
        StatementNode statement = Assert.IsType<StatementNode>(ast);
        Assert.Equal("PortugolStudioUnit", statement.StatementKind);
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
        string source = "programa {\n  funcao inicio() {\n    se (x > 0) { escreva(\"oi\") }\n  }\n}";
        ASTNode ast = Adapter.Parse(source);
        string regenerated = Adapter.Generate(ast);
        Assert.Equal(source, regenerated);
    }

    [Fact]
    public void ValidateSyntax_AlwaysReturnsValid()
    {
        ValidationResult result = Adapter.ValidateSyntax("programa { funcao inicio() {} }");
        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void ExtractIdentifiers_ReturnsEmpty()
    {
        List<string> ids = Adapter.ExtractIdentifiers("programa { funcao inicio() { inteiro idade } }");
        Assert.Empty(ids);
    }

    [Fact]
    public void TraduMethods_ReturnEmpty_BecauseTraduUnsupported()
    {
        string source = "programa { funcao inicio() { inteiro x = 5 } }";
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
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["program"] = 16,    // programa
            ["function"] = 17,   // funcao
            ["integer"] = 0,     // inteiro
            ["if"] = 6,          // se
            ["else"] = 7,        // senão
        };
        string translated = "program {\n  function start() {\n    integer x = 5\n    if (x > 0) {} else {}\n  }\n}";
        string original = Adapter.ReverseSubstituteKeywords(translated, PortugolTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("programa", original);
        Assert.Contains("funcao", original);
        Assert.Contains("inteiro", original);
        Assert.Contains("se", original);
        Assert.Contains("senao", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideBlockComments_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = 6,
        };
        string translated = "/* if appears here but should stay */\nx = 1";
        string original = Adapter.ReverseSubstituteKeywords(translated, PortugolTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("/* if appears here but should stay */", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideLineComments_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = 6,
        };
        string translated = "// if comment\nx = 1";
        string original = Adapter.ReverseSubstituteKeywords(translated, PortugolTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("// if comment", original);
    }

    [Fact]
    public void ReverseSubstituteKeywords_KeywordsInsideCharLiterals_AreNotTranslated()
    {
        Dictionary<string, int> reverseMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["if"] = 6,
        };
        string translated = "caracter c = 'i'";
        string original = Adapter.ReverseSubstituteKeywords(translated, PortugolTestHelpers.MakeLookup(reverseMap));
        Assert.Contains("'i'", original);
    }
}
