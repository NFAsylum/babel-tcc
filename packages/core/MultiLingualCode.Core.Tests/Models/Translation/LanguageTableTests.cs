using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Tests.Models.Translation;

public class LanguageTableTests
{
    private static string GetTestDataPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    [Fact]
    public void LoadFrom_ValidFile_LoadsTranslations()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal("1.0.0", table.Version);
        Assert.Equal("pt-br", table.LanguageCode);
        Assert.Equal("Portugues Brasileiro", table.LanguageName);
        Assert.Equal("CSharp", table.ProgrammingLanguage);
        Assert.True(table.Count > 0);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            LanguageTable.LoadFrom("nonexistent.json"));
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsTranslations()
    {
        var table = await LanguageTable.LoadFromAsync(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal("pt-br", table.LanguageCode);
        Assert.True(table.Count > 0);
    }

    [Fact]
    public void GetTranslation_KnownId_ReturnsTranslation()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal("se", table.GetTranslation(30));       // if -> se
        Assert.Equal("senao", table.GetTranslation(18));     // else -> senao
        Assert.Equal("classe", table.GetTranslation(10));    // class -> classe
        Assert.Equal("vazio", table.GetTranslation(75));     // void -> vazio
    }

    [Fact]
    public void GetTranslation_UnknownId_ReturnsNull()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Null(table.GetTranslation(999));
        Assert.Null(table.GetTranslation(-1));
    }

    [Fact]
    public void GetKeywordId_KnownTranslation_ReturnsId()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal(30, table.GetKeywordId("se"));
        Assert.Equal(18, table.GetKeywordId("senao"));
        Assert.Equal(10, table.GetKeywordId("classe"));
    }

    [Fact]
    public void GetKeywordId_CaseInsensitive()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal(30, table.GetKeywordId("SE"));
        Assert.Equal(30, table.GetKeywordId("Se"));
    }

    [Fact]
    public void GetKeywordId_UnknownTranslation_ReturnsMinusOne()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal(-1, table.GetKeywordId("desconhecido"));
    }

    [Fact]
    public void GetKeywordId_EmptyOrNull_ReturnsMinusOne()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        Assert.Equal(-1, table.GetKeywordId(""));
        Assert.Equal(-1, table.GetKeywordId(null!));
    }

    [Fact]
    public void BidirectionalLookup_IsConsistent()
    {
        var table = LanguageTable.LoadFrom(GetTestDataPath("pt-br-csharp.json"));

        var translation = table.GetTranslation(52); // return
        Assert.Equal("retornar", translation);

        var id = table.GetKeywordId(translation!);
        Assert.Equal(52, id);
    }

    [Fact]
    public void EmptyTranslations_AreSkipped()
    {
        // The template has empty translations - they should not be loaded
        var table = new LanguageTable();
        table.RawTranslations = new Dictionary<string, string>
        {
            { "0", "abstrato" },
            { "1", "" },       // empty - should be skipped
            { "2", "base" }
        };

        Assert.Equal(2, table.Count);
        Assert.Null(table.GetTranslation(1));
    }
}
