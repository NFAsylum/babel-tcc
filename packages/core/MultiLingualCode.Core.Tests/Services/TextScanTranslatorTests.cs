using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class TextScanTranslatorTests
{
    public static INaturalLanguageProvider MakeProviderFor(Dictionary<int, string> idToTranslation)
    {
        INaturalLanguageProvider provider = Substitute.For<INaturalLanguageProvider>();
        provider.TranslateKeyword(Arg.Any<int>()).Returns(call =>
        {
            int id = call.Arg<int>();
            if (idToTranslation.TryGetValue(id, out string? translated))
            {
                return OperationResultGeneric<string>.Ok(translated);
            }
            return OperationResultGeneric<string>.Fail($"missing id {id}");
        });
        return provider;
    }

    [Fact]
    public void BuildTranslationMap_DefaultOverload_IsCaseSensitive()
    {
        Dictionary<string, int> keywords = new() { ["if"] = 0, ["else"] = 1 };
        INaturalLanguageProvider provider = MakeProviderFor(new() { [0] = "se", [1] = "senão" });

        Dictionary<string, string> map = TextScanTranslator.BuildTranslationMap(keywords, provider);

        Assert.Equal("se", map["if"]);
        Assert.False(map.ContainsKey("IF"));
        Assert.False(map.ContainsKey("If"));
    }

    [Fact]
    public void BuildTranslationMap_CaseInsensitiveTrue_AcceptsAnyCase()
    {
        Dictionary<string, int> keywords = new() { ["se"] = 0, ["entao"] = 1 };
        INaturalLanguageProvider provider = MakeProviderFor(new() { [0] = "if", [1] = "then" });

        Dictionary<string, string> map = TextScanTranslator.BuildTranslationMap(keywords, provider, caseInsensitive: true);

        Assert.Equal("if", map["se"]);
        Assert.Equal("if", map["SE"]);
        Assert.Equal("if", map["Se"]);
        Assert.Equal("then", map["ENTAO"]);
    }

    [Fact]
    public void BuildTranslationMap_NullKeywordMap_ReturnsEmpty()
    {
        INaturalLanguageProvider provider = MakeProviderFor(new());
        Dictionary<string, string> map = TextScanTranslator.BuildTranslationMap(null!, provider);
        Assert.Empty(map);
    }

    [Fact]
    public void BuildTranslationMap_SkipsKeywordsWithoutTranslation()
    {
        Dictionary<string, int> keywords = new() { ["if"] = 0, ["return"] = 1, ["unknown"] = 99 };
        INaturalLanguageProvider provider = MakeProviderFor(new() { [0] = "se", [1] = "retornar" });

        Dictionary<string, string> map = TextScanTranslator.BuildTranslationMap(keywords, provider);

        Assert.Equal(2, map.Count);
        Assert.True(map.ContainsKey("if"));
        Assert.True(map.ContainsKey("return"));
        Assert.False(map.ContainsKey("unknown"));
    }

    [Fact]
    public void Translate_CaseInsensitiveMap_TranslatesAcrossCases()
    {
        Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["algoritmo"] = "algorithm",
            ["inicio"] = "begin",
            ["fimalgoritmo"] = "endalgorithm",
        };

        string source = "ALGORITMO\nINICIO\nfimalgoritmo";
        string translated = TextScanTranslator.Translate(source, map, LanguageScanRules.VisuAlg);

        Assert.Contains("algorithm", translated);
        Assert.Contains("begin", translated);
        Assert.Contains("endalgorithm", translated);
    }
}
