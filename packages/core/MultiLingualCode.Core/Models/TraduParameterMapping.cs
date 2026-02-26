namespace MultiLingualCode.Core.Models;

/// <summary>
/// Maps an original parameter name to its translated name for use in translated code display.
/// </summary>
public class TraduParameterMapping
{
    /// <summary>
    /// Gets or sets the parameter name as it appears in the original source code.
    /// </summary>
    public string OriginalParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translated parameter name in the target language.
    /// </summary>
    public string TranslatedParameterName { get; set; } = string.Empty;
}
