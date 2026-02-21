using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Represents a bidirectional mapping of identifiers (original <-> translated).
/// Used for user-defined identifiers like method names, variable names, etc.
/// </summary>
public class IdentifierMap
{
    private Dictionary<string, string> _originalToTranslated = new();
    private Dictionary<string, string> _translatedToOriginal = new();

    [JsonPropertyName("identifiers")]
    public Dictionary<string, string> Identifiers
    {
        get => _originalToTranslated;
        set
        {
            _originalToTranslated = new Dictionary<string, string>(value);
            _translatedToOriginal = new Dictionary<string, string>(value.Count);
            foreach (var kvp in value)
            {
                _translatedToOriginal[kvp.Value] = kvp.Key;
            }
        }
    }

    /// <summary>
    /// Returns the translated identifier for an original name, or null if not found.
    /// </summary>
    public string? GetTranslated(string originalName)
    {
        if (string.IsNullOrEmpty(originalName))
            return null;

        return _originalToTranslated.TryGetValue(originalName, out var translated) ? translated : null;
    }

    /// <summary>
    /// Returns the original identifier for a translated name, or null if not found.
    /// </summary>
    public string? GetOriginal(string translatedName)
    {
        if (string.IsNullOrEmpty(translatedName))
            return null;

        return _translatedToOriginal.TryGetValue(translatedName, out var original) ? original : null;
    }

    /// <summary>
    /// Adds or updates a mapping between an original and translated identifier.
    /// </summary>
    public void Set(string originalName, string translatedName)
    {
        ArgumentNullException.ThrowIfNull(originalName);
        ArgumentNullException.ThrowIfNull(translatedName);

        // Remove old reverse mapping if the original was already mapped
        if (_originalToTranslated.TryGetValue(originalName, out var oldTranslated))
            _translatedToOriginal.Remove(oldTranslated);

        _originalToTranslated[originalName] = translatedName;
        _translatedToOriginal[translatedName] = originalName;
    }

    /// <summary>
    /// Removes a mapping by original name.
    /// </summary>
    public bool Remove(string originalName)
    {
        if (!_originalToTranslated.TryGetValue(originalName, out var translated))
            return false;

        _originalToTranslated.Remove(originalName);
        _translatedToOriginal.Remove(translated);
        return true;
    }

    /// <summary>
    /// Returns the total number of identifier mappings.
    /// </summary>
    public int Count => _originalToTranslated.Count;

    /// <summary>
    /// Loads an IdentifierMap from a JSON file.
    /// </summary>
    public static IdentifierMap LoadFrom(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Identifier map file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<IdentifierMap>(json, JsonOptions.Default)
            ?? throw new JsonException($"Failed to deserialize identifier map: {filePath}");
    }

    /// <summary>
    /// Loads an IdentifierMap from a JSON file asynchronously.
    /// </summary>
    public static async Task<IdentifierMap> LoadFromAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Identifier map file not found: {filePath}", filePath);

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<IdentifierMap>(stream, JsonOptions.Default)
            ?? throw new JsonException($"Failed to deserialize identifier map: {filePath}");
    }
}
