using MultiLingualCode.Core.LanguageAdapters.Portugol;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Integration;

public class PortugolIntegrationTests
{
    public string TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");

    public TranslationOrchestrator CreateOrchestrator(string languageCode)
    {
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(new VisuAlgAdapter());
        registry.RegisterAdapter(new PortugolStudioAdapter());
        NaturalLanguageProvider provider = new NaturalLanguageProvider
        {
            LanguageCode = languageCode,
            TranslationsBasePath = TranslationsPath
        };
        return new TranslationOrchestrator
        {
            Registry = registry,
            Provider = provider,
            IdentifierMapperService = new IdentifierMapper()
        };
    }

    [Fact]
    public async Task VisuAlg_TranslateToPtBr_IsIdentity()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "algoritmo \"teste\"\nvar\n   x: inteiro\ninicio\n   se x > 0 entao\n      escreva(\"positivo\")\n   fimse\nfimalgoritmo";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(source, ".alg", "pt-br");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(source, result.Value);
    }

    [Fact]
    public async Task VisuAlg_RoundTripPtBr_PreservesSource()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "algoritmo \"hello\"\nvar nome: caractere\ninicio\n   escreval(\"oi\")\nfimalgoritmo";

        OperationResultGeneric<string> forward = await orchestrator.TranslateToNaturalLanguageAsync(source, ".alg", "pt-br");
        Assert.True(forward.IsSuccess);

        OperationResultGeneric<string> reverse = await orchestrator.TranslateFromNaturalLanguageAsync(forward.Value, ".alg", "pt-br");
        Assert.True(reverse.IsSuccess);
        Assert.Equal(source, reverse.Value);
    }

    [Fact]
    public async Task VisuAlg_CaseInsensitiveSource_NormalizesToCanonicalLowercase()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string upperSource = "ALGORITMO \"x\"\nINICIO\nFIMALGORITMO";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(upperSource, ".alg", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("algoritmo", result.Value);
        Assert.Contains("inicio", result.Value);
        Assert.Contains("fimalgoritmo", result.Value);
    }

    [Fact]
    public async Task VisuAlg_KeywordsInsideStrings_NotTranslated()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "algoritmo \"se entao fimse\"\ninicio\nfimalgoritmo";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(source, ".alg", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("\"se entao fimse\"", result.Value);
    }

    [Fact]
    public async Task VisuAlg_KeywordsInsideComments_NotTranslated()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "// se e enquanto comment\nalgoritmo \"x\"\ninicio\nfimalgoritmo";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(source, ".alg", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("// se e enquanto comment", result.Value);
    }

    [Fact]
    public async Task PortugolStudio_TranslateToPtBr_IsIdentity()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "programa {\n  funcao inicio() {\n    inteiro x = 5\n    se (x > 0) {\n      escreva(\"oi\")\n    } senao {\n      escreva(\"nao\")\n    }\n  }\n}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(source, ".por", "pt-br");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(source, result.Value);
    }

    [Fact]
    public async Task PortugolStudio_RoundTripPtBr_PreservesSource()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "programa {\n  funcao inicio() {\n    inteiro x = 10\n    enquanto (x > 0) {\n      x = x - 1\n    }\n  }\n}";

        OperationResultGeneric<string> forward = await orchestrator.TranslateToNaturalLanguageAsync(source, ".por", "pt-br");
        Assert.True(forward.IsSuccess);

        OperationResultGeneric<string> reverse = await orchestrator.TranslateFromNaturalLanguageAsync(forward.Value, ".por", "pt-br");
        Assert.True(reverse.IsSuccess);
        Assert.Equal(source, reverse.Value);
    }

    [Fact]
    public async Task PortugolStudio_BlockCommentsAreSkipped()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        string source = "/* se enquanto retorne */\nprograma {\n  funcao inicio() { }\n}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(source, ".por", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("/* se enquanto retorne */", result.Value);
    }

    [Fact]
    public async Task UnknownExtension_ReturnsFailure()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator("pt-br");
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync("source", ".unknown", "pt-br");
        Assert.False(result.IsSuccess);
    }
}
