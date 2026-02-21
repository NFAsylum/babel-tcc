namespace MultiLingualCode.Core.Host;

public class ReverseTranslateRequest
{
    public string TranslatedCode { get; set; } = "";
    public string FileExtension { get; set; } = ".cs";
    public string SourceLanguage { get; set; } = "pt-br";
}
