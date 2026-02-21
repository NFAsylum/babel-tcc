using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Interfaces;

/// <summary>
/// Verifies that a mock implementation can satisfy the ILanguageAdapter contract.
/// </summary>
public class ILanguageAdapterContractTests
{
    private readonly MockLanguageAdapter _adapter = new();

    [Fact]
    public void Properties_AreAccessible()
    {
        Assert.Equal("MockLang", _adapter.LanguageName);
        Assert.Single(_adapter.FileExtensions);
        Assert.Equal(".mock", _adapter.FileExtensions[0]);
        Assert.Equal("1.0.0", _adapter.Version);
    }

    [Fact]
    public void Parse_ReturnsASTNode()
    {
        var result = _adapter.Parse("test code");
        Assert.NotNull(result);
    }

    [Fact]
    public void Generate_ReturnsString()
    {
        var node = new MockASTNode();
        var result = _adapter.Generate(node);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetKeywordMap_ReturnsDictionary()
    {
        var map = _adapter.GetKeywordMap();
        Assert.NotNull(map);
        Assert.NotEmpty(map);
    }

    [Fact]
    public void ValidateSyntax_ReturnsValidationResult()
    {
        var result = _adapter.ValidateSyntax("test");
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ExtractIdentifiers_ReturnsList()
    {
        var result = _adapter.ExtractIdentifiers("test");
        Assert.NotNull(result);
    }

    private class MockASTNode : ASTNode
    {
        public override ASTNode Clone() => new MockASTNode();
    }

    private class MockLanguageAdapter : ILanguageAdapter
    {
        public string LanguageName => "MockLang";
        public string[] FileExtensions => [".mock"];
        public string Version => "1.0.0";

        public ASTNode Parse(string sourceCode) => new MockASTNode();
        public string Generate(ASTNode ast) => "generated";
        public Dictionary<string, int> GetKeywordMap() => new() { { "if", 30 } };
        public ValidationResult ValidateSyntax(string sourceCode) => new() { IsValid = true };
        public List<string> ExtractIdentifiers(string sourceCode) => new() { "test" };
    }
}
