using System.Text.Json;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Host;

public class Program
{
    public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            WriteError("Usage: dotnet MultiLingualCode.Core.Host.dll --method <method> [--params <json>] [--translations <path>] [--project <path>]");
            return 1;
        }

        string method = "";
        string paramsJson = "{}";
        string translationsPath = "";
        string projectPath = "";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--method" && i + 1 < args.Length)
            {
                method = args[i + 1];
                i++;
            }
            else if (args[i] == "--params" && i + 1 < args.Length)
            {
                paramsJson = args[i + 1];
                i++;
            }
            else if (args[i] == "--translations" && i + 1 < args.Length)
            {
                translationsPath = args[i + 1];
                i++;
            }
            else if (args[i] == "--project" && i + 1 < args.Length)
            {
                projectPath = args[i + 1];
                i++;
            }
        }

        if (string.IsNullOrEmpty(method))
        {
            WriteError("Method is required. Use --method <method>");
            return 1;
        }

        OperationResult<CoreResponse> result = await ExecuteMethod(method, paramsJson, translationsPath, projectPath);

        if (result.IsSuccess)
        {
            string json = JsonSerializer.Serialize(result.Value, JsonOptions);
            Console.WriteLine(json);
            return 0;
        }
        else
        {
            CoreResponse errorResponse = new CoreResponse
            {
                Success = false,
                Error = result.ErrorMessage
            };
            string json = JsonSerializer.Serialize(errorResponse, JsonOptions);
            Console.WriteLine(json);
            return 1;
        }
    }

    public static async Task<OperationResult<CoreResponse>> ExecuteMethod(
        string method, string paramsJson, string translationsPath, string projectPath)
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator(paramsJson, translationsPath, projectPath);

        switch (method)
        {
            case "TranslateToNaturalLanguage":
                return await HandleTranslateToNaturalLanguage(orchestrator, paramsJson);

            case "TranslateFromNaturalLanguage":
                return await HandleTranslateFromNaturalLanguage(orchestrator, paramsJson);

            case "ValidateSyntax":
                return HandleValidateSyntax(paramsJson);

            case "GetSupportedLanguages":
                return HandleGetSupportedLanguages();

            default:
                return OperationResult<CoreResponse>.Fail($"Unknown method: {method}");
        }
    }

    public static TranslationOrchestrator CreateOrchestrator(string paramsJson, string translationsPath, string projectPath)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);

        if (string.IsNullOrEmpty(translationsPath))
        {
            translationsPath = Path.Combine(AppContext.BaseDirectory, "translations");
        }

        string languageCode = ExtractLanguageCode(paramsJson);
        NaturalLanguageProvider provider = new NaturalLanguageProvider(languageCode, translationsPath);

        IdentifierMapper mapper = new IdentifierMapper();
        if (!string.IsNullOrEmpty(projectPath))
        {
            mapper.LoadMap(projectPath);
        }

        return new TranslationOrchestrator(registry, provider, mapper);
    }

    public static async Task<OperationResult<CoreResponse>> HandleTranslateToNaturalLanguage(
        TranslationOrchestrator orchestrator, string paramsJson)
    {
        TranslateRequest request = JsonSerializer.Deserialize<TranslateRequest>(paramsJson, JsonOptions)
            ?? new TranslateRequest();

        OperationResult<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            request.SourceCode, request.FileExtension, request.TargetLanguage);

        if (!result.IsSuccess)
        {
            return OperationResult<CoreResponse>.Fail(result.ErrorMessage);
        }

        return OperationResult<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = result.Value
        });
    }

    public static async Task<OperationResult<CoreResponse>> HandleTranslateFromNaturalLanguage(
        TranslationOrchestrator orchestrator, string paramsJson)
    {
        ReverseTranslateRequest request = JsonSerializer.Deserialize<ReverseTranslateRequest>(paramsJson, JsonOptions)
            ?? new ReverseTranslateRequest();

        OperationResult<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            request.TranslatedCode, request.FileExtension, request.SourceLanguage);

        if (!result.IsSuccess)
        {
            return OperationResult<CoreResponse>.Fail(result.ErrorMessage);
        }

        return OperationResult<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = result.Value
        });
    }

    public static OperationResult<CoreResponse> HandleValidateSyntax(string paramsJson)
    {
        ValidateRequest request = JsonSerializer.Deserialize<ValidateRequest>(paramsJson, JsonOptions)
            ?? new ValidateRequest();

        CSharpAdapter adapter = new CSharpAdapter();
        ValidationResult validation = adapter.ValidateSyntax(request.SourceCode);

        return OperationResult<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(validation, JsonOptions)
        });
    }

    public static OperationResult<CoreResponse> HandleGetSupportedLanguages()
    {
        List<string> languages = new List<string> { "pt-br" };

        return OperationResult<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(languages, JsonOptions)
        });
    }

    public static string ExtractLanguageCode(string paramsJson)
    {
        using JsonDocument doc = JsonDocument.Parse(paramsJson);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("targetLanguage", out JsonElement targetLang)
            && targetLang.ValueKind == JsonValueKind.String)
        {
            return targetLang.GetString() ?? "pt-br";
        }

        if (root.TryGetProperty("sourceLanguage", out JsonElement sourceLang)
            && sourceLang.ValueKind == JsonValueKind.String)
        {
            return sourceLang.GetString() ?? "pt-br";
        }

        return "pt-br";
    }

    public static void WriteError(string message)
    {
        CoreResponse errorResponse = new CoreResponse
        {
            Success = false,
            Error = message
        };
        string json = JsonSerializer.Serialize(errorResponse, JsonOptions);
        Console.Error.WriteLine(json);
    }
}
