namespace MultiLingualCode.Core.Models;

/// <summary>
/// Represents an edit event captured from the IDE.
/// </summary>
public class EditEvent
{
    /// <summary>
    /// Path of the file being edited.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Start position of the edit in the document (character offset).
    /// </summary>
    public int StartOffset { get; set; }

    /// <summary>
    /// End position of the edit in the document (character offset).
    /// </summary>
    public int EndOffset { get; set; }

    /// <summary>
    /// Text that was inserted (empty string for deletions).
    /// </summary>
    public string NewText { get; set; } = string.Empty;

    /// <summary>
    /// Text that was replaced (empty string for insertions).
    /// </summary>
    public string OldText { get; set; } = string.Empty;
}
