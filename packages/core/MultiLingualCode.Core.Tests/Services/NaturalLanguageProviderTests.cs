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
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);
        await provider.LoadTranslationTableAsync("csharp");
        return provider;
    }

    [Fact]
    public void Constructor_SetsLanguageCode()
    {
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.Equal("pt-br", provider.LanguageCode);
    }

    [Fact]
    public void Constructor_ThrowsOnNullLanguageCode()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NaturalLanguageProvider(null!, TranslationsPath));
    }

    [Fact]
    public void Constructor_ThrowsOnNullPath()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NaturalLanguageProvider("pt-br", null!));
    }

    [Fact]
    public async Task LoadTranslationTableAsync_LoadsSuccessfully()
    {
        var provider = await CreateLoadedProvider();

        Assert.Equal("Portugues Brasileiro", provider.LanguageName);
        Assert.True(provider.IsLoaded("csharp"));
    }

    [Fact]
    public async Task LoadTranslationTableAsync_CachesTable()
    {
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        await provider.LoadTranslationTableAsync("csharp");
        var table1 = provider.ActiveLanguageTable;

        // Load again - should use cache
        await provider.LoadTranslationTableAsync("csharp");
        var table2 = provider.ActiveLanguageTable;

        Assert.Same(table1, table2);
    }

    [Fact]
    public async Task LoadTranslationTableAsync_InvalidPath_ThrowsFileNotFound()
    {
        var provider = new NaturalLanguageProvider("pt-br", "/nonexistent/path");

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            provider.LoadTranslationTableAsync("csharp"));
    }

    [Fact]
    public async Task TranslateKeyword_KnownId_ReturnsTranslation()
    {
        var provider = await CreateLoadedProvider();

        Assert.Equal("se", provider.TranslateKeyword(30));        // if
        Assert.Equal("senao", provider.TranslateKeyword(18));      // else
        Assert.Equal("classe", provider.TranslateKeyword(10));     // class
        Assert.Equal("vazio", provider.TranslateKeyword(75));      // void
        Assert.Equal("retornar", provider.TranslateKeyword(52));   // return
    }

    [Fact]
    public async Task TranslateKeyword_UnknownId_ReturnsNull()
    {
        var provider = await CreateLoadedProvider();

        Assert.Null(provider.TranslateKeyword(999));
    }

    [Fact]
    public void TranslateKeyword_BeforeLoad_ReturnsNull()
    {
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.Null(provider.TranslateKeyword(30));
    }

    [Fact]
    public async Task ReverseTranslateKeyword_KnownTranslation_ReturnsId()
    {
        var provider = await CreateLoadedProvider();

        Assert.Equal(30, provider.ReverseTranslateKeyword("se"));
        Assert.Equal(18, provider.ReverseTranslateKeyword("senao"));
        Assert.Equal(10, provider.ReverseTranslateKeyword("classe"));
    }

    [Fact]
    public async Task ReverseTranslateKeyword_UnknownTranslation_ReturnsMinusOne()
    {
        var provider = await CreateLoadedProvider();

        Assert.Equal(-1, provider.ReverseTranslateKeyword("desconhecido"));
    }

    [Fact]
    public void ReverseTranslateKeyword_BeforeLoad_ReturnsMinusOne()
    {
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.Equal(-1, provider.ReverseTranslateKeyword("se"));
    }

    [Fact]
    public async Task ReverseTranslateKeyword_EmptyOrNull_ReturnsMinusOne()
    {
        var provider = await CreateLoadedProvider();

        Assert.Equal(-1, provider.ReverseTranslateKeyword(""));
        Assert.Equal(-1, provider.ReverseTranslateKeyword(null!));
    }

    [Fact]
    public async Task TranslateIdentifier_WithMap_ReturnsTranslation()
    {
        var provider = await CreateLoadedProvider();

        var map = new IdentifierMap();
        map.Set("GetName", "ObterNome");
        map.Set("SetValue", "DefinirValor");
        provider.SetIdentifierMap(map);

        var context = new IdentifierContext { OriginalName = "GetName", Kind = IdentifierKind.Method };
        Assert.Equal("ObterNome", provider.TranslateIdentifier("GetName", context));
    }

    [Fact]
    public async Task TranslateIdentifier_WithoutMap_ReturnsNull()
    {
        var provider = await CreateLoadedProvider();

        var context = new IdentifierContext { OriginalName = "GetName", Kind = IdentifierKind.Method };
        Assert.Null(provider.TranslateIdentifier("GetName", context));
    }

    [Fact]
    public async Task TranslateIdentifier_UnknownIdentifier_ReturnsNull()
    {
        var provider = await CreateLoadedProvider();

        var map = new IdentifierMap();
        map.Set("GetName", "ObterNome");
        provider.SetIdentifierMap(map);

        var context = new IdentifierContext { OriginalName = "Unknown", Kind = IdentifierKind.Method };
        Assert.Null(provider.TranslateIdentifier("Unknown", context));
    }

    [Fact]
    public async Task TranslateIdentifier_EmptyOrNull_ReturnsNull()
    {
        var provider = await CreateLoadedProvider();

        var context = new IdentifierContext { Kind = IdentifierKind.Variable };
        Assert.Null(provider.TranslateIdentifier("", context));
        Assert.Null(provider.TranslateIdentifier(null!, context));
    }

    [Fact]
    public async Task LoadIdentifierMapAsync_LoadsFromFile()
    {
        var provider = await CreateLoadedProvider();

        var mapPath = Path.Combine(AppContext.BaseDirectory, "TestData", "identifier-map.json");
        await provider.LoadIdentifierMapAsync(mapPath);

        var context = new IdentifierContext { OriginalName = "GetName", Kind = IdentifierKind.Method };
        Assert.Equal("ObterNome", provider.TranslateIdentifier("GetName", context));
    }

    [Fact]
    public void IsLoaded_ReturnsFalseBeforeLoad()
    {
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.False(provider.IsLoaded("csharp"));
    }

    [Fact]
    public async Task IsLoaded_ReturnsTrueAfterLoad()
    {
        var provider = await CreateLoadedProvider();

        Assert.True(provider.IsLoaded("csharp"));
        Assert.False(provider.IsLoaded("python"));
    }

    [Fact]
    public async Task ActiveKeywordTable_IsAvailableAfterLoad()
    {
        var provider = await CreateLoadedProvider();

        Assert.NotNull(provider.ActiveKeywordTable);
        Assert.Equal(30, provider.ActiveKeywordTable!.GetKeywordId("if"));
    }

    [Fact]
    public void ActiveKeywordTable_IsNullBeforeLoad()
    {
        var provider = new NaturalLanguageProvider("pt-br", TranslationsPath);

        Assert.Null(provider.ActiveKeywordTable);
    }

    [Fact]
    public async Task RoundTrip_TranslateAndReverse()
    {
        var provider = await CreateLoadedProvider();

        // Translate forward
        var translated = provider.TranslateKeyword(30); // if -> se
        Assert.Equal("se", translated);

        // Translate back
        var id = provider.ReverseTranslateKeyword(translated!);
        Assert.Equal(30, id);
    }
}
