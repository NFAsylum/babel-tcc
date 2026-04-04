namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to apply user edits from translated code back to the original code using 3-way diff.
/// </summary>
public class ApplyEditsRequest
{
    /// <summary>
    /// The original source code on disk.
    /// </summary>
    public string OriginalCode { get; set; } = "";

    /// <summary>
    /// The translated code before user edits (from forward translation cache).
    /// </summary>
    public string PreviousTranslatedCode { get; set; } = "";

    /// <summary>
    /// The translated code after user edits (what the user saved).
    /// </summary>
    public string EditedTranslatedCode { get; set; } = "";

    /// <summary>
    /// The file extension identifying the programming language (e.g. ".cs", ".py").
    /// </summary>
    public string FileExtension { get; set; } = ".cs";

    /// <summary>
    /// The natural language code the code was translated into (e.g. "pt-br").
    /// </summary>
    public string SourceLanguage { get; set; } = "pt-br";
}
