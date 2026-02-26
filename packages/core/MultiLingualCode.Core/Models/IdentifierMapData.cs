using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Models;

/// <summary>
/// JSON-serializable data containing identifier and literal translation mappings for a source file.
/// </summary>
public class IdentifierMapData
{
    /// <summary>
    /// Gets or sets the identifier translation mappings, keyed by original name then by language code to translated name.
    /// </summary>
    [JsonPropertyName("identifiers")]
    public Dictionary<string, Dictionary<string, string>> Identifiers { get; set; } = new();

    /// <summary>
    /// Gets or sets the literal translation mappings, keyed by original literal then by language code to translated literal.
    /// </summary>
    [JsonPropertyName("literals")]
    public Dictionary<string, Dictionary<string, string>> Literals { get; set; } = new();
}
