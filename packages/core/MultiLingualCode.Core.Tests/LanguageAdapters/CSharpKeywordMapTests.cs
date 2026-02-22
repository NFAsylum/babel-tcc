using MultiLingualCode.Core.LanguageAdapters;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class CSharpKeywordMapTests
{
    [Fact]
    public void TextToId_Contains77Keywords()
    {
        Assert.Equal(77, CSharpKeywordMap.TextToId.Count);
    }

    [Fact]
    public void IdToText_Contains77Keywords()
    {
        Assert.Equal(77, CSharpKeywordMap.IdToText.Count);
    }

    [Fact]
    public void GetId_KnownKeyword_ReturnsId()
    {
        Assert.Equal(10, CSharpKeywordMap.GetId("class"));
        Assert.Equal(30, CSharpKeywordMap.GetId("if"));
        Assert.Equal(52, CSharpKeywordMap.GetId("return"));
        Assert.Equal(75, CSharpKeywordMap.GetId("void"));
    }

    [Fact]
    public void GetId_CaseInsensitive()
    {
        Assert.Equal(10, CSharpKeywordMap.GetId("CLASS"));
        Assert.Equal(10, CSharpKeywordMap.GetId("Class"));
    }

    [Fact]
    public void GetId_UnknownKeyword_ReturnsMinusOne()
    {
        Assert.Equal(-1, CSharpKeywordMap.GetId("notakeyword"));
    }

    [Fact]
    public void GetId_EmptyString_ReturnsMinusOne()
    {
        Assert.Equal(-1, CSharpKeywordMap.GetId(""));
    }

    [Fact]
    public void GetText_KnownId_ReturnsKeyword()
    {
        Assert.Equal("class", CSharpKeywordMap.GetText(10));
        Assert.Equal("if", CSharpKeywordMap.GetText(30));
        Assert.Equal("return", CSharpKeywordMap.GetText(52));
    }

    [Fact]
    public void GetText_UnknownId_ReturnsEmptyString()
    {
        Assert.Equal("", CSharpKeywordMap.GetText(999));
        Assert.Equal("", CSharpKeywordMap.GetText(-1));
    }

    [Fact]
    public void GetMap_ReturnsCopy()
    {
        Dictionary<string, int> map = CSharpKeywordMap.GetMap();

        map["test"] = 999;
        Assert.Equal(-1, CSharpKeywordMap.GetId("test"));
    }

    [Fact]
    public void TextToId_And_IdToText_AreConsistent()
    {
        foreach (KeyValuePair<string, int> kvp in CSharpKeywordMap.TextToId)
        {
            Assert.Equal(kvp.Key, CSharpKeywordMap.GetText(kvp.Value));
        }
    }

    [Fact]
    public void AllIds_AreUnique()
    {
        HashSet<int> ids = new HashSet<int>(CSharpKeywordMap.IdToText.Keys);
        Assert.Equal(CSharpKeywordMap.IdToText.Count, ids.Count);
    }

    [Theory]
    [InlineData("abstract", 0)]
    [InlineData("bool", 3)]
    [InlineData("namespace", 39)]
    [InlineData("while", 77)]
    public void GetId_SpecificKeywords_ReturnsExpectedId(string keyword, int expectedId)
    {
        Assert.Equal(expectedId, CSharpKeywordMap.GetId(keyword));
    }

    [Fact]
    public void Id74_IsNotUsed()
    {
        Assert.Equal("", CSharpKeywordMap.GetText(74));
    }
}
