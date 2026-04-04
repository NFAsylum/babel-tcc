namespace MultiLingualCode.Core.Models;

/// <summary>
/// Represents a single diff operation between two versions of a line.
/// </summary>
public class DiffOp
{
    /// <summary>
    /// The type of diff operation.
    /// </summary>
    public DiffOpType Type { get; set; }

    /// <summary>
    /// The line from the previous version (set for Equal, Modified, Delete).
    /// </summary>
    public string PreviousLine { get; set; } = "";

    /// <summary>
    /// The line from the edited version (set for Equal, Modified, Insert).
    /// </summary>
    public string EditedLine { get; set; } = "";
}
