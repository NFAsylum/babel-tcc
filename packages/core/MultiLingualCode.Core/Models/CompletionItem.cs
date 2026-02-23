namespace MultiLingualCode.Core.Models;

/// <summary>
/// An autocomplete suggestion item provided to the IDE.
/// </summary>
public class CompletionItem
{
    /// <summary>
    /// Text displayed in the autocomplete list (translated keyword or identifier).
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Text inserted when the suggestion is accepted.
    /// </summary>
    public string InsertText { get; set; } = string.Empty;

    /// <summary>
    /// Additional detail shown alongside the label (e.g. the original keyword).
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Kind of completion item.
    /// </summary>
    public CompletionItemKind Kind { get; set; }
}
