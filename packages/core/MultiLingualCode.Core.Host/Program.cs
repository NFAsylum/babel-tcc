using System.Text.Json;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;
using MultiLingualCode.Core.Utilities;

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

        if (string.IsNullOrEmpty(translationsPath))
        {
            translationsPath = Path.Combine(AppContext.BaseDirectory, "translations");
        }

        CoreResponse coreResponse = await ExecuteMethod(method, paramsJson, translationsPath, projectPath);
        string json = JsonSerializer.Serialize(coreResponse, JsonOptions);

        if (coreResponse.Success)
        {
            Console.WriteLine(json);
            return 0;
        }
        else
        {
            Console.WriteLine(json);
            return 1;
        }
    }

    public static async Task<CoreResponse> ExecuteMethod(
        string method, string paramsJson, string translationsPath, string projectPath)
    {
        switch (method)
        {
            case "TranslateToNaturalLanguage":
            {
                OperationResultGeneric<TranslateRequest> parseResult = JsonFileReader.ReadFromString<TranslateRequest>(paramsJson, JsonOptions);
                if (!parseResult.IsSuccess)
                {
                    return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                }
                TranslateRequest request = parseResult.Value;
                TranslationOrchestrator orchestrator = CreateOrchestrator(request.TargetLanguage, translationsPath, projectPath);
                return await HandleTranslateToNaturalLanguage(orchestrator, request);
            }

            case "TranslateFromNaturalLanguage":
            {
                OperationResultGeneric<ReverseTranslateRequest> parseResult = JsonFileReader.ReadFromString<ReverseTranslateRequest>(paramsJson, JsonOptions);
                if (!parseResult.IsSuccess)
                {
                    return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                }
                ReverseTranslateRequest request = parseResult.Value;
                TranslationOrchestrator orchestrator = CreateOrchestrator(request.SourceLanguage, translationsPath, projectPath);
                return await HandleTranslateFromNaturalLanguage(orchestrator, request);
            }

            case "ValidateSyntax":
            {
                OperationResultGeneric<ValidateRequest> parseResult = JsonFileReader.ReadFromString<ValidateRequest>(paramsJson, JsonOptions);
                if (!parseResult.IsSuccess)
                {
                    return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                }
                return HandleValidateSyntax(parseResult.Value);
            }

            case "GetSupportedLanguages":
                return HandleGetSupportedLanguages(translationsPath);

            default:
                return new CoreResponse { Success = false, Error = $"Unknown method: {method}" };
        }
    }

    public static TranslationOrchestrator CreateOrchestrator(string languageCode, string translationsPath, string projectPath)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);

        NaturalLanguageProvider provider = new NaturalLanguageProvider { LanguageCode = languageCode, TranslationsBasePath = translationsPath };

        IdentifierMapper mapper = new IdentifierMapper();
        if (!string.IsNullOrEmpty(projectPath))
        {
            mapper.LoadMap(projectPath);
        }

        return new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };
    }

    public static async Task<CoreResponse> HandleTranslateToNaturalLanguage(
        TranslationOrchestrator orchestrator, TranslateRequest request)
    {
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            request.SourceCode, request.FileExtension, request.TargetLanguage);

        if (!result.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = result.ErrorMessage };
        }

        return new CoreResponse { Success = true, Result = result.Value };
    }

    public static async Task<CoreResponse> HandleTranslateFromNaturalLanguage(
        TranslationOrchestrator orchestrator, ReverseTranslateRequest request)
    {
        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            request.TranslatedCode, request.FileExtension, request.SourceLanguage);

        if (!result.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = result.ErrorMessage };
        }

        return new CoreResponse { Success = true, Result = result.Value };
    }

    public static CoreResponse HandleValidateSyntax(ValidateRequest request)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        ValidationResult validation = adapter.ValidateSyntax(request.SourceCode);

        return new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(validation, JsonOptions)
        };
    }

    public static CoreResponse HandleGetSupportedLanguages(string translationsPath)
    {
        string naturalLanguagesPath = Path.Combine(translationsPath, "natural-languages");
        List<string> languages = new List<string>();

        if (Directory.Exists(naturalLanguagesPath))
        {
            foreach (string directory in Directory.GetDirectories(naturalLanguagesPath))
            {
                languages.Add(Path.GetFileName(directory));
            }
        }

        return new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(languages, JsonOptions)
        };
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
