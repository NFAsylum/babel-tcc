using System.Text.Json;
using MultiLingualCode.Core.Services;
using Host = MultiLingualCode.Core.Host;

namespace MultiLingualCode.Core.Tests.Integration;

public class HostTests : IDisposable
{
    public string TranslationsPath;
    public string TempDir;
    public LanguageRegistry Registry;

    public HostTests()
    {
        TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        TempDir = Path.Combine(Path.GetTempPath(), $"host_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
        Registry = Host.Program.CreateRegistry();
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    public Task<Host.CoreResponse> Route(string method, string paramsJson)
    {
        Dictionary<string, TranslationOrchestrator> cache = new();
        return Host.Program.RouteRequest(method, paramsJson, Registry, TranslationsPath, TempDir, cache);
    }

    public Task<Host.CoreResponse> RouteWithCache(string method, string paramsJson, Dictionary<string, TranslationOrchestrator> cache)
    {
        return Host.Program.RouteRequest(method, paramsJson, Registry, TranslationsPath, TempDir, cache);
    }

    // === RouteRequest ===

    [Fact]
    public async Task RouteRequest_TranslateToNaturalLanguage_ReturnsTranslatedCode()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("TranslateToNaturalLanguage", paramsJson);

        Assert.True(result.Success, result.Error);
        Assert.Contains("classe", result.Result);
    }

    [Fact]
    public async Task RouteRequest_TranslateFromNaturalLanguage_ReturnsOriginalCode()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            translatedCode = "publico classe Foo {}",
            fileExtension = ".cs",
            sourceLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("TranslateFromNaturalLanguage", paramsJson);

        Assert.True(result.Success, result.Error);
        Assert.Contains("class", result.Result);
    }

    [Fact]
    public async Task RouteRequest_ValidateSyntax_ReturnsValidResult()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("ValidateSyntax", paramsJson);

        Assert.True(result.Success, result.Error);
        Assert.Contains("isValid", result.Result);
    }

    [Fact]
    public async Task RouteRequest_GetSupportedLanguages_ReturnsLanguageList()
    {
        Host.CoreResponse result = await Route("GetSupportedLanguages", "{}");

        Assert.True(result.Success, result.Error);
        Assert.Contains("pt-br", result.Result);
    }

    [Fact]
    public async Task RouteRequest_UnknownMethod_ReturnsError()
    {
        Host.CoreResponse result = await Route("NonExistentMethod", "{}");

        Assert.False(result.Success);
        Assert.False(result.Success);
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
        Assert.Contains("se", result.Result);
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

    // === RouteRequest with cache ===

    [Fact]
    public async Task RouteRequest_WithCache_TranslateToNaturalLanguage_ReturnsTranslatedCode()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await RouteWithCache("TranslateToNaturalLanguage", paramsJson, cache);

        Assert.True(result.Success, result.Error);
        Assert.Contains("classe", result.Result);
    }

    [Fact]
    public async Task RouteRequest_WithCache_TranslateFromNaturalLanguage_ReturnsOriginalCode()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();
        string paramsJson = JsonSerializer.Serialize(new
        {
            translatedCode = "publico classe Foo {}",
            fileExtension = ".cs",
            sourceLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await RouteWithCache("TranslateFromNaturalLanguage", paramsJson, cache);

        Assert.True(result.Success, result.Error);
        Assert.Contains("class", result.Result);
    }

    [Fact]
    public async Task RouteRequest_WithCache_ValidateSyntax_ReturnsValidResult()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await RouteWithCache("ValidateSyntax", paramsJson, cache);

        Assert.True(result.Success, result.Error);
        Assert.Contains("isValid", result.Result);
    }

    [Fact]
    public async Task RouteRequest_WithCache_GetSupportedLanguages_ReturnsLanguageList()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();

        Host.CoreResponse result = await RouteWithCache("GetSupportedLanguages", "{}", cache);

        Assert.True(result.Success, result.Error);
        Assert.Contains("pt-br", result.Result);
    }

    [Fact]
    public async Task RouteRequest_WithCache_UnknownMethod_ReturnsError()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();

        Host.CoreResponse result = await RouteWithCache("BadMethod", "{}", cache);

        Assert.False(result.Success);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_WithCache_InvalidJson_ReturnsError()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();

        Host.CoreResponse result = await RouteWithCache("TranslateToNaturalLanguage", "not json{{{", cache);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_WithCache_ReusesCachedOrchestrator()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        await RouteWithCache("TranslateToNaturalLanguage", paramsJson, cache);
        await RouteWithCache("TranslateToNaturalLanguage", paramsJson, cache);

        Assert.Single(cache);
    }

    // === CreateOrchestrator ===

    [Fact]
    public void CreateOrchestrator_WithValidParams_ReturnsOrchestrator()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);

        Assert.NotNull(orchestrator);
        Assert.NotNull(orchestrator.Registry);
        Assert.NotNull(orchestrator.Provider);
        Assert.NotNull(orchestrator.IdentifierMapperService);
    }

    [Fact]
    public void CreateOrchestrator_WithEmptyProjectPath_DoesNotThrow()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, "");

        Assert.NotNull(orchestrator);
    }

    // === WriteError ===

    [Fact]
    public void WriteError_WritesToStdErr()
    {
        StringWriter errOutput = new StringWriter();
        Console.SetError(errOutput);

        Host.Program.WriteError("test error message");

        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });

        string output = errOutput.ToString();
        Assert.Contains("test error message", output);
        Assert.Contains("false", output);
    }

    // === Main smoke tests ===

    [Fact]
    public async Task Main_WithMethodArg_ReturnsZero()
    {
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        int exitCode = await Host.Program.Main(new[]
        {
            "--method", "GetSupportedLanguages",
            "--translations", TranslationsPath
        });

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(0, exitCode);
        Assert.Contains("pt-br", output.ToString());
    }

    [Fact]
    public async Task Main_WithInvalidMethod_ReturnsOne()
    {
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        int exitCode = await Host.Program.Main(new[]
        {
            "--method", "NonExistent",
            "--translations", TranslationsPath
        });

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task Main_WithTranslateMethod_TranslatesCode()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        StringWriter output = new StringWriter();
        Console.SetOut(output);

        int exitCode = await Host.Program.Main(new[]
        {
            "--method", "TranslateToNaturalLanguage",
            "--params", paramsJson,
            "--translations", TranslationsPath
        });

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(0, exitCode);
        Assert.Contains("classe", output.ToString());
    }

    // === ExtractLanguageCode ===

    [Fact]
    public void ExtractLanguageCode_TranslateTo_ReturnsTargetLanguage()
    {
        string paramsJson = JsonSerializer.Serialize(new { targetLanguage = "pt-br" }, Host.Program.JsonOptions);

        MultiLingualCode.Core.Models.OperationResultGeneric<string> result = Host.Program.ExtractLanguageCode("TranslateToNaturalLanguage", paramsJson);

        Assert.True(result.IsSuccess);
        Assert.Equal("pt-br", result.Value);
    }

    [Fact]
    public void ExtractLanguageCode_TranslateFrom_ReturnsSourceLanguage()
    {
        string paramsJson = JsonSerializer.Serialize(new { sourceLanguage = "es-es" }, Host.Program.JsonOptions);

        MultiLingualCode.Core.Models.OperationResultGeneric<string> result = Host.Program.ExtractLanguageCode("TranslateFromNaturalLanguage", paramsJson);

        Assert.True(result.IsSuccess);
        Assert.Equal("es-es", result.Value);
    }

    [Fact]
    public void ExtractLanguageCode_MissingField_ReturnsFail()
    {
        MultiLingualCode.Core.Models.OperationResultGeneric<string> result = Host.Program.ExtractLanguageCode("TranslateToNaturalLanguage", "{}");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ExtractLanguageCode_InvalidJson_ReturnsFail()
    {
        MultiLingualCode.Core.Models.OperationResultGeneric<string> result = Host.Program.ExtractLanguageCode("TranslateToNaturalLanguage", "not json");

        Assert.False(result.IsSuccess);
    }

    // === HandleGetKeywordMap ===

    [Fact]
    public async Task HandleGetKeywordMap_ValidRequest_ReturnsReversedMap()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.GetKeywordMapRequest request = new Host.GetKeywordMapRequest
        {
            FileExtension = ".cs",
            TargetLanguage = "pt-br"
        };

        Host.CoreResponse result = await Host.Program.HandleGetKeywordMap(orchestrator, request);

        Assert.True(result.Success, result.Error);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.NotEmpty(map);
        Assert.True(map.ContainsValue("public") || map.ContainsValue("class") || map.ContainsValue("if"));
    }

    [Fact]
    public async Task HandleGetKeywordMap_InvalidExtension_ReturnsError()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.GetKeywordMapRequest request = new Host.GetKeywordMapRequest
        {
            FileExtension = ".xyz",
            TargetLanguage = "pt-br"
        };

        Host.CoreResponse result = await Host.Program.HandleGetKeywordMap(orchestrator, request);

        Assert.False(result.Success);
    }

    // === HandleGetIdentifierMap ===

    [Fact]
    public void HandleGetIdentifierMap_EmptyIdentifiers_ReturnsEmptyMap()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.GetIdentifierMapRequest request = new Host.GetIdentifierMapRequest
        {
            TargetLanguage = "pt-br"
        };

        Host.CoreResponse result = Host.Program.HandleGetIdentifierMap(orchestrator, request);

        Assert.True(result.Success, result.Error);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.Empty(map);
    }

    [Fact]
    public async Task HandleGetIdentifierMap_AfterTranslation_ReturnsPopulatedMap()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);

        // Translate code with tradu annotations to populate identifier map
        string codeWithTradu = "public class Calculator { // tradu[pt-br]:Calculadora\n  public int Add(int a, int b) { return a + b; } // tradu[pt-br]:Somar\n}";
        Host.TranslateRequest translateReq = new Host.TranslateRequest
        {
            SourceCode = codeWithTradu,
            FileExtension = ".cs",
            TargetLanguage = "pt-br"
        };
        Host.CoreResponse translateResult = await Host.Program.HandleTranslateToNaturalLanguage(orchestrator, translateReq);
        Assert.True(translateResult.Success, translateResult.Error);

        Host.GetIdentifierMapRequest request = new Host.GetIdentifierMapRequest
        {
            TargetLanguage = "pt-br"
        };

        Host.CoreResponse result = Host.Program.HandleGetIdentifierMap(orchestrator, request);

        Assert.True(result.Success, result.Error);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.NotEmpty(map);
    }

    // === RouteRequest for GetKeywordMap and GetIdentifierMap ===

    [Fact]
    public async Task RouteRequest_GetKeywordMap_ReturnsKeywordMap()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("GetKeywordMap", paramsJson);

        Assert.True(result.Success, result.Error);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.NotEmpty(map);
    }

    [Fact]
    public async Task RouteRequest_GetIdentifierMap_ReturnsMap()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("GetIdentifierMap", paramsJson);

        Assert.True(result.Success, result.Error);
    }

    [Fact]
    public async Task RouteRequest_GetKeywordMap_MalformedJson_ReturnsError()
    {
        Host.CoreResponse result = await Route("GetKeywordMap", "not json{{{");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_GetKeywordCategories_ReturnsCategoryMap()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            fileExtension = ".cs"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("GetKeywordCategories", paramsJson);

        Assert.True(result.Success, result.Error);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.NotEmpty(map);
        Assert.Equal("control", map["if"]);
        Assert.Equal("type", map["class"]);
        Assert.Equal("modifier", map["public"]);
        Assert.Equal("literal", map["true"]);
    }

    [Fact]
    public async Task RouteRequest_GetKeywordCategories_Python_ReturnsCategoryMap()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            fileExtension = ".py"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("GetKeywordCategories", paramsJson);

        Assert.True(result.Success, result.Error);
        Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(result.Result, Host.Program.JsonOptions)!;
        Assert.NotEmpty(map);
        Assert.Equal("control", map["if"]);
        Assert.Equal("type", map["def"]);
        Assert.Equal("literal", map["True"]);
    }

    [Fact]
    public async Task RouteRequest_GetKeywordCategories_InvalidExtension_ReturnsError()
    {
        string paramsJson = JsonSerializer.Serialize(new
        {
            fileExtension = ".xyz"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await Route("GetKeywordCategories", paramsJson);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_GetIdentifierMap_MalformedJson_ReturnsError()
    {
        Host.CoreResponse result = await Route("GetIdentifierMap", "not json{{{");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_ApplyTranslatedEdits_ReturnsSuccess()
    {
        Dictionary<string, TranslationOrchestrator> cache = new();
        string translateJson = JsonSerializer.Serialize(new
        {
            sourceCode = "public class Foo {}",
            fileExtension = ".cs",
            targetLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse translated = await RouteWithCache("TranslateToNaturalLanguage", translateJson, cache);
        Assert.True(translated.Success);

        string applyJson = JsonSerializer.Serialize(new
        {
            originalCode = "public class Foo {}",
            previousTranslatedCode = translated.Result,
            editedTranslatedCode = translated.Result,
            fileExtension = ".cs",
            sourceLanguage = "pt-br"
        }, Host.Program.JsonOptions);

        Host.CoreResponse result = await RouteWithCache("ApplyTranslatedEdits", applyJson, cache);

        Assert.True(result.Success, result.Error);
    }

    [Fact]
    public async Task RouteRequest_ApplyTranslatedEdits_MalformedJson_ReturnsError()
    {
        Host.CoreResponse result = await Route("ApplyTranslatedEdits", "bad json{");

        Assert.False(result.Success);
    }

    // === ExtractLanguageCode for GetKeywordMap/GetIdentifierMap ===

    [Fact]
    public void ExtractLanguageCode_GetKeywordMap_ReturnsTargetLanguage()
    {
        string paramsJson = JsonSerializer.Serialize(new { targetLanguage = "pt-br" }, Host.Program.JsonOptions);

        MultiLingualCode.Core.Models.OperationResultGeneric<string> result = Host.Program.ExtractLanguageCode("GetKeywordMap", paramsJson);

        Assert.True(result.IsSuccess);
        Assert.Equal("pt-br", result.Value);
    }

    [Fact]
    public void ExtractLanguageCode_GetIdentifierMap_ReturnsTargetLanguage()
    {
        string paramsJson = JsonSerializer.Serialize(new { targetLanguage = "es-es" }, Host.Program.JsonOptions);

        MultiLingualCode.Core.Models.OperationResultGeneric<string> result = Host.Program.ExtractLanguageCode("GetIdentifierMap", paramsJson);

        Assert.True(result.IsSuccess);
        Assert.Equal("es-es", result.Value);
    }

    // === RunPersistent ===

    [Fact]
    public async Task RunPersistent_QuitCommand_ReturnsZero()
    {
        StringReader input = new StringReader("{\"method\":\"quit\"}\n");
        Console.SetIn(input);

        int exitCode = await Host.Program.RunPersistent(TranslationsPath, TempDir);

        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task RunPersistent_TranslateRequest_WritesResponseToStdout()
    {
        string request = JsonSerializer.Serialize(new
        {
            method = "GetSupportedLanguages",
            @params = new { }
        });
        StringReader input = new StringReader(request + "\n{\"method\":\"quit\"}\n");
        StringWriter output = new StringWriter();
        Console.SetIn(input);
        Console.SetOut(output);

        int exitCode = await Host.Program.RunPersistent(TranslationsPath, TempDir);

        Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(0, exitCode);
        Assert.Contains("pt-br", output.ToString());
    }

    [Fact]
    public async Task RunPersistent_InvalidJson_WritesErrorResponse()
    {
        StringReader input = new StringReader("not json\n{\"method\":\"quit\"}\n");
        StringWriter output = new StringWriter();
        Console.SetIn(input);
        Console.SetOut(output);

        int exitCode = await Host.Program.RunPersistent(TranslationsPath, TempDir);

        Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        Assert.Equal(0, exitCode);
        Assert.Contains("false", output.ToString());
    }

    [Fact]
    public async Task RunPersistent_EmptyLine_SkipsWithoutError()
    {
        StringReader input = new StringReader("\n\n{\"method\":\"quit\"}\n");
        Console.SetIn(input);

        int exitCode = await Host.Program.RunPersistent(TranslationsPath, TempDir);

        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Assert.Equal(0, exitCode);
    }

    // === Error paths ===

    [Fact]
    public async Task RouteRequest_TranslateWithMalformedJson_ReturnsError()
    {
        Host.CoreResponse result = await Route("TranslateToNaturalLanguage",
            "{\"targetLanguage\":\"pt-br\",\"sourceCode\":\"x\",\"fileExtension\":}");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_TranslateFromWithMalformedJson_ReturnsError()
    {
        Host.CoreResponse result = await Route("TranslateFromNaturalLanguage",
            "{\"sourceLanguage\":\"pt-br\",bad}");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RouteRequest_ValidateSyntaxWithMalformedJson_ReturnsError()
    {
        Host.CoreResponse result = await Route("ValidateSyntax", "not json");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task HandleTranslateToNaturalLanguage_InvalidExtension_ReturnsError()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.TranslateRequest request = new Host.TranslateRequest
        {
            SourceCode = "code",
            FileExtension = ".xyz",
            TargetLanguage = "pt-br"
        };

        Host.CoreResponse result = await Host.Program.HandleTranslateToNaturalLanguage(orchestrator, request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task HandleTranslateFromNaturalLanguage_InvalidExtension_ReturnsError()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);
        Host.ReverseTranslateRequest request = new Host.ReverseTranslateRequest
        {
            TranslatedCode = "code",
            FileExtension = ".xyz",
            SourceLanguage = "pt-br"
        };

        Host.CoreResponse result = await Host.Program.HandleTranslateFromNaturalLanguage(orchestrator, request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task HandleApplyTranslatedEdits_ValidInput_ReturnsSuccess()
    {
        TranslationOrchestrator orchestrator = Host.Program.CreateOrchestrator("pt-br", TranslationsPath, TempDir);

        Host.CoreResponse translated = await Host.Program.HandleTranslateToNaturalLanguage(orchestrator,
            new Host.TranslateRequest { SourceCode = "public class Foo {}", FileExtension = ".cs", TargetLanguage = "pt-br" });
        Assert.True(translated.Success);

        Host.ApplyEditsRequest request = new Host.ApplyEditsRequest
        {
            OriginalCode = "public class Foo {}",
            PreviousTranslatedCode = translated.Result,
            EditedTranslatedCode = translated.Result,
            FileExtension = ".cs",
            SourceLanguage = "pt-br"
        };

        Host.CoreResponse result = await Host.Program.HandleApplyTranslatedEdits(orchestrator, request);

        Assert.True(result.Success, result.Error);
    }

    [Fact]
    public void HandleValidateSyntax_InvalidExtension_ReturnsError()
    {
        Host.CoreResponse result = Host.Program.HandleValidateSyntax(
            new Host.ValidateRequest { SourceCode = "x", FileExtension = ".xyz" }, Registry);

        Assert.False(result.Success);
    }
}
