using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Host;

/// <summary>
/// JSON-serializable model for keyword-categories.json files.
/// Maps keyword text to semantic category (control, type, modifier, literal).
/// </summary>
public class KeywordCategoryFile
{
    /// <summary>
    /// Dictionary mapping keyword text to its semantic category.
    /// </summary>
    [JsonPropertyName("categories")]
    public Dictionary<string, string> Categories { get; set; } = new();
}
