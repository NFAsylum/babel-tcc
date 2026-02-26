using System.Text.Json;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Service that manages identifier translation mappings and their persistence to disk.
/// </summary>
public class IdentifierMapper
{
    /// <summary>
    /// The directory name where identifier map files are stored within a project.
    /// </summary>
    public const string MapDirectory = ".multilingual";

    /// <summary>
    /// The file name for the identifier map JSON file.
    /// </summary>
    public const string MapFileName = "identifier-map.json";

    /// <summary>
    /// The in-memory identifier map data containing identifiers and literals.
    /// </summary>
    public IdentifierMapData Data = new();

    /// <summary>
    /// The file path from which the current map was loaded.
    /// </summary>
    public string LoadedPath = "";

    /// <summary>
    /// Loads the identifier map from disk synchronously. Creates a new empty map if the file does not exist.
    /// </summary>
    /// <param name="projectPath">The root path of the project containing the map file.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public OperationResult LoadMap(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
        {
            return OperationResult.Fail("Project path cannot be empty.");
        }

        string filePath = GetMapFilePath(projectPath);
        LoadedPath = filePath;

        if (!File.Exists(filePath))
        {
            Data = new IdentifierMapData();
            return OperationResult.Ok();
        }

        OperationResultGeneric<IdentifierMapData> result = JsonFileReader.ReadFromFile<IdentifierMapData>(filePath, JsonSerializerReadOptions);
        if (result.IsSuccess)
        {
            Data = result.Value;
        }
        else
        {
            Data = new IdentifierMapData();
        }

        return OperationResult.Ok();
    }

    /// <summary>
    /// Loads the identifier map from disk asynchronously. Creates a new empty map if the file does not exist.
    /// </summary>
    /// <param name="projectPath">The root path of the project containing the map file.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public async Task<OperationResult> LoadMapAsync(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
        {
            return OperationResult.Fail("Project path cannot be empty.");
        }

        string filePath = GetMapFilePath(projectPath);
        LoadedPath = filePath;

        if (!File.Exists(filePath))
        {
            Data = new IdentifierMapData();
            return OperationResult.Ok();
        }

        OperationResultGeneric<IdentifierMapData> result = await JsonFileReader.ReadFromFileAsync<IdentifierMapData>(filePath, JsonSerializerReadOptions);
        if (result.IsSuccess)
        {
            Data = result.Value;
        }
        else
        {
            Data = new IdentifierMapData();
        }

        return OperationResult.Ok();
    }

    /// <summary>
    /// Gets the translated name for an identifier in the specified target language.
    /// </summary>
    /// <param name="identifier">The original identifier name.</param>
    /// <param name="targetLanguage">The target natural language code.</param>
    /// <returns>An operation result containing the translated name, or a failure if not found.</returns>
    public OperationResultGeneric<string> GetTranslation(string identifier, string targetLanguage)
    {
        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(targetLanguage))
        {
            return OperationResultGeneric<string>.Fail("Identifier or target language is empty.");
        }

        if (Data.Identifiers.ContainsKey(identifier)
            && Data.Identifiers[identifier].ContainsKey(targetLanguage))
        {
            return OperationResultGeneric<string>.Ok(Data.Identifiers[identifier][targetLanguage]);
        }

        return OperationResultGeneric<string>.Fail($"No translation found for identifier: {identifier}");
    }

    /// <summary>
    /// Gets the original identifier name from a translated identifier in the specified source language.
    /// </summary>
    /// <param name="translatedIdentifier">The translated identifier name.</param>
    /// <param name="sourceLanguage">The source natural language code.</param>
    /// <returns>An operation result containing the original identifier, or a failure if not found.</returns>
    public OperationResultGeneric<string> GetOriginal(string translatedIdentifier, string sourceLanguage)
    {
        if (string.IsNullOrEmpty(translatedIdentifier) || string.IsNullOrEmpty(sourceLanguage))
        {
            return OperationResultGeneric<string>.Fail("Translated identifier or source language is empty.");
        }

        foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data.Identifiers)
        {
            if (kvp.Value.ContainsKey(sourceLanguage)
                && string.Equals(kvp.Value[sourceLanguage], translatedIdentifier, StringComparison.Ordinal))
            {
                return OperationResultGeneric<string>.Ok(kvp.Key);
            }
        }

        return OperationResultGeneric<string>.Fail($"No original found for: {translatedIdentifier}");
    }

    /// <summary>
    /// Gets the translated value for a literal in the specified target language.
    /// </summary>
    /// <param name="literal">The original literal value.</param>
    /// <param name="targetLanguage">The target natural language code.</param>
    /// <returns>An operation result containing the translated literal, or a failure if not found.</returns>
    public OperationResultGeneric<string> GetLiteralTranslation(string literal, string targetLanguage)
    {
        if (string.IsNullOrEmpty(literal) || string.IsNullOrEmpty(targetLanguage))
        {
            return OperationResultGeneric<string>.Fail("Literal or target language is empty.");
        }

        if (Data.Literals.ContainsKey(literal)
            && Data.Literals[literal].ContainsKey(targetLanguage))
        {
            return OperationResultGeneric<string>.Ok(Data.Literals[literal][targetLanguage]);
        }

        return OperationResultGeneric<string>.Fail($"No translation found for literal: {literal}");
    }

    /// <summary>
    /// Adds or updates the translation of an identifier for a given target language.
    /// </summary>
    /// <param name="identifier">The original identifier name.</param>
    /// <param name="targetLanguage">The target natural language code.</param>
    /// <param name="translatedName">The translated identifier name.</param>
    public void SetTranslation(string identifier, string targetLanguage, string translatedName)
    {
        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(translatedName))
        {
            return;
        }

        if (!Data.Identifiers.ContainsKey(identifier))
        {
            Data.Identifiers[identifier] = new Dictionary<string, string>();
        }

        Data.Identifiers[identifier][targetLanguage] = translatedName;
    }

    /// <summary>
    /// Adds or updates the translation of a literal value for a given target language.
    /// </summary>
    /// <param name="literal">The original literal value.</param>
    /// <param name="targetLanguage">The target natural language code.</param>
    /// <param name="translatedLiteral">The translated literal value.</param>
    public void SetLiteralTranslation(string literal, string targetLanguage, string translatedLiteral)
    {
        if (string.IsNullOrEmpty(literal) || string.IsNullOrEmpty(targetLanguage) || string.IsNullOrEmpty(translatedLiteral))
        {
            return;
        }

        if (!Data.Literals.ContainsKey(literal))
        {
            Data.Literals[literal] = new Dictionary<string, string>();
        }

        Data.Literals[literal][targetLanguage] = translatedLiteral;
    }

    /// <summary>
    /// Removes an identifier and all its translations from the map.
    /// </summary>
    /// <param name="identifier">The original identifier name to remove.</param>
    /// <returns>True if the identifier was found and removed; false otherwise.</returns>
    public bool RemoveIdentifier(string identifier)
    {
        return Data.Identifiers.Remove(identifier);
    }

    /// <summary>
    /// Saves the current identifier map to disk as JSON. Uses the loaded path if no project path is specified.
    /// </summary>
    /// <param name="projectPath">Optional project root path. If empty, uses the previously loaded path.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public OperationResult SaveMap(string projectPath = "")
    {
        string filePath;
        if (!string.IsNullOrEmpty(projectPath))
        {
            filePath = GetMapFilePath(projectPath);
        }
        else if (!string.IsNullOrEmpty(LoadedPath))
        {
            filePath = LoadedPath;
        }
        else
        {
            return OperationResult.Fail("No project path set. Call LoadMap first or provide a projectPath.");
        }

        return JsonFileReader.WriteToFile(filePath, Data, WriteOptions);
    }

    /// <summary>
    /// Gets the number of identifiers currently stored in the map.
    /// </summary>
    public int IdentifierCount => Data.Identifiers.Count;

    /// <summary>
    /// Gets the number of literals currently stored in the map.
    /// </summary>
    public int LiteralCount => Data.Literals.Count;

    /// <summary>
    /// Builds the full file path for the identifier map within a given project directory.
    /// </summary>
    /// <param name="projectPath">The root path of the project.</param>
    /// <returns>The full path to the identifier map JSON file.</returns>
    public static string GetMapFilePath(string projectPath)
    {
        return Path.Combine(projectPath, MapDirectory, MapFileName);
    }

    /// <summary>
    /// JSON serializer options used when reading identifier map files from disk.
    /// </summary>
    public static readonly JsonSerializerOptions JsonSerializerReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// JSON serializer options used when writing identifier map files to disk.
    /// </summary>
    public static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };
}
