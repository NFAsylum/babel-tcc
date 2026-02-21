using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Tests.Models.Translation;

public class KeywordTableTests
{
    static string GetTestDataPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    [Fact]
    public void LoadFrom_ValidFile_LoadsKeywords()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count > 0);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ReturnsFailure()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom("nonexistent.json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsKeywords()
    {
        OperationResult<KeywordTable> result = await KeywordTable.LoadFromAsync(GetTestDataPath("keywords-base.json"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count > 0);
    }

    [Fact]
    public void GetKeywordId_KnownKeyword_ReturnsId()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(30, table.GetKeywordId("if"));
        Assert.Equal(18, table.GetKeywordId("else"));
        Assert.Equal(10, table.GetKeywordId("class"));
        Assert.Equal(75, table.GetKeywordId("void"));
    }

    [Fact]
    public void GetKeywordId_CaseInsensitive()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(30, table.GetKeywordId("IF"));
        Assert.Equal(30, table.GetKeywordId("If"));
    }

    [Fact]
    public void GetKeywordId_UnknownKeyword_ReturnsMinusOne()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(-1, table.GetKeywordId("nonexistent"));
    }

    [Fact]
    public void GetKeywordId_EmptyOrNull_ReturnsMinusOne()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(-1, table.GetKeywordId(""));
        Assert.Equal(-1, table.GetKeywordId(null!));
    }

    [Fact]
    public void GetKeyword_KnownId_ReturnsKeyword()
    {
        OperationResult<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        OperationResult<string> ifResult = table.GetKeyword(30);
        Assert.True(ifResult.IsSuccess);
        Assert.Equal("if", ifResult.Value);

        OperationResult<string> elseResult = table.GetKeyword(18);
        Assert.True(elseResult.IsSuccess);
        Assert.Equal("else", elseResult.Value);

        OperationResult<string> classResult = table.GetKeyword(10);
        Assert.True(classResult.IsSuccess);
        Assert.Equal("class", classResult.Value);
    }

    [Fact]
    public void GetKeyword_UnknownId_ReturnsFailure()
    {
        OperationResult<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        OperationResult<string> result999 = table.GetKeyword(999);
        Assert.False(result999.IsSuccess);

        OperationResult<string> resultNeg = table.GetKeyword(-1);
        Assert.False(resultNeg.IsSuccess);
    }

    [Fact]
    public void Count_ReturnsCorrectNumber()
    {
        OperationResult<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        // keywords-base.json has 77 entries (IDs 0-77, skipping 74)
        Assert.Equal(77, table.Count);
    }

    [Fact]
    public void BidirectionalLookup_IsConsistent()
    {
        OperationResult<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        int id = table.GetKeywordId("return");
        Assert.NotEqual(-1, id);

        OperationResult<string> keywordResult = table.GetKeyword(id);
        Assert.True(keywordResult.IsSuccess);
        Assert.Equal("return", keywordResult.Value);
    }
}
