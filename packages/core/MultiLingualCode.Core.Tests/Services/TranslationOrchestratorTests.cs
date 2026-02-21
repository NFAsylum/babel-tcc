using MultiLingualCode.Core.Exceptions;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class TranslationOrchestratorTests
{
    private readonly LanguageRegistry _registry = new();
    private readonly IdentifierMapper _mapper = new();
    private readonly string _translationsPath;

    public TranslationOrchestratorTests()
    {
        _translationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
    }

    private NaturalLanguageProvider CreateProvider()
    {
        return new NaturalLanguageProvider("pt-br", _translationsPath);
    }

    /// <summary>
    /// Creates a mock adapter that builds a simple AST from "keywords" in the source code.
    /// Keywords "if", "else", "class", "void", "return" are recognized.
    /// Identifiers starting with uppercase are marked as translatable.
    /// Generate reconstructs text from the AST nodes.
    /// </summary>
    private static ILanguageAdapter CreateMockCSharpAdapter()
    {
        var adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("CSharp");
        adapter.FileExtensions.Returns(new[] { ".cs" });
        adapter.Version.Returns("1.0.0");

        // Parse: creates a flat AST with keyword and identifier nodes
        adapter.Parse(Arg.Any<string>()).Returns(callInfo =>
        {
            var source = callInfo.ArgAt<string>(0);
            var root = new StatementNode { StatementKind = "CompilationUnit", RawText = source };
            var tokens = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var keywordMap = new Dictionary<string, int>
            {
                ["if"] = 30, ["else"] = 18, ["class"] = 10,
                ["void"] = 75, ["return"] = 52, ["public"] = 49,
                ["static"] = 58, ["int"] = 33,
                // PT-BR translated keywords (for reverse parsing)
                ["se"] = 30, ["senao"] = 18, ["classe"] = 10,
                ["vazio"] = 75, ["retornar"] = 52, ["publico"] = 49,
                ["estatico"] = 58, ["inteiro"] = 33
            };

            foreach (var token in tokens)
            {
                var clean = token.TrimEnd('{', '}', '(', ')', ';', ',');
                if (keywordMap.TryGetValue(clean.ToLowerInvariant(), out var id))
                {
                    root.Children.Add(new KeywordNode
                    {
                        KeywordId = id,
                        OriginalKeyword = clean,
                        Parent = root
                    });
                }
                else if (clean.Length > 0 && char.IsLetter(clean[0]))
                {
                    root.Children.Add(new IdentifierNode
                    {
                        Name = clean,
                        IsTranslatable = char.IsUpper(clean[0]),
                        Parent = root
                    });
                }
            }

            return root;
        });

        // Generate: reconstructs code from AST node keywords/identifiers
        adapter.Generate(Arg.Any<ASTNode>()).Returns(callInfo =>
        {
            var ast = callInfo.ArgAt<ASTNode>(0);
            var parts = new List<string>();
            CollectParts(ast, parts);
            return string.Join(" ", parts);
        });

        return adapter;
    }

    private static void CollectParts(ASTNode node, List<string> parts)
    {
        switch (node)
        {
            case KeywordNode kw:
                parts.Add(kw.OriginalKeyword);
                break;
            case IdentifierNode id:
                parts.Add(id.Name);
                break;
        }

        foreach (var child in node.Children)
            CollectParts(child, parts);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_TranslatesKeywords()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();

        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        var result = await orchestrator.TranslateToNaturalLanguageAsync(
            "public class Program", ".cs", "pt-br");

        Assert.Contains("publico", result);
        Assert.Contains("classe", result);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_TranslatesIdentifiers()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();

        var tempDir = Path.Combine(Path.GetTempPath(), $"orch_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            _mapper.LoadMap(tempDir);
            _mapper.SetTranslation("Calculator", "pt-br", "Calculadora");

            var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

            var result = await orchestrator.TranslateToNaturalLanguageAsync(
                "public class Calculator", ".cs", "pt-br");

            Assert.Contains("Calculadora", result);
            Assert.Contains("publico", result);
            Assert.Contains("classe", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_ReversesKeywords()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();

        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        var result = await orchestrator.TranslateFromNaturalLanguageAsync(
            "publico classe Programa", ".cs", "pt-br");

        Assert.Contains("public", result);
        Assert.Contains("class", result);
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_ReversesIdentifiers()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();

        var tempDir = Path.Combine(Path.GetTempPath(), $"orch_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            _mapper.LoadMap(tempDir);
            _mapper.SetTranslation("Calculator", "pt-br", "Calculadora");

            var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

            var result = await orchestrator.TranslateFromNaturalLanguageAsync(
                "publico classe Calculadora", ".cs", "pt-br");

            Assert.Contains("public", result);
            Assert.Contains("class", result);
            Assert.Contains("Calculator", result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_UnsupportedExtension_Throws()
    {
        var provider = CreateProvider();
        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        var ex = await Assert.ThrowsAsync<UnsupportedLanguageException>(() =>
            orchestrator.TranslateToNaturalLanguageAsync("code", ".xyz", "pt-br"));

        Assert.Equal(".xyz", ex.FileExtension);
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_UnsupportedExtension_Throws()
    {
        var provider = CreateProvider();
        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        var ex = await Assert.ThrowsAsync<UnsupportedLanguageException>(() =>
            orchestrator.TranslateFromNaturalLanguageAsync("code", ".xyz", "pt-br"));

        Assert.Equal(".xyz", ex.FileExtension);
    }

    [Fact]
    public void Constructor_ThrowsOnNullArguments()
    {
        var provider = CreateProvider();

        Assert.Throws<ArgumentNullException>(() =>
            new TranslationOrchestrator(null!, provider, _mapper));
        Assert.Throws<ArgumentNullException>(() =>
            new TranslationOrchestrator(_registry, null!, _mapper));
        Assert.Throws<ArgumentNullException>(() =>
            new TranslationOrchestrator(_registry, provider, null!));
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_ThrowsOnNullArguments()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();
        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orchestrator.TranslateToNaturalLanguageAsync(null!, ".cs", "pt-br"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orchestrator.TranslateToNaturalLanguageAsync("code", null!, "pt-br"));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orchestrator.TranslateToNaturalLanguageAsync("code", ".cs", null!));
    }

    [Fact]
    public async Task RoundTrip_TranslateAndReverse()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();
        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        var original = "public void Main";
        var translated = await orchestrator.TranslateToNaturalLanguageAsync(
            original, ".cs", "pt-br");

        Assert.Contains("publico", translated);
        Assert.Contains("vazio", translated);

        var reversed = await orchestrator.TranslateFromNaturalLanguageAsync(
            translated, ".cs", "pt-br");

        Assert.Contains("public", reversed);
        Assert.Contains("void", reversed);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_PreservesUntranslatableIdentifiers()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();
        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        // "x" starts with lowercase, so IsTranslatable = false in the mock adapter
        var result = await orchestrator.TranslateToNaturalLanguageAsync(
            "int x", ".cs", "pt-br");

        Assert.Contains("inteiro", result);
        Assert.Contains("x", result);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_UnknownKeywordId_PreservesOriginal()
    {
        var adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        var provider = CreateProvider();
        var orchestrator = new TranslationOrchestrator(_registry, provider, _mapper);

        // "Main" is an identifier (uppercase), not a keyword - won't be in translation table
        // It should remain as "Main" since there's no identifier mapping for it
        var result = await orchestrator.TranslateToNaturalLanguageAsync(
            "void Main", ".cs", "pt-br");

        Assert.Contains("vazio", result);
        Assert.Contains("Main", result);
    }
}
