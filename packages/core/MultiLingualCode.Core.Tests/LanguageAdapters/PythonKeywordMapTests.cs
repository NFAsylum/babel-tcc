using MultiLingualCode.Core.LanguageAdapters.Python;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class PythonKeywordMapTests
{
    [Fact]
    public void TextToId_WhenAccessed_Contains35Keywords()
    {
        Assert.Equal(35, PythonKeywordMap.TextToId.Count);
    }

    [Fact]
    public void IdToText_WhenAccessed_Contains35Keywords()
    {
        Assert.Equal(35, PythonKeywordMap.IdToText.Count);
    }

    [Fact]
    public void GetId_KnownKeyword_ReturnsId()
    {
        Assert.Equal(0, PythonKeywordMap.GetId("False"));
        Assert.Equal(1, PythonKeywordMap.GetId("None"));
        Assert.Equal(2, PythonKeywordMap.GetId("True"));
        Assert.Equal(11, PythonKeywordMap.GetId("def"));
        Assert.Equal(20, PythonKeywordMap.GetId("if"));
        Assert.Equal(30, PythonKeywordMap.GetId("return"));
        Assert.Equal(34, PythonKeywordMap.GetId("yield"));
    }

    [Fact]
    public void GetId_CaseSensitive_TrueNotTrue()
    {
        Assert.Equal(2, PythonKeywordMap.GetId("True"));
        Assert.Equal(-1, PythonKeywordMap.GetId("true"));
        Assert.Equal(-1, PythonKeywordMap.GetId("TRUE"));
    }

    [Fact]
    public void GetId_CaseSensitive_FalseNotFalse()
    {
        Assert.Equal(0, PythonKeywordMap.GetId("False"));
        Assert.Equal(-1, PythonKeywordMap.GetId("false"));
    }

    [Fact]
    public void GetId_CaseSensitive_NoneNotNone()
    {
        Assert.Equal(1, PythonKeywordMap.GetId("None"));
        Assert.Equal(-1, PythonKeywordMap.GetId("none"));
    }

    [Fact]
    public void GetId_UnknownKeyword_ReturnsMinusOne()
    {
        Assert.Equal(-1, PythonKeywordMap.GetId("print"));
        Assert.Equal(-1, PythonKeywordMap.GetId("match"));
        Assert.Equal(-1, PythonKeywordMap.GetId("self"));
    }

    [Fact]
    public void GetText_KnownId_ReturnsKeyword()
    {
        Assert.Equal("False", PythonKeywordMap.GetText(0));
        Assert.Equal("None", PythonKeywordMap.GetText(1));
        Assert.Equal("True", PythonKeywordMap.GetText(2));
        Assert.Equal("def", PythonKeywordMap.GetText(11));
        Assert.Equal("yield", PythonKeywordMap.GetText(34));
    }

    [Fact]
    public void GetText_UnknownId_ReturnsEmptyString()
    {
        Assert.Equal("", PythonKeywordMap.GetText(-1));
        Assert.Equal("", PythonKeywordMap.GetText(35));
        Assert.Equal("", PythonKeywordMap.GetText(99));
    }

    [Fact]
    public void GetMap_ReturnsCopy_NotSameReference()
    {
        Dictionary<string, int> map1 = PythonKeywordMap.GetMap();
        Dictionary<string, int> map2 = PythonKeywordMap.GetMap();
        Assert.NotSame(map1, map2);
    }

    [Fact]
    public void GetMap_MutatingCopy_DoesNotAffectOriginal()
    {
        Dictionary<string, int> map = PythonKeywordMap.GetMap();
        map["fake"] = 999;
        Assert.False(PythonKeywordMap.TextToId.ContainsKey("fake"));
    }

    [Fact]
    public void GetText_AfterGetId_RoundTripsConsistently()
    {
        foreach (KeyValuePair<string, int> kvp in PythonKeywordMap.TextToId)
        {
            Assert.Equal(kvp.Key, PythonKeywordMap.GetText(kvp.Value));
            Assert.Equal(kvp.Value, PythonKeywordMap.GetId(kvp.Key));
        }
    }

    [Fact]
    public void GetMap_WhenComparedToPython3Spec_ContainsAllHardKeywords()
    {
        string[] expected = {
            "False", "None", "True", "and", "as", "assert", "async", "await",
            "break", "class", "continue", "def", "del", "elif", "else", "except",
            "finally", "for", "from", "global", "if", "import", "in", "is",
            "lambda", "nonlocal", "not", "or", "pass", "raise", "return",
            "try", "while", "with", "yield"
        };
        Dictionary<string, int> map = PythonKeywordMap.GetMap();
        foreach (string kw in expected)
        {
            Assert.True(map.ContainsKey(kw), $"Missing keyword: {kw}");
        }
        Assert.Equal(expected.Length, map.Count);
    }

    [Fact]
    public void GetText_WithIds0To34_ReturnsNonEmptyForAll()
    {
        for (int i = 0; i <= 34; i++)
        {
            Assert.NotEqual("", PythonKeywordMap.GetText(i));
        }
    }
}
