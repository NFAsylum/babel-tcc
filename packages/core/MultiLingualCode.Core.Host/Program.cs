using System.Text.Json;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Host;

public class Program
{
    public static List<string> SupportedLanguages = new List<string> { "pt-br" };

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

        try
        {
            OperationResultGeneric<CoreResponse> result = await ExecuteMethod(method, paramsJson, translationsPath, projectPath);

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
        catch (ArgumentException ex)
        {
            CoreResponse errorResponse = new CoreResponse
            {
                Success = false,
                Error = ex.Message
            };
            string json = JsonSerializer.Serialize(errorResponse, JsonOptions);
            Console.WriteLine(json);
            return 1;
        }
    }

    public static async Task<OperationResultGeneric<CoreResponse>> ExecuteMethod(
        string method, string paramsJson, string translationsPath, string projectPath)
    {
        switch (method)
        {
            case "TranslateToNaturalLanguage":
            {
                TranslateRequest request = JsonSerializer.Deserialize<TranslateRequest>(paramsJson, JsonOptions)
                    ?? new TranslateRequest();
                TranslationOrchestrator orchestrator = CreateOrchestrator(request.TargetLanguage, translationsPath, projectPath);
                return await HandleTranslateToNaturalLanguage(orchestrator, request);
            }

            case "TranslateFromNaturalLanguage":
            {
                ReverseTranslateRequest request = JsonSerializer.Deserialize<ReverseTranslateRequest>(paramsJson, JsonOptions)
                    ?? new ReverseTranslateRequest();
                TranslationOrchestrator orchestrator = CreateOrchestrator(request.SourceLanguage, translationsPath, projectPath);
                return await HandleTranslateFromNaturalLanguage(orchestrator, request);
            }

            case "ValidateSyntax":
                return HandleValidateSyntax(paramsJson);

            case "GetSupportedLanguages":
                return HandleGetSupportedLanguages();

            default:
                return OperationResultGeneric<CoreResponse>.Fail($"Unknown method: {method}");
        }
    }

    public static TranslationOrchestrator CreateOrchestrator(string languageCode, string translationsPath, string projectPath)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);

        if (string.IsNullOrEmpty(translationsPath))
        {
            translationsPath = Path.Combine(AppContext.BaseDirectory, "translations");
        }

        NaturalLanguageProvider provider = new NaturalLanguageProvider(languageCode, translationsPath);

        IdentifierMapper mapper = new IdentifierMapper();
        if (!string.IsNullOrEmpty(projectPath))
        {
            mapper.LoadMap(projectPath);
        }

        return new TranslationOrchestrator(registry, provider, mapper);
    }

    public static async Task<OperationResultGeneric<CoreResponse>> HandleTranslateToNaturalLanguage(
        TranslationOrchestrator orchestrator, TranslateRequest request)
    {
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            request.SourceCode, request.FileExtension, request.TargetLanguage);

        if (!result.IsSuccess)
        {
            return OperationResultGeneric<CoreResponse>.Fail(result.ErrorMessage);
        }

        return OperationResultGeneric<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = result.Value
        });
    }

    public static async Task<OperationResultGeneric<CoreResponse>> HandleTranslateFromNaturalLanguage(
        TranslationOrchestrator orchestrator, ReverseTranslateRequest request)
    {
        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            request.TranslatedCode, request.FileExtension, request.SourceLanguage);

        if (!result.IsSuccess)
        {
            return OperationResultGeneric<CoreResponse>.Fail(result.ErrorMessage);
        }

        return OperationResultGeneric<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = result.Value
        });
    }

    public static OperationResultGeneric<CoreResponse> HandleValidateSyntax(string paramsJson)
    {
        ValidateRequest request = JsonSerializer.Deserialize<ValidateRequest>(paramsJson, JsonOptions)
            ?? new ValidateRequest();

        CSharpAdapter adapter = new CSharpAdapter();
        ValidationResult validation = adapter.ValidateSyntax(request.SourceCode);

        return OperationResultGeneric<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(validation, JsonOptions)
        });
    }

    public static OperationResultGeneric<CoreResponse> HandleGetSupportedLanguages()
    {
        return OperationResultGeneric<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(SupportedLanguages, JsonOptions)
        });
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
