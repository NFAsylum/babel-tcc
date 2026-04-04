namespace MultiLingualCode.Core.Models;

/// <summary>
/// Types of diff operations between two versions of a line.
/// </summary>
public enum DiffOpType
{
    /// <summary>Line is identical in both versions.</summary>
    Equal,

    /// <summary>Line was modified (exists in both but with different content).</summary>
    Modified,

    /// <summary>Line was inserted in the edited version.</summary>
    Insert,

    /// <summary>Line was deleted from the previous version.</summary>
    Delete
}
