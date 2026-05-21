using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Services;

public class LanguageScanRulesTests
{
    [Fact]
    public void Default_NewInstance_HasCaseSensitiveKeywords()
    {
        LanguageScanRules rules = new LanguageScanRules();
        Assert.False(rules.CaseInsensitiveKeywords);
    }

    [Fact]
    public void CSharpPreset_IsCaseSensitive()
    {
        Assert.False(LanguageScanRules.CSharp.CaseInsensitiveKeywords);
    }

    [Fact]
    public void PythonPreset_IsCaseSensitive()
    {
        Assert.False(LanguageScanRules.Python.CaseInsensitiveKeywords);
    }

    [Fact]
    public void VisuAlgPreset_IsCaseInsensitive_AndLineCommentOnly()
    {
        LanguageScanRules rules = LanguageScanRules.VisuAlg;
        Assert.True(rules.CaseInsensitiveKeywords);
        Assert.Equal("//", rules.LineComment);
        Assert.Equal("", rules.BlockCommentStart);
        Assert.False(rules.HasSingleQuoteStrings);
    }

    [Fact]
    public void PortugolStudioPreset_IsCaseSensitive_AndSupportsBlockComments()
    {
        LanguageScanRules rules = LanguageScanRules.PortugolStudio;
        Assert.False(rules.CaseInsensitiveKeywords);
        Assert.Equal("//", rules.LineComment);
        Assert.Equal("/*", rules.BlockCommentStart);
        Assert.Equal("*/", rules.BlockCommentEnd);
        Assert.True(rules.HasSingleQuoteStrings);
    }
}
