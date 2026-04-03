using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Tests.Utilities;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("GetValue", "getValue")]
    [InlineData("getValue", "getValue")]
    [InlineData("X", "x")]
    [InlineData("ABC", "abc")]
    [InlineData("HTMLParser", "htmlParser")]
    [InlineData("getID", "getId")]
    [InlineData("", "")]
    public void ToCamelCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, input.ToCamelCase());
    }

    [Theory]
    [InlineData("getValue", "GetValue")]
    [InlineData("get_value", "GetValue")]
    [InlineData("get-value", "GetValue")]
    [InlineData("GetValue", "GetValue")]
    [InlineData("x", "X")]
    [InlineData("HTMLParser", "HtmlParser")]
    [InlineData("", "")]
    public void ToPascalCase_WithVariousInputs_ConvertsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, input.ToPascalCase());
    }

    [Theory]
    [InlineData("getValue", new[] { "get", "Value" })]
    [InlineData("get_value", new[] { "get", "value" })]
    [InlineData("get-value", new[] { "get", "value" })]
    [InlineData("HTMLParser", new[] { "HTML", "Parser" })]
    [InlineData("simpleword", new[] { "simpleword" })]
    [InlineData("ABC", new[] { "ABC" })]
    [InlineData("", new string[0])]
    public void SplitIntoWords_WithVariousFormats_SplitsCorrectly(string input, string[] expected)
    {
        Assert.Equal(expected, input.SplitIntoWords());
    }

    [Fact]
    public void ToCamelCase_WithNullInput_ReturnsNull()
    {
        string value = null!;
        Assert.Null(value!.ToCamelCase());
    }

    [Fact]
    public void ToPascalCase_WithNullInput_ReturnsNull()
    {
        string value = null!;
        Assert.Null(value!.ToPascalCase());
    }
}
