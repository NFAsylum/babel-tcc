using MultiLingualCode.Core.LanguageAdapters.Portugol;

namespace MultiLingualCode.Core.Tests.LanguageAdapters.Portugol;

public class PortugolStudioKeywordMapTests
{
    [Fact]
    public void TextToId_WhenAccessed_Contains26Keywords()
    {
        Assert.Equal(26, PortugolStudioKeywordMap.TextToId.Count);
    }

    [Fact]
    public void IdToText_WhenAccessed_Contains26Keywords()
    {
        Assert.Equal(26, PortugolStudioKeywordMap.IdToText.Count);
    }

    [Fact]
    public void GetId_KnownKeyword_ReturnsId()
    {
        Assert.Equal(0, PortugolStudioKeywordMap.GetId("inteiro"));
        Assert.Equal(6, PortugolStudioKeywordMap.GetId("se"));
        Assert.Equal(8, PortugolStudioKeywordMap.GetId("enquanto"));
        Assert.Equal(16, PortugolStudioKeywordMap.GetId("programa"));
        Assert.Equal(17, PortugolStudioKeywordMap.GetId("funcao"));
        Assert.Equal(25, PortugolStudioKeywordMap.GetId("falso"));
    }

    [Fact]
    public void GetId_CaseSensitive_RejectsMixedCase()
    {
        Assert.Equal(6, PortugolStudioKeywordMap.GetId("se"));
        Assert.Equal(-1, PortugolStudioKeywordMap.GetId("SE"));
        Assert.Equal(-1, PortugolStudioKeywordMap.GetId("Se"));
        Assert.Equal(-1, PortugolStudioKeywordMap.GetId("PROGRAMA"));
    }

    [Fact]
    public void GetId_UnknownKeyword_ReturnsMinusOne()
    {
        Assert.Equal(-1, PortugolStudioKeywordMap.GetId("var"));
        Assert.Equal(-1, PortugolStudioKeywordMap.GetId("escreva"));
        Assert.Equal(-1, PortugolStudioKeywordMap.GetId(""));
    }

    [Fact]
    public void GetText_KnownId_ReturnsKeyword()
    {
        Assert.Equal("inteiro", PortugolStudioKeywordMap.GetText(0));
        Assert.Equal("se", PortugolStudioKeywordMap.GetText(6));
        Assert.Equal("programa", PortugolStudioKeywordMap.GetText(16));
        Assert.Equal("funcao", PortugolStudioKeywordMap.GetText(17));
        Assert.Equal("falso", PortugolStudioKeywordMap.GetText(25));
    }

    [Fact]
    public void GetText_UnknownId_ReturnsEmptyString()
    {
        Assert.Equal("", PortugolStudioKeywordMap.GetText(-1));
        Assert.Equal("", PortugolStudioKeywordMap.GetText(26));
        Assert.Equal("", PortugolStudioKeywordMap.GetText(999));
    }

    [Fact]
    public void GetMap_ReturnsCopy_NotSameReference()
    {
        Dictionary<string, int> first = PortugolStudioKeywordMap.GetMap();
        Dictionary<string, int> second = PortugolStudioKeywordMap.GetMap();
        Assert.NotSame(first, second);
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public void TextToId_And_IdToText_AreConsistent()
    {
        foreach (KeyValuePair<string, int> kvp in PortugolStudioKeywordMap.TextToId)
        {
            Assert.Equal(kvp.Key, PortugolStudioKeywordMap.GetText(kvp.Value));
        }
    }

    [Fact]
    public void IdSpace_IsContiguous_FromZeroToCountMinusOne()
    {
        for (int id = 0; id < PortugolStudioKeywordMap.TextToId.Count; id++)
        {
            string text = PortugolStudioKeywordMap.GetText(id);
            Assert.NotEqual("", text);
        }
    }
}
