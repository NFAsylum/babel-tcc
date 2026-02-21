using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Models.AST;

public class KeywordNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        var node = new KeywordNode
        {
            KeywordId = 30,
            OriginalKeyword = "if",
            StartPosition = 10,
            EndPosition = 12,
            StartLine = 1,
            EndLine = 1
        };

        var clone = (KeywordNode)node.Clone();

        Assert.Equal(30, clone.KeywordId);
        Assert.Equal("if", clone.OriginalKeyword);
        Assert.Equal(10, clone.StartPosition);
        Assert.Equal(12, clone.EndPosition);
        Assert.Equal(1, clone.StartLine);
        Assert.Equal(1, clone.EndLine);
    }

    [Fact]
    public void Clone_IsDeepCopy()
    {
        var node = new KeywordNode { KeywordId = 30, OriginalKeyword = "if" };
        var clone = (KeywordNode)node.Clone();

        clone.KeywordId = 99;
        Assert.Equal(30, node.KeywordId);
    }

    [Fact]
    public void Clone_DeepClonesChildren()
    {
        var parent = new KeywordNode { KeywordId = 30, OriginalKeyword = "if" };
        var child = new IdentifierNode { Name = "x" };
        child.Parent = parent;
        parent.Children.Add(child);

        var clone = (KeywordNode)parent.Clone();

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
    public void Clone_CopiesAllProperties()
    {
        var node = new IdentifierNode
        {
            Name = "calculator",
            IsTranslatable = true,
            TranslatedName = "calculadora",
            StartPosition = 0,
            EndPosition = 10,
            StartLine = 0,
            EndLine = 0
        };

        var clone = (IdentifierNode)node.Clone();

        Assert.Equal("calculator", clone.Name);
        Assert.True(clone.IsTranslatable);
        Assert.Equal("calculadora", clone.TranslatedName);
        Assert.Equal(0, clone.StartPosition);
        Assert.Equal(10, clone.EndPosition);
    }

    [Fact]
    public void Clone_IsDeepCopy()
    {
        var node = new IdentifierNode { Name = "original", IsTranslatable = false };
        var clone = (IdentifierNode)node.Clone();

        clone.Name = "modified";
        Assert.Equal("original", node.Name);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var node = new IdentifierNode();

        Assert.Equal(string.Empty, node.Name);
        Assert.False(node.IsTranslatable);
        Assert.Null(node.TranslatedName);
    }
}

public class LiteralNodeTests
{
    [Fact]
    public void Clone_CopiesStringLiteral()
    {
        var node = new LiteralNode
        {
            Value = "hello",
            Type = LiteralType.String,
            IsTranslatable = true
        };

        var clone = (LiteralNode)node.Clone();

        Assert.Equal("hello", clone.Value);
        Assert.Equal(LiteralType.String, clone.Type);
        Assert.True(clone.IsTranslatable);
    }

    [Fact]
    public void Clone_CopiesNumberLiteral()
    {
        var node = new LiteralNode { Value = 42, Type = LiteralType.Number };
        var clone = (LiteralNode)node.Clone();

        Assert.Equal(42, clone.Value);
        Assert.Equal(LiteralType.Number, clone.Type);
    }

    [Fact]
    public void Clone_CopiesBooleanLiteral()
    {
        var node = new LiteralNode { Value = true, Type = LiteralType.Boolean };
        var clone = (LiteralNode)node.Clone();

        Assert.Equal(true, clone.Value);
        Assert.Equal(LiteralType.Boolean, clone.Type);
    }

    [Fact]
    public void Clone_CopiesNullLiteral()
    {
        var node = new LiteralNode { Value = null, Type = LiteralType.Null };
        var clone = (LiteralNode)node.Clone();

        Assert.Null(clone.Value);
        Assert.Equal(LiteralType.Null, clone.Type);
    }
}

public class ExpressionNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        var node = new ExpressionNode
        {
            ExpressionKind = "BinaryExpression",
            RawText = "a + b"
        };

        var clone = (ExpressionNode)node.Clone();

        Assert.Equal("BinaryExpression", clone.ExpressionKind);
        Assert.Equal("a + b", clone.RawText);
    }

    [Fact]
    public void Clone_IsDeepCopy()
    {
        var node = new ExpressionNode { ExpressionKind = "MethodCall", RawText = "Foo()" };
        var clone = (ExpressionNode)node.Clone();

        clone.RawText = "Bar()";
        Assert.Equal("Foo()", node.RawText);
    }
}

public class StatementNodeTests
{
    [Fact]
    public void Clone_CopiesAllProperties()
    {
        var node = new StatementNode
        {
            StatementKind = "IfStatement",
            RawText = "if (x) { }"
        };

        var clone = (StatementNode)node.Clone();

        Assert.Equal("IfStatement", clone.StatementKind);
        Assert.Equal("if (x) { }", clone.RawText);
    }

    [Fact]
    public void Clone_DeepClonesNestedHierarchy()
    {
        var ifStmt = new StatementNode { StatementKind = "IfStatement" };
        var keyword = new KeywordNode { KeywordId = 30, OriginalKeyword = "if" };
        keyword.Parent = ifStmt;
        ifStmt.Children.Add(keyword);

        var block = new StatementNode { StatementKind = "Block" };
        block.Parent = ifStmt;
        ifStmt.Children.Add(block);

        var innerExpr = new ExpressionNode { ExpressionKind = "Assignment", RawText = "x = 1" };
        innerExpr.Parent = block;
        block.Children.Add(innerExpr);

        var clone = (StatementNode)ifStmt.Clone();

        Assert.Equal(2, clone.Children.Count);
        Assert.IsType<KeywordNode>(clone.Children[0]);
        Assert.IsType<StatementNode>(clone.Children[1]);

        var clonedBlock = (StatementNode)clone.Children[1];
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
