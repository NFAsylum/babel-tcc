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

/// <summary>
/// A diagnostic message (error, warning, or info) associated with a code location.
/// </summary>
public class Diagnostic
{
    /// <summary>
    /// Severity level of the diagnostic.
    /// </summary>
    public DiagnosticSeverity Severity { get; set; }

    /// <summary>
    /// Human-readable message describing the issue.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the diagnostic was found (0-based).
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Column number where the diagnostic was found (0-based).
    /// </summary>
    public int Column { get; set; }
}

/// <summary>
/// Severity levels for diagnostics.
/// </summary>
public enum DiagnosticSeverity
{
    Error,
    Warning,
    Info
}
