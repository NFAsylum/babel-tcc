using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Services;

public class NaturalLanguageProviderTests
{
    private static string TranslationsPath =>
        Path.Combine(AppContext.BaseDirectory, "TestData", "translations");

    private static async Task<NaturalLanguageProvider> CreateLoadedProvider()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);
        await provider.LoadTranslationTableAsync("csharp");
        return provider;
    }

    [Fact]
    public void Constructor_SetsLanguageCode()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.Equal("pt-br", provider.LanguageCode);
    }

    [Fact]
    public void Create_NullLanguageCode_ReturnsFailure()
    {
        OperationResultGeneric<NaturalLanguageProvider> result = NaturalLanguageProvider.Create(null!, TranslationsPath);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_NullPath_ReturnsFailure()
    {
        OperationResultGeneric<NaturalLanguageProvider> result = NaturalLanguageProvider.Create("pt-br", null!);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadTranslationTableAsync_LoadsSuccessfully()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        Assert.Equal("Portugues Brasileiro", provider.LanguageName);
        Assert.True(provider.IsLoaded("csharp"));
    }

    [Fact]
    public async Task LoadTranslationTableAsync_CachesTable()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        await provider.LoadTranslationTableAsync("csharp");
        LanguageTable table1 = provider.GetActiveLanguageTable();

        // Load again - should use cache
        await provider.LoadTranslationTableAsync("csharp");
        LanguageTable table2 = provider.GetActiveLanguageTable();

        Assert.Same(table1, table2);
    }

    [Fact]
    public async Task LoadTranslationTableAsync_InvalidPath_DoesNotLoad()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", "/nonexistent/path");

        await provider.LoadTranslationTableAsync("csharp");

        Assert.False(provider.HasActiveTable);
    }

    [Fact]
    public async Task TranslateKeyword_KnownId_ReturnsTranslation()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        OperationResultGeneric<string> ifResult = provider.TranslateKeyword(30);
        OperationResultGeneric<string> elseResult = provider.TranslateKeyword(18);
        OperationResultGeneric<string> classResult = provider.TranslateKeyword(10);
        OperationResultGeneric<string> voidResult = provider.TranslateKeyword(75);
        OperationResultGeneric<string> returnResult = provider.TranslateKeyword(52);

        Assert.True(ifResult.IsSuccess);
        Assert.Equal("se", ifResult.Value);
        Assert.True(elseResult.IsSuccess);
        Assert.Equal("senao", elseResult.Value);
        Assert.True(classResult.IsSuccess);
        Assert.Equal("classe", classResult.Value);
        Assert.True(voidResult.IsSuccess);
        Assert.Equal("vazio", voidResult.Value);
        Assert.True(returnResult.IsSuccess);
        Assert.Equal("retornar", returnResult.Value);
    }

    [Fact]
    public async Task TranslateKeyword_UnknownId_ReturnsFailure()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        OperationResultGeneric<string> result = provider.TranslateKeyword(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void TranslateKeyword_BeforeLoad_ReturnsFailure()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        OperationResultGeneric<string> result = provider.TranslateKeyword(30);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ReverseTranslateKeyword_KnownTranslation_ReturnsId()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        Assert.Equal(30, provider.ReverseTranslateKeyword("se"));
        Assert.Equal(18, provider.ReverseTranslateKeyword("senao"));
        Assert.Equal(10, provider.ReverseTranslateKeyword("classe"));
    }

    [Fact]
    public async Task ReverseTranslateKeyword_UnknownTranslation_ReturnsMinusOne()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        Assert.Equal(-1, provider.ReverseTranslateKeyword("desconhecido"));
    }

    [Fact]
    public void ReverseTranslateKeyword_BeforeLoad_ReturnsMinusOne()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.Equal(-1, provider.ReverseTranslateKeyword("se"));
    }

    [Fact]
    public async Task ReverseTranslateKeyword_EmptyOrNull_ReturnsMinusOne()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        Assert.Equal(-1, provider.ReverseTranslateKeyword(""));
        Assert.Equal(-1, provider.ReverseTranslateKeyword(null!));
    }

    [Fact]
    public async Task TranslateIdentifier_WithMap_ReturnsTranslation()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        IdentifierMap map = new IdentifierMap();
        map.Set("GetName", "ObterNome");
        map.Set("SetValue", "DefinirValor");
        provider.SetIdentifierMap(map);

        IdentifierContext context = new IdentifierContext { OriginalName = "GetName", Kind = IdentifierKind.Method };
        OperationResultGeneric<string> result = provider.TranslateIdentifier("GetName", context);
        Assert.True(result.IsSuccess);
        Assert.Equal("ObterNome", result.Value);
    }

    [Fact]
    public async Task TranslateIdentifier_WithoutMap_ReturnsFailure()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        IdentifierContext context = new IdentifierContext { OriginalName = "GetName", Kind = IdentifierKind.Method };
        OperationResultGeneric<string> result = provider.TranslateIdentifier("GetName", context);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task TranslateIdentifier_UnknownIdentifier_ReturnsFailure()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        IdentifierMap map = new IdentifierMap();
        map.Set("GetName", "ObterNome");
        provider.SetIdentifierMap(map);

        IdentifierContext context = new IdentifierContext { OriginalName = "Unknown", Kind = IdentifierKind.Method };
        OperationResultGeneric<string> result = provider.TranslateIdentifier("Unknown", context);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task TranslateIdentifier_EmptyOrNull_ReturnsFailure()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        IdentifierContext context = new IdentifierContext { Kind = IdentifierKind.Variable };
        OperationResultGeneric<string> emptyResult = provider.TranslateIdentifier("", context);
        OperationResultGeneric<string> nullResult = provider.TranslateIdentifier(null!, context);
        Assert.False(emptyResult.IsSuccess);
        Assert.False(nullResult.IsSuccess);
    }

    [Fact]
    public async Task LoadIdentifierMapAsync_LoadsFromFile()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        string mapPath = Path.Combine(AppContext.BaseDirectory, "TestData", "identifier-map.json");
        await provider.LoadIdentifierMapAsync(mapPath);

        IdentifierContext context = new IdentifierContext { OriginalName = "GetName", Kind = IdentifierKind.Method };
        OperationResultGeneric<string> result = provider.TranslateIdentifier("GetName", context);
        Assert.True(result.IsSuccess);
        Assert.Equal("ObterNome", result.Value);
    }

    [Fact]
    public void IsLoaded_ReturnsFalseBeforeLoad()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.False(provider.IsLoaded("csharp"));
    }

    [Fact]
    public async Task IsLoaded_ReturnsTrueAfterLoad()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        Assert.True(provider.IsLoaded("csharp"));
        Assert.False(provider.IsLoaded("python"));
    }

    [Fact]
    public async Task ActiveKeywordTable_IsAvailableAfterLoad()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        KeywordTable keywordTable = provider.GetActiveKeywordTable();
        Assert.NotNull(keywordTable);
        Assert.Equal(30, keywordTable.GetKeywordId("if"));
    }

    [Fact]
    public void ActiveKeywordTable_HasActiveTableIsFalseBeforeLoad()
    {
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.False(provider.HasActiveTable);
    }

    [Fact]
    public async Task RoundTrip_TranslateAndReverse()
    {
        NaturalLanguageProvider provider = await CreateLoadedProvider();

        // Translate forward
        OperationResultGeneric<string> translatedResult = provider.TranslateKeyword(30); // if -> se
        Assert.True(translatedResult.IsSuccess);
        Assert.Equal("se", translatedResult.Value);

        // Translate back
        int id = provider.ReverseTranslateKeyword(translatedResult.Value);
        Assert.Equal(30, id);
    }
}
