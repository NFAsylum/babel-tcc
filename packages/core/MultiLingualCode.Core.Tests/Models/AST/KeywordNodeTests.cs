using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class KeywordNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        KeywordNode node = new KeywordNode
        {
            KeywordId = 30,
            Text = "if",
            StartPosition = 10,
            EndPosition = 12,
            StartLine = 1,
            EndLine = 1
        };

        KeywordNode clone = (KeywordNode)node.Clone();

        Assert.Equal(30, clone.KeywordId);
        Assert.Equal("if", clone.Text);
        Assert.Equal(10, clone.StartPosition);
        Assert.Equal(12, clone.EndPosition);
        Assert.Equal(1, clone.StartLine);
        Assert.Equal(1, clone.EndLine);
    }

    [Fact]
    public void Clone_IsDeepCopy()
    {
        KeywordNode node = new KeywordNode { KeywordId = 30, Text = "if" };
        KeywordNode clone = (KeywordNode)node.Clone();

        clone.KeywordId = 99;
        Assert.Equal(30, node.KeywordId);
    }

    [Fact]
    public void Clone_DeepClonesChildren()
    {
        KeywordNode parent = new KeywordNode { KeywordId = 30, Text = "if" };
        IdentifierNode child = new IdentifierNode { Name = "x" };
        child.Parent = parent;
        parent.Children.Add(child);

        KeywordNode clone = (KeywordNode)parent.Clone();

        Assert.Single(clone.Children);
        Assert.IsType<IdentifierNode>(clone.Children[0]);
        Assert.Equal("x", ((IdentifierNode)clone.Children[0]).Name);
        Assert.Same(clone, clone.Children[0].Parent);
        Assert.NotSame(child, clone.Children[0]);
    }
}
