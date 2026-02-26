namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to reverse-translate code from a natural language back to the original programming language.
/// </summary>
public class ReverseTranslateRequest
{
    /// <summary>
    /// The translated source code to convert back to the original programming language.
    /// </summary>
    public string TranslatedCode { get; set; } = "";

    /// <summary>
    /// The file extension identifying the programming language (e.g. ".cs").
    /// </summary>
    public string FileExtension { get; set; } = ".cs";

    /// <summary>
    /// The natural language code the code was translated into (e.g. "pt-br").
    /// </summary>
    public string SourceLanguage { get; set; } = "pt-br";
}
