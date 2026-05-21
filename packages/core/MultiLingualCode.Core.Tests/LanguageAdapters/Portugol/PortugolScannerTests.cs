using MultiLingualCode.Core.LanguageAdapters.Portugol;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.Portugol;

public class PortugolScannerTests
{
    public Dictionary<int, string> IdToOriginal = new()
    {
        [0] = "se",
        [1] = "senao",
        [2] = "enquanto",
    };

    public Dictionary<string, int> TranslatedToId = new(StringComparer.OrdinalIgnoreCase)
    {
        ["if"] = 0,
        ["else"] = 1,
        ["while"] = 2,
    };

    public int Lookup(string word)
    {
        if (TranslatedToId.TryGetValue(word, out int id))
        {
            return id;
        }
        return -1;
    }

    [Fact]
    public void ReverseSubstitute_BasicKeywordReplacement_RestoresOriginal()
    {
        string translated = "if (x > 0) else while";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("se (x > 0) senao enquanto", result);
    }

    [Fact]
    public void ReverseSubstitute_WordInString_IsPreserved()
    {
        string translated = "escreva(\"if branch\")";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("escreva(\"if branch\")", result);
    }

    [Fact]
    public void ReverseSubstitute_WordInLineComment_IsPreserved()
    {
        string translated = "// if and else\nx = 1";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("// if and else\nx = 1", result);
    }

    [Fact]
    public void ReverseSubstitute_WordInBlockComment_IsPreserved_WhenBlockSupported()
    {
        string translated = "/* if branch */\nif (x)";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.PortugolStudio, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("/* if branch */\nse (x)", result);
    }

    [Fact]
    public void ReverseSubstitute_BlockComment_NotSupported_TreatedAsCode()
    {
        // VisuAlg has no block comments, so /* is not skipped; "if" inside it would be translated.
        string translated = "/* if */";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("/* se */", result);
    }

    [Fact]
    public void ReverseSubstitute_SingleQuoteChar_PreservedWhenSupported()
    {
        string translated = "caracter c = 'i'";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.PortugolStudio, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("caracter c = 'i'", result);
    }

    [Fact]
    public void ReverseSubstitute_UnknownWord_PassesThrough()
    {
        string translated = "myVariable = 42";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("myVariable = 42", result);
    }

    [Fact]
    public void ReverseSubstitute_EmptyInput_ReturnsEmpty()
    {
        string result = PortugolScanner.ReverseSubstitute("", LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal("", result);
    }

    [Fact]
    public void ReverseSubstitute_UnterminatedString_DoesNotCrash()
    {
        string translated = "\"unterminated if";
        string result = PortugolScanner.ReverseSubstitute(translated, LanguageScanRules.VisuAlg, Lookup, id => IdToOriginal.GetValueOrDefault(id, ""));
        Assert.Equal(translated, result);
    }
}
