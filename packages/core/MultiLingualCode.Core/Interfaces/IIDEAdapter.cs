using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Adapter for an IDE. Each supported IDE (VS Code, Visual Studio, etc.)
/// implements this interface to handle visual translation in the editor.
/// </summary>
public interface IIDEAdapter
{
    /// <summary>
    /// Name of the IDE (e.g. "VSCode", "VisualStudio").
    /// </summary>
    string IDEName { get; }

    /// <summary>
    /// Displays translated content in the editor for the given file.
    /// </summary>
    /// <param name="filePath">Path of the file being translated.</param>
    /// <param name="translatedContent">The translated source code to display.</param>
    Task ShowTranslatedContentAsync(string filePath, string translatedContent);

    /// <summary>
    /// Captures the next edit event from the user in the editor.
    /// </summary>
    /// <returns>The edit event that occurred.</returns>
    Task<EditEvent> CaptureEditEventAsync();

    /// <summary>
    /// Saves original (untranslated) content to disk.
    /// </summary>
    /// <param name="filePath">Path where the file should be saved.</param>
    /// <param name="originalContent">The original source code to save.</param>
    Task SaveOriginalContentAsync(string filePath, string originalContent);

    /// <summary>
    /// Provides autocomplete suggestions based on translations.
    /// </summary>
    /// <param name="partialText">The partial text typed by the user.</param>
    /// <param name="position">Cursor position in the document.</param>
    /// <returns>List of completion suggestions.</returns>
    Task<List<CompletionItem>> ProvideAutocompleteAsync(string partialText, int position);

    /// <summary>
    /// Displays translated diagnostics (errors/warnings) in the IDE.
    /// </summary>
    /// <param name="diagnostics">List of diagnostics to display.</param>
    Task ShowDiagnosticsAsync(List<Diagnostic> diagnostics);
}
