namespace MultiLingualCode.Core.Models;

public class TraduAnnotation
{
    public string OriginalIdentifier { get; set; } = string.Empty;
    public string TranslatedIdentifier { get; set; } = string.Empty;
    public List<TraduParameterMapping> ParameterMappings { get; set; } = new();
    public string OriginalLiteral { get; set; } = string.Empty;
    public string TranslatedLiteral { get; set; } = string.Empty;
    public bool IsLiteralAnnotation { get; set; }
    public int SourceLine { get; set; }
}
