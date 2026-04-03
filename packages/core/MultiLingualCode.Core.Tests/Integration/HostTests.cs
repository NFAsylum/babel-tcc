using System.Text.Json;
using MultiLingualCode.Core.Services;
using Host = MultiLingualCode.Core.Host;

namespace MultiLingualCode.Core.Tests.Integration;

public class HostTests : IDisposable
{
    public string TranslationsPath;
    public string TempDir;

    public HostTests()
    {
        TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        TempDir = Path.Combine(Path.GetTempPath(), $"host_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    // === ExecuteMethod ===

    [Fact]
    public async Task ExecuteMethod_TranslateToNaturalLanguage_ReturnsTranslatedCode()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Host.Program.ExecuteMethod(
            "TranslateToNaturalLanguage", paramsJson, TranslationsPath, TempDir);

        Assert.True(result.Success, result.Error);
        Assert.Contains("classe", result.Result);
    }

    [Fact]
    public async Task ExecuteMethod_TranslateFromNaturalLanguage_ReturnsOriginalCode()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            translatedCode = "publico classe Foo {}",
            fileExtension = ".cs",
            sourceLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Host.Program.ExecuteMethod(
            "TranslateFromNaturalLanguage", paramsJson, TranslationsPath, TempDir);

        Assert.True(result.Success, result.Error);
        Assert.Contains("class", result.Result);
    }

    [Fact]
    public async Task ExecuteMethod_ValidateSyntax_ReturnsValidResult()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Host.Program.ExecuteMethod(
            "ValidateSyntax", paramsJson, TranslationsPath, TempDir);

        Assert.True(result.Success, result.Error);
        Assert.Contains("isValid", result.Result);
    }

    [Fact]
    public async Task ExecuteMethod_GetSupportedLanguages_ReturnsLanguageList()
    {
        Host.CoreResponse result = await Host.Program.ExecuteMethod(
            "GetSupportedLanguages", "{}", TranslationsPath, TempDir);

        Assert.True(result.Success, result.Error);
        Assert.Contains("pt-br", result.Result);
    }

    [Fact]
    public async Task ExecuteMethod_UnknownMethod_ReturnsError()
    {
        Host.CoreResponse result = await Host.Program.ExecuteMethod(
            "NonExistentMethod", "{}", TranslationsPath, TempDir);

        Assert.False(result.Success);
        Assert.Contains("Unknown method", result.Error);
    }

    // === Handlers ===

    [Fact]
    public async Task HandleTranslateToNaturalLanguage_ValidCode_TranslatesKeywords()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.TranslateRequest request = new Host.TranslateRequest
        {
            SourceCode = "if (true) { return; }",
            FileExtension = ".cs",
            TargetLanguage = "pt-br"
        };

        Host.CoreResponse result = await Host.Program.HandleTranslateToNaturalLanguage(orchestrator, request);

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain("if", result.Result.Split(' ')[0] == "if" ? "if_found" : "");
    }

    [Fact]
    public async Task HandleTranslateFromNaturalLanguage_TranslatedCode_ReversesKeywords()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.TranslateRequest translateReq = new Host.TranslateRequest
        {
            SourceCode = "public class Foo {}",
            FileExtension = ".cs",
            TargetLanguage = "pt-br"
        };
        Host.CoreResponse translated = await Host.Program.HandleTranslateToNaturalLanguage(orchestrator, translateReq);
        Assert.True(translated.Success);

        Host.ReverseTranslateRequest reverseReq = new Host.ReverseTranslateRequest
        {
            TranslatedCode = translated.Result,
            FileExtension = ".cs",
            SourceLanguage = "pt-br"
        };
        Host.CoreResponse reversed = await Host.Program.HandleTranslateFromNaturalLanguage(orchestrator, reverseReq);

        Assert.True(reversed.Success, reversed.Error);
        Assert.Contains("public", reversed.Result);
        Assert.Contains("class", reversed.Result);
    }

    [Fact]
    public void HandleGetSupportedLanguages_ValidPath_ReturnsLanguages()
    {
        Host.CoreResponse result = Host.Program.HandleGetSupportedLanguages(TranslationsPath);

        Assert.True(result.Success);
        List<string> languages = JsonSerializer.Deserialize<List<string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.Contains("pt-br", languages);
    }

    [Fact]
    public void HandleGetSupportedLanguages_InvalidPath_ReturnsEmptyList()
    {
        Host.CoreResponse result = Host.Program.HandleGetSupportedLanguages("/nonexistent/path");

        Assert.True(result.Success);
        List<string> languages = JsonSerializer.Deserialize<List<string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.Empty(languages);
    }

    // === GetOrCreateOrchestrator ===

    [Fact]
    public void GetOrCreateOrchestrator_FirstCall_CreatesOrchestrator()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();

        TranslationOrchestrator result = Host.Program.GetOrCreateOrchestrator(
            cache, "pt-br", TranslationsPath, TempDir);

        Assert.NotNull(result);
        Assert.Single(cache);
    }

    [Fact]
    public void GetOrCreateOrchestrator_SameLanguage_ReturnsCached()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();

        TranslationOrchestrator first = Host.Program.GetOrCreateOrchestrator(
            cache, "pt-br", TranslationsPath, TempDir);
        TranslationOrchestrator second = Host.Program.GetOrCreateOrchestrator(
            cache, "pt-br", TranslationsPath, TempDir);

        Assert.Same(first, second);
        Assert.Single(cache);
    }

    [Fact]
    public void GetOrCreateOrchestrator_DifferentLanguage_CreatesNew()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();

        TranslationOrchestrator ptBr = Host.Program.GetOrCreateOrchestrator(
            cache, "pt-br", TranslationsPath, TempDir);
        TranslationOrchestrator esEs = Host.Program.GetOrCreateOrchestrator(
            cache, "es-es", TranslationsPath, TempDir);

        Assert.NotSame(ptBr, esEs);
        Assert.Equal(2, cache.Count);
    }

    // === RunSingleRequest ===

    [Fact]
    public async Task RunSingleRequest_ValidMethod_ReturnsZero()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        StringWriter output = new StringWriter();
        Console.SetOut(output);

        int exitCode = await Host.Program.RunSingleRequest(
            "TranslateToNaturalLanguage", paramsJson, TranslationsPath, TempDir);

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(0, exitCode);
        Assert.Contains("classe", output.ToString());
    }

    [Fact]
    public async Task RunSingleRequest_InvalidMethod_ReturnsOne()
    {
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        int exitCode = await Host.Program.RunSingleRequest(
            "BadMethod", "{}", TranslationsPath, TempDir);

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(1, exitCode);
    }
}
