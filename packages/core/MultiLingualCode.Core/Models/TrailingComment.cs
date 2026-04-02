namespace MultiLingualCode.Core.Models;

/// <summary>
/// Represents a trailing comment extracted from source code, with the comment text and its line number.
/// </summary>
public class TrailingComment
{
    /// <summary>
    /// The comment text without the language-specific prefix (e.g., without "//" or "#").
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// The zero-based line number where the comment appears.
    /// </summary>
    public int Line { get; set; }
}
