using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class KeywordNodeTests
{
    [Fact]
    public void Clone_WhenCalled_CopiesAllProperties()
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
    public void Clone_WhenModifyingClone_DoesNotAffectOriginal()
    {
        KeywordNode node = new KeywordNode { KeywordId = 30, Text = "if" };
        KeywordNode clone = (KeywordNode)node.Clone();

        clone.KeywordId = 99;
        Assert.Equal(30, node.KeywordId);
    }

    [Fact]
    public void Clone_WithChildren_DeepClonesChildNodes()
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

public class IdentifierNodeTests
{
    [Fact]
    public void Clone_WhenCalled_CopiesAllProperties()
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
    public void Clone_WhenModifyingClone_DoesNotAffectOriginal()
    {
        IdentifierNode node = new IdentifierNode { Name = "original", IsTranslatable = false };
        IdentifierNode clone = (IdentifierNode)node.Clone();

        clone.Name = "modified";
        Assert.Equal("original", node.Name);
    }

    [Fact]
    public void Constructor_WithNoArgs_SetsDefaultValues()
    {
        IdentifierNode node = new IdentifierNode();

        Assert.Equal(string.Empty, node.Name);
        Assert.False(node.IsTranslatable);
        Assert.Equal("", node.TranslatedName);
    }
}

public class LiteralNodeTests
{
    [Fact]
    public void Clone_WithStringLiteral_CopiesValueAndType()
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
    public void Clone_WithNumberLiteral_CopiesValueAndType()
    {
        LiteralNode node = new LiteralNode { Value = 42, Type = LiteralType.Number };
        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal(42, clone.Value);
        Assert.Equal(LiteralType.Number, clone.Type);
    }

    [Fact]
    public void Clone_WithBooleanLiteral_CopiesValueAndType()
    {
        LiteralNode node = new LiteralNode { Value = true, Type = LiteralType.Boolean };
        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal(true, clone.Value);
        Assert.Equal(LiteralType.Boolean, clone.Type);
    }

    [Fact]
    public void Clone_WithNullLiteral_CopiesValueAndType()
    {
        LiteralNode node = new LiteralNode { Value = "", Type = LiteralType.Null };
        LiteralNode clone = (LiteralNode)node.Clone();

        Assert.Equal("", clone.Value);
        Assert.Equal(LiteralType.Null, clone.Type);
    }
}

public class ExpressionNodeTests
{
    [Fact]
    public void Clone_WhenCalled_CopiesAllProperties()
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
    public void Clone_WhenModifyingClone_DoesNotAffectOriginal()
    {
        ExpressionNode node = new ExpressionNode { ExpressionKind = "MethodCall", RawText = "Foo()" };
        ExpressionNode clone = (ExpressionNode)node.Clone();

        clone.RawText = "Bar()";
        Assert.Equal("Foo()", node.RawText);
    }
}

public class StatementNodeTests
{
    [Fact]
    public void Clone_WhenCalled_CopiesAllProperties()
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
    public void Clone_WithNestedHierarchy_DeepClonesAllLevels()
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
