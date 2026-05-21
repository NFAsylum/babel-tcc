using MultiLingualCode.Core.LanguageAdapters.Portugol;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.Portugol;

public class VisuAlgKeywordMapTests
{
    [Fact]
    public void TextToId_WhenAccessed_Contains48Keywords()
    {
        Assert.Equal(48, VisuAlgKeywordMap.TextToId.Count);
    }

    [Fact]
    public void IdToText_WhenAccessed_Contains48Keywords()
    {
        Assert.Equal(48, VisuAlgKeywordMap.IdToText.Count);
    }

    [Fact]
    public void GetId_KnownKeyword_ReturnsId()
    {
        Assert.Equal(0, VisuAlgKeywordMap.GetId("algoritmo"));
        Assert.Equal(1, VisuAlgKeywordMap.GetId("fimalgoritmo"));
        Assert.Equal(12, VisuAlgKeywordMap.GetId("se"));
        Assert.Equal(13, VisuAlgKeywordMap.GetId("entao"));
        Assert.Equal(21, VisuAlgKeywordMap.GetId("enquanto"));
        Assert.Equal(34, VisuAlgKeywordMap.GetId("funcao"));
        Assert.Equal(47, VisuAlgKeywordMap.GetId("debug"));
    }

    [Fact]
    public void GetId_CaseInsensitive_AcceptsAllCasings()
    {
        Assert.Equal(0, VisuAlgKeywordMap.GetId("algoritmo"));
        Assert.Equal(0, VisuAlgKeywordMap.GetId("ALGORITMO"));
        Assert.Equal(0, VisuAlgKeywordMap.GetId("Algoritmo"));
        Assert.Equal(12, VisuAlgKeywordMap.GetId("SE"));
        Assert.Equal(12, VisuAlgKeywordMap.GetId("Se"));
    }

    [Fact]
    public void GetId_UnknownKeyword_ReturnsMinusOne()
    {
        Assert.Equal(-1, VisuAlgKeywordMap.GetId("print"));
        Assert.Equal(-1, VisuAlgKeywordMap.GetId("if"));
        Assert.Equal(-1, VisuAlgKeywordMap.GetId(""));
    }

    [Fact]
    public void GetText_KnownId_ReturnsCanonicalLowercaseKeyword()
    {
        Assert.Equal("algoritmo", VisuAlgKeywordMap.GetText(0));
        Assert.Equal("fimalgoritmo", VisuAlgKeywordMap.GetText(1));
        Assert.Equal("se", VisuAlgKeywordMap.GetText(12));
        Assert.Equal("funcao", VisuAlgKeywordMap.GetText(34));
        Assert.Equal("debug", VisuAlgKeywordMap.GetText(47));
    }

    [Fact]
    public void GetText_UnknownId_ReturnsEmptyString()
    {
        Assert.Equal("", VisuAlgKeywordMap.GetText(-1));
        Assert.Equal("", VisuAlgKeywordMap.GetText(48));
        Assert.Equal("", VisuAlgKeywordMap.GetText(999));
    }

    [Fact]
    public void GetMap_ReturnsCopy_NotSameReference()
    {
        Dictionary<string, int> first = VisuAlgKeywordMap.GetMap();
        Dictionary<string, int> second = VisuAlgKeywordMap.GetMap();
        Assert.NotSame(first, second);
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public void GetMap_PreservesCaseInsensitiveComparer()
    {
        Dictionary<string, int> map = VisuAlgKeywordMap.GetMap();
        Assert.Equal(12, map["se"]);
        Assert.Equal(12, map["SE"]);
        Assert.Equal(12, map["Se"]);
    }

    [Fact]
    public void TextToId_And_IdToText_AreConsistent()
    {
        foreach (KeyValuePair<string, int> kvp in VisuAlgKeywordMap.TextToId)
        {
            Assert.Equal(kvp.Key, VisuAlgKeywordMap.GetText(kvp.Value));
        }
    }

    [Fact]
    public void IdSpace_IsContiguous_FromZeroToCountMinusOne()
    {
        for (int id = 0; id < VisuAlgKeywordMap.TextToId.Count; id++)
        {
            string text = VisuAlgKeywordMap.GetText(id);
            Assert.NotEqual("", text);
        }
    }
}
