using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Tests.Models.Translation;

public class KeywordTableTests
{
    private static string GetTestDataPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    [Fact]
    public void LoadFrom_ValidFile_LoadsKeywords()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.True(table.Count > 0);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            KeywordTable.LoadFrom("nonexistent.json"));
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsKeywords()
    {
        var table = await KeywordTable.LoadFromAsync(GetTestDataPath("keywords-base.json"));

        Assert.True(table.Count > 0);
    }

    [Fact]
    public void GetKeywordId_KnownKeyword_ReturnsId()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.Equal(30, table.GetKeywordId("if"));
        Assert.Equal(18, table.GetKeywordId("else"));
        Assert.Equal(10, table.GetKeywordId("class"));
        Assert.Equal(75, table.GetKeywordId("void"));
    }

    [Fact]
    public void GetKeywordId_CaseInsensitive()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.Equal(30, table.GetKeywordId("IF"));
        Assert.Equal(30, table.GetKeywordId("If"));
    }

    [Fact]
    public void GetKeywordId_UnknownKeyword_ReturnsMinusOne()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.Equal(-1, table.GetKeywordId("nonexistent"));
    }

    [Fact]
    public void GetKeywordId_EmptyOrNull_ReturnsMinusOne()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.Equal(-1, table.GetKeywordId(""));
        Assert.Equal(-1, table.GetKeywordId(null!));
    }

    [Fact]
    public void GetKeyword_KnownId_ReturnsKeyword()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.Equal("if", table.GetKeyword(30));
        Assert.Equal("else", table.GetKeyword(18));
        Assert.Equal("class", table.GetKeyword(10));
    }

    [Fact]
    public void GetKeyword_UnknownId_ReturnsNull()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.Null(table.GetKeyword(999));
        Assert.Null(table.GetKeyword(-1));
    }

    [Fact]
    public void Count_ReturnsCorrectNumber()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        // keywords-base.json has 77 entries (IDs 0-77, skipping 74)
        Assert.Equal(77, table.Count);
    }

    [Fact]
    public void BidirectionalLookup_IsConsistent()
    {
        var table = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        var id = table.GetKeywordId("return");
        Assert.NotEqual(-1, id);

        var keyword = table.GetKeyword(id);
        Assert.Equal("return", keyword);
    }
}
