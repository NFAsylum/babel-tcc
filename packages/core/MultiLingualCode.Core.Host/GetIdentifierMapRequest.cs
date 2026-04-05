namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to get the identifier translation map for a specific target language.
/// </summary>
public class GetIdentifierMapRequest
{
    /// <summary>
    /// The target natural language code (e.g. "pt-br").
    /// </summary>
    public string TargetLanguage { get; set; } = "pt-br";
}
