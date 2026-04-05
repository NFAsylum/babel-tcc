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
/// Supports two modes: single-request (CLI args) and persistent (stdin/stdout JSON Lines).
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
    /// Application entry point. Detects mode based on CLI arguments:
    /// - If --method is present: single-request mode (process one request and exit)
    /// - Otherwise: persistent mode (read JSON Lines from stdin, respond on stdout)
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string translationsPath = "";
        string projectPath = "";
        string method = "";
        string paramsJson = "{}";

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

        if (string.IsNullOrEmpty(translationsPath))
        {
            translationsPath = Path.Combine(AppContext.BaseDirectory, "translations");
        }

        if (!string.IsNullOrEmpty(method))
        {
            return await RunSingleRequest(method, paramsJson, translationsPath, projectPath);
        }

        return await RunPersistent(translationsPath, projectPath);
    }

    /// <summary>
    /// Single-request mode: process one request from CLI args and exit.
    /// </summary>
    public static async Task<int> RunSingleRequest(
        string method, string paramsJson, string translationsPath, string projectPath)
    {
        CoreResponse coreResponse = await ExecuteMethod(method, paramsJson, translationsPath, projectPath);
        string json = JsonSerializer.Serialize(coreResponse, JsonOptions);
        Console.WriteLine(json);
        return coreResponse.Success ? 0 : 1;
    }

    /// <summary>
    /// Persistent mode: read JSON Lines from stdin, process each request, respond on stdout.
    /// The orchestrator is created once and reused across all requests.
    /// </summary>
    public static async Task<int> RunPersistent(string translationsPath, string projectPath)
    {
        Dictionary<string, TranslationOrchestrator> orchestratorCache = new();
        string? line;

        while ((line = Console.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            CoreResponse response;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(line);
                JsonElement root = doc.RootElement;

                string requestMethod = root.GetProperty("method").GetString() ?? "";

                if (requestMethod == "quit")
                {
                    break;
                }

                string requestParams = root.TryGetProperty("params", out JsonElement paramsElement)
                    ? paramsElement.GetRawText()
                    : "{}";

                response = await ExecuteMethodPersistent(
                    requestMethod, requestParams, translationsPath, projectPath, orchestratorCache);
            }
            catch (Exception ex)
            {
                response = new CoreResponse { Success = false, Error = $"Failed to process request: {ex.Message}" };
            }

            string responseJson = JsonSerializer.Serialize(response, JsonOptions);
            Console.WriteLine(responseJson);
            Console.Out.Flush();
        }

        return 0;
    }

    /// <summary>
    /// Routes a method in persistent mode, reusing cached orchestrators by language code.
    /// </summary>
    public static async Task<CoreResponse> ExecuteMethodPersistent(
        string method, string paramsJson, string translationsPath, string projectPath,
        Dictionary<string, TranslationOrchestrator> orchestratorCache)
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
                    TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
                        orchestratorCache, request.TargetLanguage, translationsPath, projectPath);
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
                    TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
                        orchestratorCache, request.SourceLanguage, translationsPath, projectPath);
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

            case "ApplyTranslatedEdits":
                {
                    OperationResultGeneric<ApplyEditsRequest> parseResult = JsonFileReader.ReadFromString<ApplyEditsRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    ApplyEditsRequest request = parseResult.Value;
                    TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
                        orchestratorCache, request.SourceLanguage, translationsPath, projectPath);
                    return await HandleApplyTranslatedEdits(orchestrator, request);
                }

            case "GetKeywordMap":
                {
                    OperationResultGeneric<GetKeywordMapRequest> parseResult = JsonFileReader.ReadFromString<GetKeywordMapRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    GetKeywordMapRequest request = parseResult.Value;
                    TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
                        orchestratorCache, request.TargetLanguage, translationsPath, projectPath);
                    return await HandleGetKeywordMap(orchestrator, request);
                }

            case "GetIdentifierMap":
                {
                    OperationResultGeneric<GetIdentifierMapRequest> parseResult = JsonFileReader.ReadFromString<GetIdentifierMapRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    GetIdentifierMapRequest request = parseResult.Value;
                    TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
                        orchestratorCache, request.TargetLanguage, translationsPath, projectPath);
                    return HandleGetIdentifierMap(orchestrator, request);
                }

            case "GetSupportedLanguages":
                return HandleGetSupportedLanguages(translationsPath);

            default:
                return new CoreResponse { Success = false, Error = $"Unknown method: {method}" };
        }
    }

    /// <summary>
    /// Returns a cached orchestrator for the given language, or creates and caches a new one.
    /// </summary>
    public static TranslationOrchestrator GetOrCreateOrchestrator(
        Dictionary<string, TranslationOrchestrator> cache, string languageCode,
        string translationsPath, string projectPath)
    {
        if (cache.TryGetValue(languageCode, out TranslationOrchestrator? cached))
        {
            return cached;
        }

        TranslationOrchestrator orchestrator = CreateOrchestrator(languageCode, translationsPath, projectPath);
        cache[languageCode] = orchestrator;
        return orchestrator;
    }

    /// <summary>
    /// Creates a shared language registry with all supported adapters.
    /// </summary>
    public static LanguageRegistry CreateRegistry()
    {
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(new CSharpAdapter());
        registry.RegisterAdapter(new PythonAdapter());
        return registry;
    }

    /// <summary>
    /// Routes a method name to its handler, deserializes parameters, and returns the response.
    /// </summary>
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
    public static TranslationOrchestrator CreateOrchestrator(string languageCode, string translationsPath, string projectPath)
    {
        LanguageRegistry registry = CreateRegistry();
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
    /// Returns the keyword translation map (translated -> original) for a language pair.
    /// </summary>
    public static async Task<CoreResponse> HandleGetKeywordMap(
        TranslationOrchestrator orchestrator, GetKeywordMapRequest request)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = orchestrator.Registry.GetAdapter(request.FileExtension);
        if (!adapterResult.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = adapterResult.ErrorMessage };
        }

        ILanguageAdapter adapter = adapterResult.Value;

        OperationResult loadResult = await orchestrator.Provider.LoadTranslationTableAsync(adapter.LanguageName);
        if (!loadResult.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = loadResult.ErrorMessage };
        }

        // Build reversed map: translated keyword -> original keyword
        Dictionary<string, int> keywordMap = adapter.GetKeywordMap();
        Dictionary<string, string> reversedMap = new();

        foreach (KeyValuePair<string, int> kvp in keywordMap)
        {
            OperationResultGeneric<string> translatedResult = orchestrator.Provider.TranslateKeyword(kvp.Value);
            if (translatedResult.IsSuccess)
            {
                reversedMap[translatedResult.Value] = kvp.Key;
            }
        }

        string mapJson = JsonSerializer.Serialize(reversedMap, JsonOptions);
        return new CoreResponse { Success = true, Result = mapJson };
    }

    /// <summary>
    /// Returns the identifier translation map (translated -> original) for a target language.
    /// Reads from the IdentifierMapper loaded for the current project.
    /// </summary>
    public static CoreResponse HandleGetIdentifierMap(
        TranslationOrchestrator orchestrator, GetIdentifierMapRequest request)
    {
        IdentifierMapper mapper = orchestrator.IdentifierMapperService;
        Dictionary<string, string> reversedMap = new();

        foreach (KeyValuePair<string, Dictionary<string, string>> kvp in mapper.Data.Identifiers)
        {
            string originalName = kvp.Key;
            if (kvp.Value.TryGetValue(request.TargetLanguage, out string? translatedName)
                && !string.IsNullOrEmpty(translatedName))
            {
                reversedMap[translatedName] = originalName;
            }
        }

        string mapJson = JsonSerializer.Serialize(reversedMap, JsonOptions);
        return new CoreResponse { Success = true, Result = mapJson };
    }

    /// <summary>
    /// Handles applying user edits from translated code back to original using 3-way diff.
    /// </summary>
    public static async Task<CoreResponse> HandleApplyTranslatedEdits(
        TranslationOrchestrator orchestrator, ApplyEditsRequest request)
    {
        OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
            request.OriginalCode, request.PreviousTranslatedCode, request.EditedTranslatedCode,
            request.FileExtension, request.SourceLanguage);

        if (!result.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = result.ErrorMessage };
        }

        return new CoreResponse { Success = true, Result = result.Value };
    }

    /// <summary>
    /// Handles validating source code syntax and returning diagnostics.
    /// </summary>
    public static CoreResponse HandleValidateSyntax(ValidateRequest request)
    {
        LanguageRegistry registry = CreateRegistry();

        OperationResultGeneric<ILanguageAdapter> adapterResult = registry.GetAdapter(request.FileExtension);
        if (!adapterResult.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = adapterResult.ErrorMessage };
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
