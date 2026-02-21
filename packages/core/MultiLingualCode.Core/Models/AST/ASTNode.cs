namespace MultiLingualCode.Core.Models.AST;

/// <summary>
/// Base class for all AST (Abstract Syntax Tree) nodes.
/// Concrete node types (KeywordNode, IdentifierNode, etc.) are defined in tarefa007.
/// </summary>
public abstract class ASTNode
{
    /// <summary>
    /// Start position (character offset) in the source code.
    /// </summary>
    public int StartPosition { get; set; }

    /// <summary>
    /// End position (character offset) in the source code.
    /// </summary>
    public int EndPosition { get; set; }

    /// <summary>
    /// Parent node in the AST hierarchy. Null for the root node.
    /// </summary>
    public ASTNode? Parent { get; set; }

    /// <summary>
    /// Child nodes in the AST hierarchy.
    /// </summary>
    public List<ASTNode> Children { get; set; } = new();

    /// <summary>
    /// Creates a deep copy of this node.
    /// </summary>
    public abstract ASTNode Clone();
}
