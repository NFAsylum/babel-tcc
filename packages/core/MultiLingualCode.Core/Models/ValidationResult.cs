namespace MultiLingualCode.Core.Models;

/// <summary>
/// Result of a syntax validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the code is syntactically valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of diagnostics (errors and warnings) found during validation.
    /// </summary>
    public List<Diagnostic> Diagnostics { get; set; } = new();
}
