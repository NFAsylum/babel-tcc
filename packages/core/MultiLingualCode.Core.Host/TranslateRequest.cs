namespace MultiLingualCode.Core.Host;

public class TranslateRequest
{
    public string SourceCode { get; set; } = "";
    public string FileExtension { get; set; } = ".cs";
    public string TargetLanguage { get; set; } = "pt-br";
}
