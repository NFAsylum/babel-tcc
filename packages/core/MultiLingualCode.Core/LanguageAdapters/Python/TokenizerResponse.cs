using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.LanguageAdapters.Python;

/// <summary>
/// Model for deserializing the JSON response from the Python tokenizer subprocess.
/// </summary>
public class TokenizerResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether tokenization completed without errors.
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>
    /// Gets or sets the error message when tokenization fails.
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = "";

    /// <summary>
    /// Gets or sets the list of tokens produced by the tokenizer.
    /// </summary>
    [JsonPropertyName("tokens")]
    public List<PythonToken> Tokens { get; set; } = new();
}
