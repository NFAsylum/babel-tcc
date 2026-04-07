namespace MultiLingualCode.Core.Host;

/// <summary>
/// Request to get the keyword category map for a specific programming language.
/// </summary>
public class GetKeywordCategoriesRequest
{
    /// <summary>
    /// The file extension identifying the programming language (e.g. ".cs", ".py").
    /// </summary>
    public string FileExtension { get; set; } = ".cs";
}
