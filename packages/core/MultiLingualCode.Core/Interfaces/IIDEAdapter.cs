using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Defines the contract for integrating with an IDE (e.g., VS Code) to display translated code and handle editor interactions.
/// </summary>
public interface IIDEAdapter
{
    /// <summary>
    /// Gets the name of the IDE this adapter targets.
    /// </summary>
    string IDEName { get; }

    /// <summary>
    /// Displays translated source code content in the IDE for the specified file.
    /// </summary>
    Task ShowTranslatedContentAsync(string filePath, string translatedContent);

    /// <summary>
    /// Waits for and captures the next edit event from the IDE editor.
    /// </summary>
    Task<EditEvent> CaptureEditEventAsync();

    /// <summary>
    /// Persists the original (untranslated) source code content for the specified file.
    /// </summary>
    Task SaveOriginalContentAsync(string filePath, string originalContent);

    /// <summary>
    /// Provides autocomplete suggestions for the given partial text at the specified cursor position.
    /// </summary>
    Task<List<CompletionItem>> ProvideAutocompleteAsync(string partialText, int position);

    /// <summary>
    /// Displays a list of diagnostics (errors, warnings) in the IDE.
    /// </summary>
    Task ShowDiagnosticsAsync(List<Diagnostic> diagnostics);
}
