namespace MultiLingualCode.Core.Models;

public class IdentifierContext
{
    public string OriginalName { get; set; } = string.Empty;
    public IdentifierKind Kind { get; set; }
    public string ContainingType { get; set; } = "";
    public string FilePath { get; set; } = "";
}
