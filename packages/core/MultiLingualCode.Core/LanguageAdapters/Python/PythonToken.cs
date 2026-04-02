using System.Text.Json.Serialization;

namespace MultiLingualCode.Core.LanguageAdapters.Python;

/// <summary>
/// Represents a single token produced by the Python tokenizer subprocess.
/// </summary>
public class PythonToken
{
    /// <summary>
    /// Gets or sets the numeric token type from Python's <c>token</c> module.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name of the token type (e.g. "NAME", "OP", "NUMBER").
    /// </summary>
    [JsonPropertyName("typeName")]
    public string TypeName { get; set; } = "";

    /// <summary>
    /// Gets or sets the actual text content of the token.
    /// </summary>
    [JsonPropertyName("string")]
    public string String { get; set; } = "";

    /// <summary>
    /// Gets or sets the one-based line number where the token starts.
    /// </summary>
    [JsonPropertyName("startLine")]
    public int StartLine { get; set; }

    /// <summary>
    /// Gets or sets the zero-based column offset where the token starts.
    /// </summary>
    [JsonPropertyName("startCol")]
    public int StartCol { get; set; }

    /// <summary>
    /// Gets or sets the one-based line number where the token ends.
    /// </summary>
    [JsonPropertyName("endLine")]
    public int EndLine { get; set; }

    /// <summary>
    /// Gets or sets the zero-based column offset where the token ends.
    /// </summary>
    [JsonPropertyName("endCol")]
    public int EndCol { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this token is a Python keyword.
    /// </summary>
    [JsonPropertyName("isKeyword")]
    public bool IsKeyword { get; set; }
}
