namespace MultiLingualCode.Core.Host;

/// <summary>
/// Standard response object returned by the CLI host for all operations.
/// </summary>
public class CoreResponse
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The operation result payload (JSON string or translated code). Empty on failure.
    /// </summary>
    public string Result { get; set; } = "";

    /// <summary>
    /// The error message describing what went wrong. Empty on success.
    /// </summary>
    public string Error { get; set; } = "";
}
