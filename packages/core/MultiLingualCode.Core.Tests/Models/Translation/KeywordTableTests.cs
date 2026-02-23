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
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count > 0);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ReturnsFailure()
    {
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom("nonexistent.json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsKeywords()
    {
        OperationResultGeneric<KeywordTable> result = await KeywordTable.LoadFromAsync(GetTestDataPath("keywords-base.json"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count > 0);
    }

    [Fact]
    public void GetKeywordId_KnownKeyword_ReturnsId()
    {
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
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
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(30, table.GetKeywordId("IF"));
        Assert.Equal(30, table.GetKeywordId("If"));
    }

    [Fact]
    public void GetKeywordId_UnknownKeyword_ReturnsMinusOne()
    {
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(-1, table.GetKeywordId("nonexistent"));
    }

    [Fact]
    public void GetKeywordId_EmptyOrNull_ReturnsMinusOne()
    {
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        Assert.Equal(-1, table.GetKeywordId(""));
        Assert.Equal(-1, table.GetKeywordId(null!));
    }

    [Fact]
    public void GetKeyword_KnownId_ReturnsKeyword()
    {
        OperationResultGeneric<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        OperationResultGeneric<string> ifResult = table.GetKeyword(30);
        Assert.True(ifResult.IsSuccess);
        Assert.Equal("if", ifResult.Value);

        OperationResultGeneric<string> elseResult = table.GetKeyword(18);
        Assert.True(elseResult.IsSuccess);
        Assert.Equal("else", elseResult.Value);

        OperationResultGeneric<string> classResult = table.GetKeyword(10);
        Assert.True(classResult.IsSuccess);
        Assert.Equal("class", classResult.Value);
    }

    [Fact]
    public void GetKeyword_UnknownId_ReturnsFailure()
    {
        OperationResultGeneric<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        OperationResultGeneric<string> result999 = table.GetKeyword(999);
        Assert.False(result999.IsSuccess);

        OperationResultGeneric<string> resultNeg = table.GetKeyword(-1);
        Assert.False(resultNeg.IsSuccess);
    }

    [Fact]
    public void Count_ReturnsCorrectNumber()
    {
        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(result.IsSuccess);
        KeywordTable table = result.Value;

        // keywords-base.json has 77 entries (IDs 0-77, skipping 74)
        Assert.Equal(77, table.Count);
    }

    [Fact]
    public void BidirectionalLookup_IsConsistent()
    {
        OperationResultGeneric<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        int id = table.GetKeywordId("return");
        Assert.NotEqual(-1, id);

        OperationResultGeneric<string> keywordResult = table.GetKeyword(id);
        Assert.True(keywordResult.IsSuccess);
        Assert.Equal("return", keywordResult.Value);
    }

    [Fact]
    public void Keywords_SetProperty_BuildsBothDictionaries()
    {
        KeywordTable table = new KeywordTable();
        table.Keywords = new Dictionary<string, int>
        {
            ["test"] = 100,
            ["hello"] = 200
        };

        Assert.Equal(100, table.GetKeywordId("test"));
        Assert.Equal(200, table.GetKeywordId("hello"));

        OperationResultGeneric<string> result = table.GetKeyword(100);
        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void Keywords_EmptyDictionary_CountIsZero()
    {
        KeywordTable table = new KeywordTable();
        table.Keywords = new Dictionary<string, int>();

        Assert.Equal(0, table.Count);
        Assert.Equal(-1, table.GetKeywordId("anything"));
    }

    [Fact]
    public void LoadFrom_InvalidJsonFile_ReturnsFailure()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "invalid-keywords-" + Guid.NewGuid() + ".json");
        File.WriteAllText(tempPath, "{ invalid json content }}}");

        try
        {
            OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(tempPath);
            Assert.False(result.IsSuccess);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void LoadFrom_EmptyJsonFile_ReturnsFailureOrEmptyTable()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "empty-keywords-" + Guid.NewGuid() + ".json");
        File.WriteAllText(tempPath, "{}");

        try
        {
            OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(tempPath);
            if (result.IsSuccess)
            {
                Assert.Equal(0, result.Value.Count);
            }
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void AllKeywordIds_AreNonNegative()
    {
        OperationResultGeneric<KeywordTable> loadResult = KeywordTable.LoadFrom(GetTestDataPath("keywords-base.json"));
        Assert.True(loadResult.IsSuccess);
        KeywordTable table = loadResult.Value;

        foreach (KeyValuePair<string, int> kvp in table.KeywordToId)
        {
            Assert.True(kvp.Value >= 0, $"Keyword '{kvp.Key}' has negative id: {kvp.Value}");
        }
    }
}
