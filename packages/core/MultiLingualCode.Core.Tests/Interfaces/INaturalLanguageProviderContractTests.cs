using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Interfaces;

/// <summary>
/// Verifies that a mock implementation can satisfy the INaturalLanguageProvider contract.
/// </summary>
public class INaturalLanguageProviderContractTests
{
    private readonly MockNaturalLanguageProvider _provider = new();

    [Fact]
    public void Properties_AreAccessible()
    {
        Assert.Equal("pt-br", _provider.LanguageCode);
        Assert.Equal("Portugues Brasileiro", _provider.LanguageName);
    }

    [Fact]
    public async Task LoadTranslationTableAsync_Completes()
    {
        await _provider.LoadTranslationTableAsync("CSharp");
    }

    [Fact]
    public void TranslateKeyword_ReturnsTranslation()
    {
        var result = _provider.TranslateKeyword(30);
        Assert.Equal("se", result);
    }

    [Fact]
    public void TranslateKeyword_ReturnsNullForUnknown()
    {
        var result = _provider.TranslateKeyword(999);
        Assert.Null(result);
    }

    [Fact]
    public void ReverseTranslateKeyword_ReturnsId()
    {
        var result = _provider.ReverseTranslateKeyword("se");
        Assert.Equal(30, result);
    }

    [Fact]
    public void ReverseTranslateKeyword_ReturnsNegativeForUnknown()
    {
        var result = _provider.ReverseTranslateKeyword("desconhecido");
        Assert.Equal(-1, result);
    }

    [Fact]
    public void TranslateIdentifier_ReturnsTranslation()
    {
        var context = new IdentifierContext
        {
            OriginalName = "count",
            Kind = IdentifierKind.Variable
        };
        var result = _provider.TranslateIdentifier("count", context);
        Assert.NotNull(result);
    }

    private class MockNaturalLanguageProvider : INaturalLanguageProvider
    {
        private readonly Dictionary<int, string> _keywords = new()
        {
            { 30, "se" },
            { 18, "senao" },
            { 10, "classe" }
        };

        public string LanguageCode => "pt-br";
        public string LanguageName => "Portugues Brasileiro";

        public Task LoadTranslationTableAsync(string programmingLanguage) =>
            Task.CompletedTask;

        public string? TranslateKeyword(int keywordId) =>
            _keywords.TryGetValue(keywordId, out var value) ? value : null;

        public int ReverseTranslateKeyword(string translatedKeyword)
        {
            foreach (var kvp in _keywords)
            {
                if (kvp.Value == translatedKeyword)
                    return kvp.Key;
            }
            return -1;
        }

        public string? TranslateIdentifier(string identifier, IdentifierContext context) =>
            identifier == "count" ? "contador" : null;
    }
}
