using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;

namespace MultiLingualCode.Core.Tests.Interfaces;

public class ILanguageAdapterContractTests
{
    public MockLanguageAdapter Adapter = new();

    [Fact]
    public void Properties_AreAccessible()
    {
        Assert.Equal("MockLang", Adapter.LanguageName);
        Assert.Single(Adapter.FileExtensions);
        Assert.Equal(".mock", Adapter.FileExtensions[0]);
        Assert.Equal("1.0.0", Adapter.Version);
    }

    [Fact]
    public void Parse_ReturnsASTNode()
    {
        ASTNode result = Adapter.Parse("test code");
        Assert.NotNull(result);
    }

    [Fact]
    public void Generate_ReturnsString()
    {
        MockASTNode node = new MockASTNode();
        string result = Adapter.Generate(node);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetKeywordMap_ReturnsDictionary()
    {
        Dictionary<string, int> map = Adapter.GetKeywordMap();
        Assert.NotNull(map);
        Assert.NotEmpty(map);
    }

    [Fact]
    public void ValidateSyntax_ReturnsValidationResult()
    {
        ValidationResult result = Adapter.ValidateSyntax("test");
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ExtractIdentifiers_ReturnsList()
    {
        List<string> result = Adapter.ExtractIdentifiers("test");
        Assert.NotNull(result);
    }

    public class MockASTNode : ASTNode
    {
        public override ASTNode Clone() => new MockASTNode();
    }

    public class MockLanguageAdapter : ILanguageAdapter
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
