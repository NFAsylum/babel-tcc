using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Bidirectional map between original and translated identifiers, supporting lookup in both directions.
/// </summary>
public class IdentifierMap
{
    /// <summary>
    /// Dictionary mapping original identifier names to their translated equivalents.
    /// </summary>
    public Dictionary<string, string> OriginalToTranslated = new();

    /// <summary>
    /// Dictionary mapping translated identifier names back to their originals.
    /// </summary>
    public Dictionary<string, string> TranslatedToOriginal = new();

    /// <summary>
    /// JSON-serializable dictionary of identifier mappings. Setting this property rebuilds both lookup dictionaries.
    /// </summary>
    [JsonPropertyName("identifiers")]
    public Dictionary<string, string> Identifiers
    {
        get => OriginalToTranslated;
        set
        {
            OriginalToTranslated = new Dictionary<string, string>(value);
            TranslatedToOriginal = new Dictionary<string, string>(value.Count);
            foreach (KeyValuePair<string, string> kvp in value)
            {
                TranslatedToOriginal[kvp.Value] = kvp.Key;
            }
        }
    }

    /// <summary>
    /// Looks up the translated name for a given original identifier.
    /// </summary>
    /// <param name="originalName">The original identifier name to translate.</param>
    /// <returns>An operation result containing the translated name, or a failure if not found.</returns>
    public OperationResultGeneric<string> GetTranslated(string originalName)
    {
        if (string.IsNullOrEmpty(originalName))
        {
            return OperationResultGeneric<string>.Fail("Original name is empty");
        }

        if (OriginalToTranslated.ContainsKey(originalName))
        {
            return OperationResultGeneric<string>.Ok(OriginalToTranslated[originalName]);
        }

        return OperationResultGeneric<string>.Fail($"No translation found for: {originalName}");
    }

    /// <summary>
    /// Looks up the original identifier name for a given translated name.
    /// </summary>
    /// <param name="translatedName">The translated identifier name to reverse-lookup.</param>
    /// <returns>An operation result containing the original name, or a failure if not found.</returns>
    public OperationResultGeneric<string> GetOriginal(string translatedName)
    {
        if (string.IsNullOrEmpty(translatedName))
        {
            return OperationResultGeneric<string>.Fail("Translated name is empty");
        }

        if (TranslatedToOriginal.ContainsKey(translatedName))
        {
            return OperationResultGeneric<string>.Ok(TranslatedToOriginal[translatedName]);
        }

        return OperationResultGeneric<string>.Fail($"No original found for: {translatedName}");
    }

    /// <summary>
    /// Adds or updates a bidirectional mapping between an original and translated identifier name.
    /// </summary>
    /// <param name="originalName">The original identifier name.</param>
    /// <param name="translatedName">The translated identifier name.</param>
    public void Set(string originalName, string translatedName)
    {
        if (string.IsNullOrEmpty(originalName) || string.IsNullOrEmpty(translatedName))
        {
            return;
        }

        if (OriginalToTranslated.ContainsKey(originalName))
        {
            TranslatedToOriginal.Remove(OriginalToTranslated[originalName]);
        }

        OriginalToTranslated[originalName] = translatedName;
        TranslatedToOriginal[translatedName] = originalName;
    }

    /// <summary>
    /// Removes a bidirectional mapping by the original identifier name.
    /// </summary>
    /// <param name="originalName">The original identifier name to remove.</param>
    /// <returns>True if the mapping was found and removed; false otherwise.</returns>
    public bool Remove(string originalName)
    {
        if (!OriginalToTranslated.ContainsKey(originalName))
        {
            return false;
        }

        string translated = OriginalToTranslated[originalName];
        OriginalToTranslated.Remove(originalName);
        TranslatedToOriginal.Remove(translated);
        return true;
    }

    /// <summary>
    /// Gets the number of identifier mappings in the map.
    /// </summary>
    public int Count => OriginalToTranslated.Count;

    /// <summary>
    /// Loads an <see cref="IdentifierMap"/> from a JSON file synchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the loaded map, or a failure if the file could not be read.</returns>
    public static OperationResultGeneric<IdentifierMap> LoadFrom(string filePath)
    {
        return JsonFileReader.ReadFromFile<IdentifierMap>(filePath, JsonOptions.Default);
    }

    /// <summary>
    /// Loads an <see cref="IdentifierMap"/> from a JSON file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>An operation result containing the loaded map, or a failure if the file could not be read.</returns>
    public static async Task<OperationResultGeneric<IdentifierMap>> LoadFromAsync(string filePath)
    {
        return await JsonFileReader.ReadFromFileAsync<IdentifierMap>(filePath, JsonOptions.Default);
    }
}
