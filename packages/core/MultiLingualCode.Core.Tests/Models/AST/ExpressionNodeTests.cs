using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class ExpressionNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        ExpressionNode node = new ExpressionNode
        {
            ExpressionKind = "BinaryExpression",
            RawText = "a + b"
        };

        ExpressionNode clone = (ExpressionNode)node.Clone();

        Assert.Equal("BinaryExpression", clone.ExpressionKind);
        Assert.Equal("a + b", clone.RawText);
    }

    [Fact]
    public void Clone_IsDeepCopy()
    {
        ExpressionNode node = new ExpressionNode { ExpressionKind = "MethodCall", RawText = "Foo()" };
        ExpressionNode clone = (ExpressionNode)node.Clone();

        clone.RawText = "Bar()";
        Assert.Equal("Foo()", node.RawText);
    }
}
