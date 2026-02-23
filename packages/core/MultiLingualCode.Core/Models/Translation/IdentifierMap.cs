using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Translation;

public class IdentifierMap
{
    public Dictionary<string, string> OriginalToTranslated = new();
    public Dictionary<string, string> TranslatedToOriginal = new();

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

    public OperationResultGeneric<string> GetTranslated(string originalName)
    {
        if (string.IsNullOrEmpty(originalName))
        {
            return OperationResultGeneric<string>.Fail("Original name is empty");
        }

        if (OriginalToTranslated.TryGetValue(originalName, out string? translated) && translated is not null)
        {
            return OperationResultGeneric<string>.Ok(translated);
        }

        return OperationResultGeneric<string>.Fail($"No translation found for: {originalName}");
    }

    public OperationResultGeneric<string> GetOriginal(string translatedName)
    {
        if (string.IsNullOrEmpty(translatedName))
        {
            return OperationResultGeneric<string>.Fail("Translated name is empty");
        }

        if (TranslatedToOriginal.TryGetValue(translatedName, out string? original) && original is not null)
        {
            return OperationResultGeneric<string>.Ok(original);
        }

        return OperationResultGeneric<string>.Fail($"No original found for: {translatedName}");
    }

    public void Set(string originalName, string translatedName)
    {
        if (string.IsNullOrEmpty(originalName) || string.IsNullOrEmpty(translatedName))
        {
            return;
        }

        if (OriginalToTranslated.TryGetValue(originalName, out string? oldTranslated) && oldTranslated is not null)
        {
            TranslatedToOriginal.Remove(oldTranslated);
        }

        OriginalToTranslated[originalName] = translatedName;
        TranslatedToOriginal[translatedName] = originalName;
    }

    public bool Remove(string originalName)
    {
        if (!OriginalToTranslated.TryGetValue(originalName, out string? translated) || translated is null)
        {
            return false;
        }

        OriginalToTranslated.Remove(originalName);
        TranslatedToOriginal.Remove(translated);
        return true;
    }

    public int Count => OriginalToTranslated.Count;

    public static OperationResultGeneric<IdentifierMap> LoadFrom(string filePath)
    {
        return JsonFileReader.ReadFromFile<IdentifierMap>(filePath, JsonOptions.Default);
    }

    public static async Task<OperationResultGeneric<IdentifierMap>> LoadFromAsync(string filePath)
    {
        return await JsonFileReader.ReadFromFileAsync<IdentifierMap>(filePath, JsonOptions.Default);
    }
}
