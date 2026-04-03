using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Interfaces;

public class INaturalLanguageProviderContractTests
{
    public MockNaturalLanguageProvider ProviderInstance = new();

    [Fact]
    public void Properties_WhenAccessed_ReturnExpectedValues()
    {
        Assert.Equal("pt-br", ProviderInstance.LanguageCode);
        Assert.Equal("Portugues Brasileiro", ProviderInstance.LanguageName);
    }

    [Fact]
    public async Task LoadTranslationTableAsync_WithValidLanguage_ReturnsSuccess()
    {
        OperationResult result = await ProviderInstance.LoadTranslationTableAsync("CSharp");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void TranslateKeyword_WithKnownId_ReturnsTranslation()
    {
        OperationResultGeneric<string> result = ProviderInstance.TranslateKeyword(30);
        Assert.True(result.IsSuccess);
        Assert.Equal("se", result.Value);
    }

    [Fact]
    public void TranslateKeyword_WithUnknownId_ReturnsFailure()
    {
        OperationResultGeneric<string> result = ProviderInstance.TranslateKeyword(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ReverseTranslateKeyword_WithKnownTranslation_ReturnsId()
    {
        int result = ProviderInstance.ReverseTranslateKeyword("se");
        Assert.Equal(30, result);
    }

    [Fact]
    public void ReverseTranslateKeyword_WithUnknownTranslation_ReturnsMinusOne()
    {
        int result = ProviderInstance.ReverseTranslateKeyword("desconhecido");
        Assert.Equal(-1, result);
    }

    [Fact]
    public void TranslateIdentifier_WithKnownIdentifier_ReturnsTranslation()
    {
        IdentifierContext context = new IdentifierContext
        {
            OriginalName = "count",
            Kind = IdentifierKind.Variable
        };
        OperationResultGeneric<string> result = ProviderInstance.TranslateIdentifier("count", context);
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

        public Task<OperationResult> LoadTranslationTableAsync(string programmingLanguage) =>
            Task.FromResult(OperationResult.Ok());

        public OperationResultGeneric<string> GetOriginalKeyword(int keywordId)
        {
            Dictionary<int, string> originalKeywords = new()
            {
                { 30, "if" },
                { 18, "else" },
                { 10, "class" }
            };

            if (originalKeywords.ContainsKey(keywordId))
            {
                return OperationResultGeneric<string>.Ok(originalKeywords[keywordId]);
            }

            return OperationResultGeneric<string>.Fail("Original keyword not found");
        }

        public OperationResultGeneric<string> TranslateKeyword(int keywordId)
        {
            if (Keywords.ContainsKey(keywordId))
            {
                return OperationResultGeneric<string>.Ok(Keywords[keywordId]);
            }

            return OperationResultGeneric<string>.Fail("Keyword not found");
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

        public OperationResultGeneric<string> TranslateIdentifier(string identifier, IdentifierContext context)
        {
            if (identifier == "count")
            {
                return OperationResultGeneric<string>.Ok("contador");
            }

            return OperationResultGeneric<string>.Fail("Not found");
        }
    }
}
