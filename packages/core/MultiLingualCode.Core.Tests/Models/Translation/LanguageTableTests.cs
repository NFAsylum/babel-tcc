using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Tests.Models.Translation;

public class LanguageTableTests
{
    static string PtBrCSharpPath() =>
        Path.Combine(TranslationsPathResolver.Resolve(), "natural-languages", "pt-br", "csharp.json");

    [Fact]
    public void LoadFrom_ValidFile_LoadsTranslations()
    {
        OperationResultGeneric<LanguageTable> result = LanguageTable.LoadFrom(PtBrCSharpPath());

        Assert.True(result.IsSuccess);
        LanguageTable table = result.Value;
        Assert.Equal("1.0.0", table.Version);
        Assert.Equal("pt-br", table.LanguageCode);
        Assert.Equal("Português (Brasil)", table.LanguageName);
        Assert.Equal("CSharp", table.ProgrammingLanguage);
        Assert.True(table.Count > 0);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ReturnsFailure()
    {
        OperationResultGeneric<LanguageTable> result = LanguageTable.LoadFrom("nonexistent.json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsTranslations()
    {
        OperationResultGeneric<LanguageTable> result = await LanguageTable.LoadFromAsync(PtBrCSharpPath());

        Assert.True(result.IsSuccess);
        LanguageTable table = result.Value;
        Assert.Equal("pt-br", table.LanguageCode);
        Assert.True(table.Count > 0);
    }

    [Fact]
    public void GetTranslation_KnownId_ReturnsTranslation()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        OperationResultGeneric<string> ifResult = table.GetTranslation(30);       // if -> se
        Assert.True(ifResult.IsSuccess);
        Assert.Equal("se", ifResult.Value);

        OperationResultGeneric<string> elseResult = table.GetTranslation(18);     // else -> senão
        Assert.True(elseResult.IsSuccess);
        Assert.Equal("senão", elseResult.Value);

        OperationResultGeneric<string> classResult = table.GetTranslation(10);    // class -> classe
        Assert.True(classResult.IsSuccess);
        Assert.Equal("classe", classResult.Value);

        OperationResultGeneric<string> voidResult = table.GetTranslation(75);     // void -> vazio
        Assert.True(voidResult.IsSuccess);
        Assert.Equal("vazio", voidResult.Value);
    }

    [Fact]
    public void GetTranslation_UnknownId_ReturnsFailure()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        OperationResultGeneric<string> result999 = table.GetTranslation(999);
        Assert.False(result999.IsSuccess);

        OperationResultGeneric<string> resultNeg = table.GetTranslation(-1);
        Assert.False(resultNeg.IsSuccess);
    }

    [Fact]
    public void GetKeywordId_KnownTranslation_ReturnsId()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        Assert.Equal(30, table.GetKeywordId("se"));
        Assert.Equal(18, table.GetKeywordId("senão"));
        Assert.Equal(10, table.GetKeywordId("classe"));
    }

    [Fact]
    public void GetKeywordId_WithDifferentCasing_ReturnsSameId()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        Assert.Equal(30, table.GetKeywordId("SE"));
        Assert.Equal(30, table.GetKeywordId("Se"));
    }

    [Fact]
    public void GetKeywordId_UnknownTranslation_ReturnsMinusOne()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        Assert.Equal(-1, table.GetKeywordId("desconhecido"));
    }

    [Fact]
    public void GetKeywordId_EmptyOrNull_ReturnsMinusOne()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        Assert.Equal(-1, table.GetKeywordId(""));
        Assert.Equal(-1, table.GetKeywordId(null!));
    }

    [Fact]
    public void GetTranslation_AfterGetKeywordId_RoundTripsConsistently()
    {
        OperationResultGeneric<LanguageTable> loadResult = LanguageTable.LoadFrom(PtBrCSharpPath());
        Assert.True(loadResult.IsSuccess);
        LanguageTable table = loadResult.Value;

        OperationResultGeneric<string> translationResult = table.GetTranslation(52); // return
        Assert.True(translationResult.IsSuccess);
        Assert.Equal("retornar", translationResult.Value);

        int id = table.GetKeywordId(translationResult.Value);
        Assert.Equal(52, id);
    }

    [Fact]
    public void RawTranslations_WithEmptyValues_SkipsEmptyEntries()
    {
        // The template has empty translations - they should not be loaded
        LanguageTable table = new LanguageTable();
        table.RawTranslations = new Dictionary<string, string>
        {
            { "0", "abstrato" },
            { "1", "" },       // empty - should be skipped
            { "2", "base" }
        };

        Assert.Equal(2, table.Count);

        OperationResultGeneric<string> result = table.GetTranslation(1);
        Assert.False(result.IsSuccess);
    }
}
