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
        LanguageRegistry registry = CreateRegistry();
        Dictionary<string, TranslationOrchestrator> cache = new();
        CoreResponse coreResponse = await RouteRequest(method, paramsJson, registry, translationsPath, projectPath, cache);
        string json = JsonSerializer.Serialize(coreResponse, JsonOptions);
        Console.WriteLine(json);
        return coreResponse.Success ? 0 : 1;
    }

    /// <summary>
    /// Persistent mode: read JSON Lines from stdin, process each request, respond on stdout.
    /// </summary>
    public static async Task<int> RunPersistent(string translationsPath, string projectPath)
    {
        LanguageRegistry registry = CreateRegistry();
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

                response = await RouteRequest(requestMethod, requestParams, registry, translationsPath, projectPath, orchestratorCache);
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
    /// Unified request router. Routes method to handler using two-level dispatch:
    /// Level 1 handles methods that don't need an orchestrator.
    /// Level 2 creates orchestrator on demand for translation methods.
    /// </summary>
    public static async Task<CoreResponse> RouteRequest(
        string method,
        string paramsJson,
        LanguageRegistry registry,
        string translationsPath,
        string projectPath,
        Dictionary<string, TranslationOrchestrator> orchestratorCache)
    {
        // Level 1: methods that don't need an orchestrator
        switch (method)
        {
            case "ValidateSyntax":
                {
                    OperationResultGeneric<ValidateRequest> parseResult = JsonFileReader.ReadFromString<ValidateRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return HandleValidateSyntax(parseResult.Value, registry);
                }

            case "GetSupportedLanguages":
                return HandleGetSupportedLanguages(translationsPath);

            case "GetKeywordCategories":
                {
                    OperationResultGeneric<GetKeywordCategoriesRequest> parseResult = JsonFileReader.ReadFromString<GetKeywordCategoriesRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return HandleGetKeywordCategories(parseResult.Value, registry, translationsPath);
                }
        }

        // Level 2: methods that need an orchestrator (created on demand)
        OperationResultGeneric<string> langResult = ExtractLanguageCode(method, paramsJson);
        if (!langResult.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = langResult.ErrorMessage };
        }

        TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
            orchestratorCache, langResult.Value, translationsPath, projectPath);

        switch (method)
        {
            case "TranslateToNaturalLanguage":
                {
                    OperationResultGeneric<TranslateRequest> parseResult = JsonFileReader.ReadFromString<TranslateRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return await HandleTranslateToNaturalLanguage(orchestrator, parseResult.Value);
                }

            case "TranslateFromNaturalLanguage":
                {
                    OperationResultGeneric<ReverseTranslateRequest> parseResult = JsonFileReader.ReadFromString<ReverseTranslateRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return await HandleTranslateFromNaturalLanguage(orchestrator, parseResult.Value);
                }

            case "ApplyTranslatedEdits":
                {
                    OperationResultGeneric<ApplyEditsRequest> parseResult = JsonFileReader.ReadFromString<ApplyEditsRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return await HandleApplyTranslatedEdits(orchestrator, parseResult.Value);
                }

            case "GetKeywordMap":
                {
                    OperationResultGeneric<GetKeywordMapRequest> parseResult = JsonFileReader.ReadFromString<GetKeywordMapRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return await HandleGetKeywordMap(orchestrator, parseResult.Value);
                }

            case "GetIdentifierMap":
                {
                    OperationResultGeneric<GetIdentifierMapRequest> parseResult = JsonFileReader.ReadFromString<GetIdentifierMapRequest>(paramsJson, JsonOptions);
                    if (!parseResult.IsSuccess)
                    {
                        return new CoreResponse { Success = false, Error = parseResult.ErrorMessage };
                    }
                    return HandleGetIdentifierMap(orchestrator, parseResult.Value);
                }

            default:
                return new CoreResponse { Success = false, Error = $"Unknown method: {method}" };
        }
    }

    /// <summary>
    /// Extracts the language code from a JSON request based on the method name.
    /// </summary>
    public static OperationResultGeneric<string> ExtractLanguageCode(string method, string paramsJson)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(paramsJson);
            JsonElement root = doc.RootElement;

            string propertyName = method == "TranslateToNaturalLanguage"
                || method == "GetKeywordMap"
                || method == "GetIdentifierMap"
                ? "targetLanguage" : "sourceLanguage";

            if (root.TryGetProperty(propertyName, out JsonElement langElement))
            {
                string languageCode = langElement.GetString() ?? "";
                if (!string.IsNullOrEmpty(languageCode))
                {
                    return OperationResultGeneric<string>.Ok(languageCode);
                }
            }

            return OperationResultGeneric<string>.Fail($"Missing or empty '{propertyName}' in request params");
        }
        catch (JsonException ex)
        {
            return OperationResultGeneric<string>.Fail($"Invalid JSON in params: {ex.Message}");
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
    public static CoreResponse HandleValidateSyntax(ValidateRequest request, LanguageRegistry registry)
    {
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
    /// Returns the keyword category map (keyword -> category) for a programming language.
    /// Reads from keyword-categories.json in the translations directory.
    /// </summary>
    public static CoreResponse HandleGetKeywordCategories(
        GetKeywordCategoriesRequest request, LanguageRegistry registry, string translationsPath)
    {
        OperationResultGeneric<ILanguageAdapter> adapterResult = registry.GetAdapter(request.FileExtension);
        if (!adapterResult.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = adapterResult.ErrorMessage };
        }

        string langName = adapterResult.Value.LanguageName.ToLowerInvariant();
        string categoriesPath = Path.Combine(
            translationsPath, "programming-languages", langName, "keyword-categories.json");

        if (!File.Exists(categoriesPath))
        {
            return new CoreResponse { Success = true, Result = "{}" };
        }

        OperationResultGeneric<KeywordCategoryFile> readResult = JsonFileReader.ReadFromFile<KeywordCategoryFile>(categoriesPath, JsonOptions);
        if (!readResult.IsSuccess)
        {
            return new CoreResponse { Success = false, Error = readResult.ErrorMessage };
        }

        string mapJson = JsonSerializer.Serialize(readResult.Value.Categories, JsonOptions);
        return new CoreResponse { Success = true, Result = mapJson };
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
