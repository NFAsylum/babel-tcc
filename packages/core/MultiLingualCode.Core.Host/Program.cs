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

    public static string ResolveTranslationsPath(string translationsPath)
    {
        if (string.IsNullOrEmpty(translationsPath))
        {
            return Path.Combine(AppContext.BaseDirectory, "translations");
        }
        return translationsPath;
    }

    public static async Task<OperationResultGeneric<CoreResponse>> ExecuteMethod(
        string method, string paramsJson, string translationsPath, string projectPath)
    {
        string resolvedTranslationsPath = ResolveTranslationsPath(translationsPath);

        switch (method)
        {
            case "TranslateToNaturalLanguage":
            {
                OperationResultGeneric<TranslateRequest> parseResult = DeserializeJson<TranslateRequest>(paramsJson);
                if (!parseResult.IsSuccess)
                {
                    return OperationResultGeneric<CoreResponse>.Fail(parseResult.ErrorMessage);
                }
                TranslateRequest request = parseResult.Value;
                TranslationOrchestrator orchestrator = CreateOrchestrator(request.TargetLanguage, resolvedTranslationsPath, projectPath);
                return await HandleTranslateToNaturalLanguage(orchestrator, request);
            }

            case "TranslateFromNaturalLanguage":
            {
                OperationResultGeneric<ReverseTranslateRequest> parseResult = DeserializeJson<ReverseTranslateRequest>(paramsJson);
                if (!parseResult.IsSuccess)
                {
                    return OperationResultGeneric<CoreResponse>.Fail(parseResult.ErrorMessage);
                }
                ReverseTranslateRequest request = parseResult.Value;
                TranslationOrchestrator orchestrator = CreateOrchestrator(request.SourceLanguage, resolvedTranslationsPath, projectPath);
                return await HandleTranslateFromNaturalLanguage(orchestrator, request);
            }

            case "ValidateSyntax":
                return HandleValidateSyntax(paramsJson);

            case "GetSupportedLanguages":
                return HandleGetSupportedLanguages(resolvedTranslationsPath);

            default:
                return OperationResultGeneric<CoreResponse>.Fail($"Unknown method: {method}");
        }
    }

    public static TranslationOrchestrator CreateOrchestrator(string languageCode, string resolvedTranslationsPath, string projectPath)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);

        NaturalLanguageProvider provider = new NaturalLanguageProvider(languageCode, resolvedTranslationsPath);

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
        OperationResultGeneric<ValidateRequest> parseResult = DeserializeJson<ValidateRequest>(paramsJson);
        if (!parseResult.IsSuccess)
        {
            return OperationResultGeneric<CoreResponse>.Fail(parseResult.ErrorMessage);
        }

        CSharpAdapter adapter = new CSharpAdapter();
        ValidationResult validation = adapter.ValidateSyntax(parseResult.Value.SourceCode);

        return OperationResultGeneric<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(validation, JsonOptions)
        });
    }

    public static OperationResultGeneric<CoreResponse> HandleGetSupportedLanguages(string translationsPath)
    {
        string naturalLanguagesPath = Path.Combine(translationsPath, "natural-languages");
        List<string> languages = new List<string>();

        if (Directory.Exists(naturalLanguagesPath))
        {
            foreach (string dir in Directory.GetDirectories(naturalLanguagesPath))
            {
                languages.Add(Path.GetFileName(dir));
            }
        }

        return OperationResultGeneric<CoreResponse>.Ok(new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(languages, JsonOptions)
        });
    }

    // I/O boundary: JsonSerializer.Deserialize retorna nullable por API do .NET
    public static OperationResultGeneric<T> DeserializeJson<T>(string json) where T : class
    {
        try
        {
            if (JsonSerializer.Deserialize(json, typeof(T), JsonOptions) is T typed)
            {
                return OperationResultGeneric<T>.Ok(typed);
            }
            return OperationResultGeneric<T>.Fail("Failed to deserialize JSON request.");
        }
        catch (JsonException ex)
        {
            return OperationResultGeneric<T>.Fail($"Invalid JSON: {ex.Message}");
        }
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
