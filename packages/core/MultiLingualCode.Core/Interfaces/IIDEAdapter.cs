using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Interfaces;

public interface IIDEAdapter
{
    string IDEName { get; }
    Task ShowTranslatedContentAsync(string filePath, string translatedContent);
    Task<EditEvent> CaptureEditEventAsync();
    Task SaveOriginalContentAsync(string filePath, string originalContent);
    Task<List<CompletionItem>> ProvideAutocompleteAsync(string partialText, int position);
    Task ShowDiagnosticsAsync(List<Diagnostic> diagnostics);
}
