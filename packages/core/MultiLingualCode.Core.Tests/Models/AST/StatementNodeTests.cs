using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class StatementNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        StatementNode node = new StatementNode
        {
            StatementKind = "IfStatement",
            RawText = "if (x) { }"
        };

        StatementNode clone = (StatementNode)node.Clone();

        Assert.Equal("IfStatement", clone.StatementKind);
        Assert.Equal("if (x) { }", clone.RawText);
    }

    [Fact]
    public void Clone_DeepClonesNestedHierarchy()
    {
        StatementNode ifStmt = new StatementNode { StatementKind = "IfStatement" };
        KeywordNode keyword = new KeywordNode { KeywordId = 30, Text = "if" };
        keyword.Parent = ifStmt;
        ifStmt.Children.Add(keyword);

        StatementNode block = new StatementNode { StatementKind = "Block" };
        block.Parent = ifStmt;
        ifStmt.Children.Add(block);

        ExpressionNode innerExpr = new ExpressionNode { ExpressionKind = "Assignment", RawText = "x = 1" };
        innerExpr.Parent = block;
        block.Children.Add(innerExpr);

        StatementNode clone = (StatementNode)ifStmt.Clone();

        Assert.Equal(2, clone.Children.Count);
        Assert.IsType<KeywordNode>(clone.Children[0]);
        Assert.IsType<StatementNode>(clone.Children[1]);

        StatementNode clonedBlock = (StatementNode)clone.Children[1];
        Assert.Single(clonedBlock.Children);
        Assert.IsType<ExpressionNode>(clonedBlock.Children[0]);
        Assert.Equal("x = 1", ((ExpressionNode)clonedBlock.Children[0]).RawText);

        // Verify parent references
        Assert.Same(clone, clone.Children[0].Parent);
        Assert.Same(clone, clonedBlock.Parent);
        Assert.Same(clonedBlock, clonedBlock.Children[0].Parent);

        // Verify deep copy (not same instances)
        Assert.NotSame(ifStmt, clone);
        Assert.NotSame(keyword, clone.Children[0]);
        Assert.NotSame(block, clonedBlock);
    }
}
