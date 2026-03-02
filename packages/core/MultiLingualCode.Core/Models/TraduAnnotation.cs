namespace MultiLingualCode.Core.Models;

/// <summary>
/// Represents a parsed <c>tradu:</c> annotation extracted from a source code comment, defining a translation mapping.
/// </summary>
public class TraduAnnotation
{
    /// <summary>
    /// Gets or sets the original identifier name as it appears in the source code.
    /// </summary>
    public string OriginalIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translated identifier name to display in the target language.
    /// </summary>
    public string TranslatedIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of parameter name mappings associated with this annotation.
    /// </summary>
    public List<TraduParameterMapping> ParameterMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets the original literal value to be translated.
    /// </summary>
    public string OriginalLiteral { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translated literal value in the target language.
    /// </summary>
    public string TranslatedLiteral { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this annotation maps a literal rather than an identifier.
    /// </summary>
    public bool IsLiteralAnnotation { get; set; }

    /// <summary>
    /// Gets or sets the line number in the source file where this annotation was found.
    /// </summary>
    public int SourceLine { get; set; }

    /// <summary>
    /// Gets or sets the target language for this annotation. Empty means the active target language.
    /// </summary>
    public string TargetLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start line of the method containing this annotation (for scoped parameter translations).
    /// </summary>
    public int MethodStartLine { get; set; } = -1;

    /// <summary>
    /// Gets or sets the end line of the method containing this annotation (for scoped parameter translations).
    /// </summary>
    public int MethodEndLine { get; set; } = -1;
}
