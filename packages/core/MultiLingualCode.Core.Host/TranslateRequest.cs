namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to translate source code from a programming language to a natural language.
/// </summary>
public class TranslateRequest
{
    /// <summary>
    /// The source code to translate.
    /// </summary>
    public string SourceCode { get; set; } = "";

    /// <summary>
    /// The file extension identifying the programming language (e.g. ".cs").
    /// </summary>
    public string FileExtension { get; set; } = ".cs";

    /// <summary>
    /// The target natural language code (e.g. "pt-br").
    /// </summary>
    public string TargetLanguage { get; set; } = "pt-br";
}
