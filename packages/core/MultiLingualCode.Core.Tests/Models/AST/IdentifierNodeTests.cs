using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class IdentifierNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        IdentifierNode node = new IdentifierNode
        {
            Name = "calculator",
            IsTranslatable = true,
            TranslatedName = "calculadora",
            StartPosition = 0,
            EndPosition = 10,
            StartLine = 0,
            EndLine = 0
        };

        IdentifierNode clone = (IdentifierNode)node.Clone();

        Assert.Equal("calculator", clone.Name);
        Assert.True(clone.IsTranslatable);
        Assert.Equal("calculadora", clone.TranslatedName);
        Assert.Equal(0, clone.StartPosition);
        Assert.Equal(10, clone.EndPosition);
    }

    [Fact]
    public void Clone_IsDeepCopy()
    {
        IdentifierNode node = new IdentifierNode { Name = "original", IsTranslatable = false };
        IdentifierNode clone = (IdentifierNode)node.Clone();

        clone.Name = "modified";
        Assert.Equal("original", node.Name);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        IdentifierNode node = new IdentifierNode();

        Assert.Equal(string.Empty, node.Name);
        Assert.False(node.IsTranslatable);
        Assert.Equal("", node.TranslatedName);
    }
}
