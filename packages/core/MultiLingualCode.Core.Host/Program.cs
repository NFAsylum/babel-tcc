using System.Text.Json;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.LanguageAdapters.Python;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Host;

/// <summary>
/// CLI host entry point that dispatches translation and validation requests to the core engine.
/// </summary>
public class Program
{
    /// <summary>
    /// Shared JSON serializer options using camelCase naming for CLI input/output.
    /// </summary>
    public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Application entry point. Parses CLI arguments and dispatches to the appropriate handler method.
    /// </summary>
    /// <param name="args">Command-line arguments: --method, --params, --translations, --project.</param>
    /// <returns>Exit code 0 on success, 1 on failure.</returns>
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

    /// <summary>
    /// Routes a method name to its handler, deserializes parameters, and returns the response.
    /// </summary>
    /// <param name="method">The method name to execute (e.g. "TranslateToNaturalLanguage").</param>
    /// <param name="paramsJson">JSON string containing the request parameters.</param>
    /// <param name="translationsPath">Path to the translations directory.</param>
    /// <param name="projectPath">Path to the project root for identifier mapping.</param>
    /// <returns>A response indicating success or failure with result data or error message.</returns>
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

    /// <summary>
    /// Creates and configures a translation orchestrator with all language adapters, language provider, and identifier mapper.
    /// </summary>
    /// <param name="languageCode">The natural language code (e.g. "pt-br").</param>
    /// <param name="translationsPath">Path to the translations base directory.</param>
    /// <param name="projectPath">Path to the project root for loading identifier maps.</param>
    /// <returns>A fully configured translation orchestrator.</returns>
    public static TranslationOrchestrator CreateOrchestrator(string languageCode, string translationsPath, string projectPath)
    {
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(new CSharpAdapter());
        registry.RegisterAdapter(new PythonAdapter());

        NaturalLanguageProvider provider = new NaturalLanguageProvider { LanguageCode = languageCode, TranslationsBasePath = translationsPath };

        IdentifierMapper mapper = new IdentifierMapper();
        if (!string.IsNullOrEmpty(projectPath))
        {
            mapper.LoadMap(projectPath);
        }

        return new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };
    }

    /// <summary>
    /// Handles translating source code from a programming language to a natural language.
    /// </summary>
    /// <param name="orchestrator">The configured translation orchestrator.</param>
    /// <param name="request">The translation request containing source code and target language.</param>
    /// <returns>A response containing the translated code or an error message.</returns>
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

    /// <summary>
    /// Handles reverse-translating code from a natural language back to the original programming language.
    /// </summary>
    /// <param name="orchestrator">The configured translation orchestrator.</param>
    /// <param name="request">The reverse translation request containing translated code and source language.</param>
    /// <returns>A response containing the restored original code or an error message.</returns>
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

    /// <summary>
    /// Handles validating source code syntax and returning diagnostics.
    /// Uses the LanguageRegistry to resolve the correct adapter for the file extension.
    /// </summary>
    /// <param name="request">The validation request containing the source code to check.</param>
    /// <returns>A response containing the serialized validation result.</returns>
    public static CoreResponse HandleValidateSyntax(ValidateRequest request)
    {
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(new CSharpAdapter());
        registry.RegisterAdapter(new PythonAdapter());

        OperationResultGeneric<ILanguageAdapter> adapterResult = registry.GetAdapter(request.FileExtension);
        if (!adapterResult.IsSuccess)
        {
            return new CoreResponse
            {
                Success = false,
                Error = adapterResult.ErrorMessage
            };
        }

        ValidationResult validation = adapterResult.Value.ValidateSyntax(request.SourceCode);

        return new CoreResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(validation, JsonOptions)
        };
    }

    /// <summary>
    /// Returns the list of supported natural languages by scanning the translations directory.
    /// </summary>
    /// <param name="translationsPath">Path to the translations base directory.</param>
    /// <returns>A response containing a serialized list of supported language codes.</returns>
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

    /// <summary>
    /// Writes an error response as JSON to stderr.
    /// </summary>
    /// <param name="message">The error message to include in the response.</param>
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
