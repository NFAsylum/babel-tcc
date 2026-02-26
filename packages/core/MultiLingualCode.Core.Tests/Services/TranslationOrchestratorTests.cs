using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class TranslationOrchestratorTests
{
    public LanguageRegistry _registry = new();
    public IdentifierMapper _mapper = new();
    public string _translationsPath;

    public TranslationOrchestratorTests()
    {
        _translationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
    }

    private NaturalLanguageProvider CreateProvider()
    {
        return new NaturalLanguageProvider { LanguageCode = "pt-br", TranslationsBasePath = _translationsPath };
    }

    /// <summary>
    /// Creates a mock adapter that builds a simple AST from "keywords" in the source code.
    /// Keywords "if", "else", "class", "void", "return" are recognized.
    /// Identifiers starting with uppercase are marked as translatable.
    /// Generate reconstructs text from the AST nodes.
    /// </summary>
    private static ILanguageAdapter CreateMockCSharpAdapter()
    {
        ILanguageAdapter adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("CSharp");
        adapter.FileExtensions.Returns(new[] { ".cs" });
        adapter.Version.Returns("1.0.0");

        // Parse: creates a flat AST with keyword and identifier nodes
        adapter.Parse(Arg.Any<string>()).Returns(callInfo =>
        {
            string source = callInfo.ArgAt<string>(0);
            StatementNode root = new StatementNode { StatementKind = "CompilationUnit", RawText = source };
            string[] tokens = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, int> keywordMap = new Dictionary<string, int>
            {
                ["if"] = 30, ["else"] = 18, ["class"] = 10,
                ["void"] = 75, ["return"] = 52, ["public"] = 49,
                ["static"] = 58, ["int"] = 33,
                // PT-BR translated keywords (for reverse parsing)
                ["se"] = 30, ["senao"] = 18, ["classe"] = 10,
                ["vazio"] = 75, ["retornar"] = 52, ["publico"] = 49,
                ["estatico"] = 58, ["inteiro"] = 33
            };

            foreach (string token in tokens)
            {
                string clean = token.TrimEnd('{', '}', '(', ')', ';', ',');
                if (keywordMap.TryGetValue(clean.ToLowerInvariant(), out int id))
                {
                    root.Children.Add(new KeywordNode
                    {
                        KeywordId = id,
                        Text = clean,
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
            ASTNode ast = callInfo.ArgAt<ASTNode>(0);
            List<string> parts = new List<string>();
            CollectParts(ast, parts);
            return string.Join(" ", parts);
        });

        // ReverseSubstituteKeywords: mock returns input unchanged (mock Parse already handles PT-BR)
        adapter.ReverseSubstituteKeywords(Arg.Any<string>(), Arg.Any<Func<string, int>>())
            .Returns(callInfo => callInfo.ArgAt<string>(0));

        return adapter;
    }

    private static void CollectParts(ASTNode node, List<string> parts)
    {
        switch (node)
        {
            case KeywordNode kw:
                parts.Add(kw.Text);
                break;
            case IdentifierNode id:
                parts.Add(id.Name);
                break;
        }

        foreach (ASTNode child in node.Children)
        {
            CollectParts(child, parts);
        }
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_TranslatesKeywords()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "public class Program", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("publico", result.Value);
        Assert.Contains("classe", result.Value);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_TranslatesIdentifiers()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            _mapper.LoadMap(tempDir);
            _mapper.SetTranslation("Calculator", "pt-br", "Calculadora");

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

            OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
                "public class Calculator", ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.Contains("Calculadora", result.Value);
            Assert.Contains("publico", result.Value);
            Assert.Contains("classe", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_ReversesKeywords()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            "publico classe Programa", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("public", result.Value);
        Assert.Contains("class", result.Value);
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_ReversesIdentifiers()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            _mapper.LoadMap(tempDir);
            _mapper.SetTranslation("Calculator", "pt-br", "Calculadora");

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

            OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
                "publico classe Calculadora", ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.Contains("public", result.Value);
            Assert.Contains("class", result.Value);
            Assert.Contains("Calculator", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_UnsupportedExtension_ReturnsFailure()
    {
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "code", ".xyz", "pt-br");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_UnsupportedExtension_ReturnsFailure()
    {
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            "code", ".xyz", "pt-br");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Constructor_AcceptsValidArguments()
    {
        NaturalLanguageProvider provider = CreateProvider();

        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        Assert.NotNull(orchestrator);
    }

    [Fact]
    public void Create_NullArguments_ReturnsFailure()
    {
        NaturalLanguageProvider provider = CreateProvider();

        OperationResultGeneric<TranslationOrchestrator> nullRegistryResult = TranslationOrchestrator.Create(null!, provider, _mapper);
        OperationResultGeneric<TranslationOrchestrator> nullProviderResult = TranslationOrchestrator.Create(_registry, null!, _mapper);
        OperationResultGeneric<TranslationOrchestrator> nullMapperResult = TranslationOrchestrator.Create(_registry, provider, null!);

        Assert.False(nullRegistryResult.IsSuccess);
        Assert.False(nullProviderResult.IsSuccess);
        Assert.False(nullMapperResult.IsSuccess);
    }

    [Fact]
    public async Task RoundTrip_TranslateAndReverse()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        string original = "public void Main";
        OperationResultGeneric<string> translatedResult = await orchestrator.TranslateToNaturalLanguageAsync(
            original, ".cs", "pt-br");

        Assert.True(translatedResult.IsSuccess);
        Assert.Contains("publico", translatedResult.Value);
        Assert.Contains("vazio", translatedResult.Value);

        OperationResultGeneric<string> reversedResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            translatedResult.Value, ".cs", "pt-br");

        Assert.True(reversedResult.IsSuccess);
        Assert.Contains("public", reversedResult.Value);
        Assert.Contains("void", reversedResult.Value);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_PreservesUntranslatableIdentifiers()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        // "x" starts with lowercase, so IsTranslatable = false in the mock adapter
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "int x", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("inteiro", result.Value);
        Assert.Contains("x", result.Value);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_UnknownKeywordId_PreservesOriginal()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        _registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = _registry, Provider = provider, IdentifierMapperService = _mapper };

        // "Main" is an identifier (uppercase), not a keyword - won't be in translation table
        // It should remain as "Main" since there's no identifier mapping for it
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "void Main", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("vazio", result.Value);
        Assert.Contains("Main", result.Value);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_WithTraduAnnotations_AppliesMappings()
    {
        // Uses real CSharpAdapter so tradu annotations are parsed from actual C# source
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_tradu_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceCode = @"public class Calculator // tradu:Calculadora
{
    public int Add(int a, int b) // tradu:Somar,a:primeiro,b:segundo
    {
        return a + b;
    }
}";

            OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceCode, ".cs", "pt-br");

            Assert.True(translationResult.IsSuccess);
            Assert.Contains("Calculadora", translationResult.Value);
            Assert.Contains("Somar", translationResult.Value);
            Assert.Contains("primeiro", translationResult.Value);
            Assert.Contains("segundo", translationResult.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
