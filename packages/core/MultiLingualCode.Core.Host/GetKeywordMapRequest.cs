namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to get the keyword translation map for a specific language pair.
/// </summary>
public class GetKeywordMapRequest
{
    /// <summary>
    /// The file extension identifying the programming language (e.g. ".cs", ".py").
    /// </summary>
    public string FileExtension { get; set; } = ".cs";

    /// <summary>
    /// The target natural language code (e.g. "pt-br").
    /// </summary>
    public string TargetLanguage { get; set; } = "pt-br";
}
