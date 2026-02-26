namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to validate the syntax of source code.
/// </summary>
public class ValidateRequest
{
    /// <summary>
    /// The source code to validate.
    /// </summary>
    public string SourceCode { get; set; } = "";

    /// <summary>
    /// The file extension identifying the programming language (e.g. ".cs").
    /// </summary>
    public string FileExtension { get; set; } = ".cs";
}
