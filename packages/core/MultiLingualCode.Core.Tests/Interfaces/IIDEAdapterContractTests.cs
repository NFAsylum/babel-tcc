using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Interfaces;

public class IIDEAdapterContractTests
{
    public MockIDEAdapter Adapter = new();

    [Fact]
    public void IDEName_WhenAccessed_ReturnsExpectedValue()
    {
        Assert.Equal("MockIDE", Adapter.IDEName);
    }

    [Fact]
    public async Task ShowTranslatedContentAsync_WithValidInput_CompletesWithoutError()
    {
        await Adapter.ShowTranslatedContentAsync("test.cs", "translated code");
    }

    [Fact]
    public async Task CaptureEditEventAsync_WhenCalled_ReturnsNonNullEditEvent()
    {
        EditEvent result = await Adapter.CaptureEditEventAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SaveOriginalContentAsync_WithValidInput_CompletesWithoutError()
    {
        await Adapter.SaveOriginalContentAsync("test.cs", "original code");
    }

    [Fact]
    public async Task ProvideAutocompleteAsync_WithPartialText_ReturnsNonNullList()
    {
        List<CompletionItem> result = await Adapter.ProvideAutocompleteAsync("se", 2);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ShowDiagnosticsAsync_WithEmptyList_CompletesWithoutError()
    {
        await Adapter.ShowDiagnosticsAsync(new List<Diagnostic>());
    }

    public class MockIDEAdapter : IIDEAdapter
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
