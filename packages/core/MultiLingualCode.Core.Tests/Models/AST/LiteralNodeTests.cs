using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class LiteralNodeTests
{
    [Fact]
    public void Clone_WithStringValue_CopiesStringLiteral()
    {
        LiteralNode node = new LiteralNode
        {
            Value = "hello",
            Type = LiteralType.String,
            IsTranslatable = true
        };

        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal("hello", clone.Value);
        Assert.Equal(LiteralType.String, clone.Type);
        Assert.True(clone.IsTranslatable);
    }

    [Fact]
    public void Clone_WithNumberValue_CopiesNumberLiteral()
    {
        LiteralNode node = new LiteralNode { Value = 42, Type = LiteralType.Number };
        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal(42, clone.Value);
        Assert.Equal(LiteralType.Number, clone.Type);
    }

    [Fact]
    public void Clone_WithBooleanValue_CopiesBooleanLiteral()
    {
        LiteralNode node = new LiteralNode { Value = true, Type = LiteralType.Boolean };
        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal(true, clone.Value);
        Assert.Equal(LiteralType.Boolean, clone.Type);
    }

    [Fact]
    public void Clone_WithNullValue_CopiesNullLiteral()
    {
        LiteralNode node = new LiteralNode { Value = "", Type = LiteralType.Null };
        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal("", clone.Value);
        Assert.Equal(LiteralType.Null, clone.Type);
    }
}
