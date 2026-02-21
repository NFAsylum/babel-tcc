using System.Text.Json;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Services;

public class IdentifierMapper
{
    public const string MapDirectory = ".multilingual";
    public const string MapFileName = "identifier-map.json";

    public IdentifierMapData Data = new();
    public string LoadedPath = "";

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

        OperationResult<IdentifierMapData> result = JsonFileReader.ReadFromFile<IdentifierMapData>(filePath, JsonSerializerReadOptions);
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

        OperationResult<IdentifierMapData> result = await JsonFileReader.ReadFromFileAsync<IdentifierMapData>(filePath, JsonSerializerReadOptions);
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

    public OperationResult<string> GetTranslation(string identifier, string targetLanguage)
    {
        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(targetLanguage))
        {
            return OperationResult<string>.Fail("Identifier or target language is empty.");
        }

        if (Data.Identifiers.TryGetValue(identifier, out Dictionary<string, string>? translations)
            && translations is not null
            && translations.TryGetValue(targetLanguage, out string? translated)
            && translated is not null)
        {
            return OperationResult<string>.Ok(translated);
        }

        return OperationResult<string>.Fail($"No translation found for identifier: {identifier}");
    }

    public OperationResult<string> GetOriginal(string translatedIdentifier, string sourceLanguage)
    {
        if (string.IsNullOrEmpty(translatedIdentifier) || string.IsNullOrEmpty(sourceLanguage))
        {
            return OperationResult<string>.Fail("Translated identifier or source language is empty.");
        }

        foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data.Identifiers)
        {
            if (kvp.Value.TryGetValue(sourceLanguage, out string? translated)
                && translated is not null
                && string.Equals(translated, translatedIdentifier, StringComparison.Ordinal))
            {
                return OperationResult<string>.Ok(kvp.Key);
            }
        }

        return OperationResult<string>.Fail($"No original found for: {translatedIdentifier}");
    }

    public OperationResult<string> GetLiteralTranslation(string literal, string targetLanguage)
    {
        if (string.IsNullOrEmpty(literal) || string.IsNullOrEmpty(targetLanguage))
        {
            return OperationResult<string>.Fail("Literal or target language is empty.");
        }

        if (Data.Literals.TryGetValue(literal, out Dictionary<string, string>? translations)
            && translations is not null
            && translations.TryGetValue(targetLanguage, out string? translated)
            && translated is not null)
        {
            return OperationResult<string>.Ok(translated);
        }

        return OperationResult<string>.Fail($"No translation found for literal: {literal}");
    }

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

    public bool RemoveIdentifier(string identifier)
    {
        return Data.Identifiers.Remove(identifier);
    }

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

    public int IdentifierCount => Data.Identifiers.Count;
    public int LiteralCount => Data.Literals.Count;

    public static string GetMapFilePath(string projectPath)
    {
        return Path.Combine(projectPath, MapDirectory, MapFileName);
    }

    public static readonly JsonSerializerOptions JsonSerializerReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };
}
