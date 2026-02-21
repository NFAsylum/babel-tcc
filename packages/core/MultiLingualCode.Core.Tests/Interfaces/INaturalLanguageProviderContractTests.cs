using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Interfaces;

public class INaturalLanguageProviderContractTests
{
    public MockNaturalLanguageProvider ProviderInstance = new();

    [Fact]
    public void Properties_AreAccessible()
    {
        Assert.Equal("pt-br", ProviderInstance.LanguageCode);
        Assert.Equal("Portugues Brasileiro", ProviderInstance.LanguageName);
    }

    [Fact]
    public async Task LoadTranslationTableAsync_Completes()
    {
        await ProviderInstance.LoadTranslationTableAsync("CSharp");
    }

    [Fact]
    public void TranslateKeyword_ReturnsTranslation()
    {
        OperationResult<string> result = ProviderInstance.TranslateKeyword(30);
        Assert.True(result.IsSuccess);
        Assert.Equal("se", result.Value);
    }

    [Fact]
    public void TranslateKeyword_ReturnsFailForUnknown()
    {
        OperationResult<string> result = ProviderInstance.TranslateKeyword(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ReverseTranslateKeyword_ReturnsId()
    {
        int result = ProviderInstance.ReverseTranslateKeyword("se");
        Assert.Equal(30, result);
    }

    [Fact]
    public void ReverseTranslateKeyword_ReturnsNegativeForUnknown()
    {
        int result = ProviderInstance.ReverseTranslateKeyword("desconhecido");
        Assert.Equal(-1, result);
    }

    [Fact]
    public void TranslateIdentifier_ReturnsTranslation()
    {
        IdentifierContext context = new IdentifierContext
        {
            OriginalName = "count",
            Kind = IdentifierKind.Variable
        };
        OperationResult<string> result = ProviderInstance.TranslateIdentifier("count", context);
        Assert.True(result.IsSuccess);
    }

    public class MockNaturalLanguageProvider : INaturalLanguageProvider
    {
        public Dictionary<int, string> Keywords = new()
        {
            { 30, "se" },
            { 18, "senao" },
            { 10, "classe" }
        };

        public string LanguageCode => "pt-br";
        public string LanguageName => "Portugues Brasileiro";

        public Task LoadTranslationTableAsync(string programmingLanguage) =>
            Task.CompletedTask;

        public OperationResult<string> TranslateKeyword(int keywordId)
        {
            if (Keywords.TryGetValue(keywordId, out string? value) && value is not null)
            {
                return OperationResult<string>.Ok(value);
            }

            return OperationResult<string>.Fail("Keyword not found");
        }

        public int ReverseTranslateKeyword(string translatedKeyword)
        {
            foreach (KeyValuePair<int, string> kvp in Keywords)
            {
                if (kvp.Value == translatedKeyword)
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        public OperationResult<string> TranslateIdentifier(string identifier, IdentifierContext context)
        {
            if (identifier == "count")
            {
                return OperationResult<string>.Ok("contador");
            }

            return OperationResult<string>.Fail("Not found");
        }
    }
}
