using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Interfaces;

/// <summary>
/// Verifies that a mock implementation can satisfy the IIDEAdapter contract.
/// </summary>
public class IIDEAdapterContractTests
{
    private readonly MockIDEAdapter _adapter = new();

    [Fact]
    public void IDEName_IsAccessible()
    {
        Assert.Equal("MockIDE", _adapter.IDEName);
    }

    [Fact]
    public async Task ShowTranslatedContentAsync_Completes()
    {
        await _adapter.ShowTranslatedContentAsync("test.cs", "translated code");
    }

    [Fact]
    public async Task CaptureEditEventAsync_ReturnsEditEvent()
    {
        var result = await _adapter.CaptureEditEventAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SaveOriginalContentAsync_Completes()
    {
        await _adapter.SaveOriginalContentAsync("test.cs", "original code");
    }

    [Fact]
    public async Task ProvideAutocompleteAsync_ReturnsList()
    {
        var result = await _adapter.ProvideAutocompleteAsync("se", 2);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ShowDiagnosticsAsync_Completes()
    {
        await _adapter.ShowDiagnosticsAsync(new List<Diagnostic>());
    }

    private class MockIDEAdapter : IIDEAdapter
    {
        public string IDEName => "MockIDE";

        public Task ShowTranslatedContentAsync(string filePath, string translatedContent) =>
            Task.CompletedTask;

        public Task<EditEvent> CaptureEditEventAsync() =>
            Task.FromResult(new EditEvent { FilePath = "test.cs" });

        public Task SaveOriginalContentAsync(string filePath, string originalContent) =>
            Task.CompletedTask;

        public Task<List<CompletionItem>> ProvideAutocompleteAsync(string partialText, int position) =>
            Task.FromResult(new List<CompletionItem>());

        public Task ShowDiagnosticsAsync(List<Diagnostic> diagnostics) =>
            Task.CompletedTask;
    }
}
