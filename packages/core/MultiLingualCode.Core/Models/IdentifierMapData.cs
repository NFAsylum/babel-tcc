using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.Models;

public class IdentifierMapData
{
    [JsonPropertyName("identifiers")]
    public Dictionary<string, Dictionary<string, string>> Identifiers { get; set; } = new();

    [JsonPropertyName("literals")]
    public Dictionary<string, Dictionary<string, string>> Literals { get; set; } = new();
}
