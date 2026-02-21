using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Service that manages bidirectional identifier mappings per project.
/// Persists mappings to .multilingual/identifier-map.json in the project root.
/// This service handles storage and lookup only; parsing of // tradu: annotations
/// is the responsibility of the tradu detection system.
/// </summary>
public class IdentifierMapper
{
    private const string MapDirectory = ".multilingual";
    private const string MapFileName = "identifier-map.json";

    private IdentifierMapData _data = new();
    private string? _loadedPath;

    /// <summary>
    /// Loads the identifier map from a project directory.
    /// If the file does not exist, starts with an empty map.
    /// </summary>
    /// <param name="projectPath">Root directory of the project.</param>
    public void LoadMap(string projectPath)
    {
        ArgumentNullException.ThrowIfNull(projectPath);

        var filePath = GetMapFilePath(projectPath);
        _loadedPath = filePath;

        if (!File.Exists(filePath))
        {
            _data = new IdentifierMapData();
            return;
        }

        var json = File.ReadAllText(filePath);
        _data = JsonSerializer.Deserialize<IdentifierMapData>(json, JsonOptions)
            ?? new IdentifierMapData();
    }

    /// <summary>
    /// Loads the identifier map from a project directory asynchronously.
    /// </summary>
    public async Task LoadMapAsync(string projectPath)
    {
        ArgumentNullException.ThrowIfNull(projectPath);

        var filePath = GetMapFilePath(projectPath);
        _loadedPath = filePath;

        if (!File.Exists(filePath))
        {
            _data = new IdentifierMapData();
            return;
        }

        await using var stream = File.OpenRead(filePath);
        _data = await JsonSerializer.DeserializeAsync<IdentifierMapData>(stream, JsonOptions)
            ?? new IdentifierMapData();
    }

    /// <summary>
    /// Returns the translated name for an identifier in the target language, or null if not mapped.
    /// </summary>
    public string? GetTranslation(string identifier, string targetLanguage)
    {
        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(targetLanguage))
            return null;

        if (_data.Identifiers.TryGetValue(identifier, out var translations)
            && translations.TryGetValue(targetLanguage, out var translated))
        {
            return translated;
        }

        return null;
    }

    /// <summary>
    /// Returns the original identifier name for a translated name, or null if not mapped.
    /// </summary>
    public string? GetOriginal(string translatedIdentifier, string sourceLanguage)
    {
        if (string.IsNullOrEmpty(translatedIdentifier) || string.IsNullOrEmpty(sourceLanguage))
            return null;

        foreach (var kvp in _data.Identifiers)
        {
            if (kvp.Value.TryGetValue(sourceLanguage, out var translated)
                && string.Equals(translated, translatedIdentifier, StringComparison.Ordinal))
            {
                return kvp.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the translated string literal in the target language, or null if not mapped.
    /// </summary>
    public string? GetLiteralTranslation(string literal, string targetLanguage)
    {
        if (string.IsNullOrEmpty(literal) || string.IsNullOrEmpty(targetLanguage))
            return null;

        if (_data.Literals.TryGetValue(literal, out var translations)
            && translations.TryGetValue(targetLanguage, out var translated))
        {
            return translated;
        }

        return null;
    }

    /// <summary>
    /// Sets a translation for an identifier in a specific language.
    /// </summary>
    public void SetTranslation(string identifier, string targetLanguage, string translatedName)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(translatedName);

        if (!_data.Identifiers.ContainsKey(identifier))
            _data.Identifiers[identifier] = new Dictionary<string, string>();

        _data.Identifiers[identifier][targetLanguage] = translatedName;
    }

    /// <summary>
    /// Sets a translation for a string literal in a specific language.
    /// </summary>
    public void SetLiteralTranslation(string literal, string targetLanguage, string translatedLiteral)
    {
        ArgumentNullException.ThrowIfNull(literal);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(translatedLiteral);

        if (!_data.Literals.ContainsKey(literal))
            _data.Literals[literal] = new Dictionary<string, string>();

        _data.Literals[literal][targetLanguage] = translatedLiteral;
    }

    /// <summary>
    /// Removes all translations for an identifier.
    /// </summary>
    public bool RemoveIdentifier(string identifier)
    {
        return _data.Identifiers.Remove(identifier);
    }

    /// <summary>
    /// Saves the identifier map to the project directory.
    /// Uses the same path from the last LoadMap call, or the provided projectPath.
    /// </summary>
    public void SaveMap(string? projectPath = null)
    {
        var filePath = projectPath != null
            ? GetMapFilePath(projectPath)
            : _loadedPath ?? throw new InvalidOperationException("No project path set. Call LoadMap first or provide a projectPath.");

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_data, WriteOptions);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Returns the number of mapped identifiers.
    /// </summary>
    public int IdentifierCount => _data.Identifiers.Count;

    /// <summary>
    /// Returns the number of mapped literals.
    /// </summary>
    public int LiteralCount => _data.Literals.Count;

    private static string GetMapFilePath(string projectPath)
    {
        return Path.Combine(projectPath, MapDirectory, MapFileName);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };
}

/// <summary>
/// Data structure for the identifier-map.json file.
/// </summary>
internal class IdentifierMapData
{
    [JsonPropertyName("identifiers")]
    public Dictionary<string, Dictionary<string, string>> Identifiers { get; set; } = new();

    [JsonPropertyName("literals")]
    public Dictionary<string, Dictionary<string, string>> Literals { get; set; } = new();
}
