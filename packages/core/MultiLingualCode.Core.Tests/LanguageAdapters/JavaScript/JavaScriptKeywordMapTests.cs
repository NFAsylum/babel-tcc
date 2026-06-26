using MultiLingualCode.Core.LanguageAdapters.JavaScript;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.JavaScript;

public class JavaScriptKeywordMapTests
{
    [Fact]
    public void TextToId_WhenAccessed_Contains38Keywords()
    {
        Assert.Equal(38, JavaScriptKeywordMap.TextToId.Count);
    }

    [Fact]
    public void IdToText_WhenAccessed_Contains38Keywords()
    {
        Assert.Equal(38, JavaScriptKeywordMap.IdToText.Count);
    }

    [Fact]
    public void GetId_KnownKeyword_ReturnsId()
    {
        Assert.Equal(0, JavaScriptKeywordMap.GetId("async"));
        Assert.Equal(5, JavaScriptKeywordMap.GetId("class"));
        Assert.Equal(17, JavaScriptKeywordMap.GetId("function"));
        Assert.Equal(18, JavaScriptKeywordMap.GetId("if"));
        Assert.Equal(34, JavaScriptKeywordMap.GetId("var"));
        Assert.Equal(37, JavaScriptKeywordMap.GetId("yield"));
    }

    [Fact]
    public void GetId_IsCaseSensitive_RejectsWrongCasing()
    {
        Assert.Equal(5, JavaScriptKeywordMap.GetId("class"));
        Assert.Equal(-1, JavaScriptKeywordMap.GetId("Class"));
        Assert.Equal(-1, JavaScriptKeywordMap.GetId("CLASS"));
        Assert.Equal(-1, JavaScriptKeywordMap.GetId("Function"));
    }

    [Fact]
    public void GetId_UnknownKeyword_ReturnsMinusOne()
    {
        Assert.Equal(-1, JavaScriptKeywordMap.GetId("foreach"));
        Assert.Equal(-1, JavaScriptKeywordMap.GetId("of"));
        Assert.Equal(-1, JavaScriptKeywordMap.GetId(""));
    }

    [Fact]
    public void GetText_KnownId_ReturnsCanonicalKeyword()
    {
        Assert.Equal("async", JavaScriptKeywordMap.GetText(0));
        Assert.Equal("class", JavaScriptKeywordMap.GetText(5));
        Assert.Equal("function", JavaScriptKeywordMap.GetText(17));
        Assert.Equal("yield", JavaScriptKeywordMap.GetText(37));
    }

    [Fact]
    public void GetText_UnknownId_ReturnsEmptyString()
    {
        Assert.Equal("", JavaScriptKeywordMap.GetText(-1));
        Assert.Equal("", JavaScriptKeywordMap.GetText(38));
        Assert.Equal("", JavaScriptKeywordMap.GetText(999));
    }

    [Fact]
    public void GetMap_ReturnsCopy_NotSameReference()
    {
        Dictionary<string, int> first = JavaScriptKeywordMap.GetMap();
        Dictionary<string, int> second = JavaScriptKeywordMap.GetMap();
        Assert.NotSame(first, second);
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public void GetMap_PreservesCaseSensitiveComparer()
    {
        Dictionary<string, int> map = JavaScriptKeywordMap.GetMap();
        Assert.Equal(5, map["class"]);
        Assert.False(map.ContainsKey("Class"));
    }

    [Fact]
    public void TextToId_And_IdToText_AreConsistent()
    {
        foreach (KeyValuePair<string, int> kvp in JavaScriptKeywordMap.TextToId)
        {
            Assert.Equal(kvp.Key, JavaScriptKeywordMap.GetText(kvp.Value));
        }
    }

    [Fact]
    public void IdSpace_IsContiguous_FromZeroToCountMinusOne()
    {
        for (int id = 0; id < JavaScriptKeywordMap.TextToId.Count; id++)
        {
            string text = JavaScriptKeywordMap.GetText(id);
            Assert.NotEqual("", text);
        }
    }
}