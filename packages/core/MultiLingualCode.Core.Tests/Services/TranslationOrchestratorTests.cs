using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class TranslationOrchestratorTests
{
    public LanguageRegistry Registry = new();
    public IdentifierMapper Mapper = new();
    public string TranslationsPath;

    public TranslationOrchestratorTests()
    {
        TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
    }

    public NaturalLanguageProvider CreateProvider()
    {
        return new NaturalLanguageProvider { LanguageCode = "pt-br", TranslationsBasePath = TranslationsPath };
    }

    /// <summary>
    /// Creates a mock adapter that builds a simple AST from "keywords" in the source code.
    /// Keywords "if", "else", "class", "void", "return" are recognized.
    /// Identifiers starting with uppercase are marked as translatable.
    /// Generate reconstructs text from the AST nodes.
    /// </summary>
    public static ILanguageAdapter CreateMockCSharpAdapter()
    {
        ILanguageAdapter adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("CSharp");
        adapter.FileExtensions.Returns(new[] { ".cs" });
        adapter.Version.Returns("1.0.0");
        adapter.ExtractTrailingComments(Arg.Any<string>()).Returns(new List<TrailingComment>());
        adapter.GetIdentifierNamesOnLine(Arg.Any<string>(), Arg.Any<int>()).Returns(new List<string>());
        adapter.GetFirstStringLiteralOnLine(Arg.Any<string>(), Arg.Any<int>()).Returns("");
        adapter.GetContainingMethodRange(Arg.Any<string>(), Arg.Any<int>()).Returns((-1, -1));

        // Parse: creates a flat AST with keyword and identifier nodes
        adapter.Parse(Arg.Any<string>()).Returns(callInfo =>
        {
            string source = callInfo.ArgAt<string>(0);
            StatementNode root = new StatementNode { StatementKind = "CompilationUnit", RawText = source };
            string[] tokens = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, int> keywordMap = new Dictionary<string, int>
            {
                ["if"] = 30,
                ["else"] = 18,
                ["class"] = 10,
                ["void"] = 75,
                ["return"] = 52,
                ["public"] = 49,
                ["static"] = 58,
                ["int"] = 33,
                // PT-BR translated keywords (for reverse parsing)
                ["se"] = 30,
                ["senao"] = 18,
                ["classe"] = 10,
                ["vazio"] = 75,
                ["retornar"] = 52,
                ["publico"] = 49,
                ["estatico"] = 58,
                ["inteiro"] = 33
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

    public static void CollectParts(ASTNode node, List<string> parts)
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
    public async Task TranslateToNaturalLanguage_WithCSharpCode_TranslatesKeywordsToPtBr()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        Registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "public class Program", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("publico", result.Value);
        Assert.Contains("classe", result.Value);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_WithTraduAnnotations_TranslatesIdentifiers()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
}";

            OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceCode, ".cs", "pt-br");

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
    public async Task TranslateFromNaturalLanguage_WithTranslatedCode_ReversesKeywords()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        Registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            "publico classe Programa", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("public", result.Value);
        Assert.Contains("class", result.Value);
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_WithMappedIdentifiers_ReversesIdentifiers()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        Registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            Mapper.LoadMap(tempDir);
            Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

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
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "code", ".xyz", "pt-br");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_UnsupportedExtension_ReturnsFailure()
    {
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            "code", ".xyz", "pt-br");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        NaturalLanguageProvider provider = CreateProvider();

        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        Assert.NotNull(orchestrator);
    }

    [Fact]
    public void Create_NullArguments_ReturnsFailure()
    {
        NaturalLanguageProvider provider = CreateProvider();

        OperationResultGeneric<TranslationOrchestrator> nullRegistryResult = TranslationOrchestrator.Create(null!, provider, Mapper);
        OperationResultGeneric<TranslationOrchestrator> nullProviderResult = TranslationOrchestrator.Create(Registry, null!, Mapper);
        OperationResultGeneric<TranslationOrchestrator> nullMapperResult = TranslationOrchestrator.Create(Registry, provider, null!);

        Assert.False(nullRegistryResult.IsSuccess);
        Assert.False(nullProviderResult.IsSuccess);
        Assert.False(nullMapperResult.IsSuccess);
    }

    [Fact]
    public async Task RoundTrip_ForwardAndReverse_PreservesKeywords()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        Registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

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
    public async Task TranslateToNaturalLanguage_LowercaseIdentifier_PreservesUntranslatable()
    {
        ILanguageAdapter adapter = CreateMockCSharpAdapter();
        Registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

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
        Registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

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

            string sourceCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
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

    [Fact]
    public async Task TranslateToNaturalLanguage_ScopedParams_FieldsNotAffected()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_scoped_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
    public int a;
    public int b;

    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
    {
        return a + b;
    }
}";

            OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceCode, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            string translated = result.Value;

            Assert.Contains("Somar", translated);
            Assert.Contains("primeiro", translated);
            Assert.Contains("segundo", translated);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task TranslateFromNaturalLanguage_WithTraduAnnotations_RestoresOriginalNames()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_reverse_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);
            mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
            mapper.SetTranslation("Add", "pt-br", "Somar");

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string translatedCode = @"publico classe Calculadora // tradu[pt-br]:Calculadora
{
    publico inteiro Somar(inteiro a, inteiro b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
    {
        retornar a + b;
    }
}";

            OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
                translatedCode, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            string restored = result.Value;

            Assert.Contains("Calculator", restored);
            Assert.Contains("Add", restored);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ApplyTraduAnnotations_MultiLanguage_SetsBothLanguages()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_multi_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);
            NaturalLanguageProvider provider = CreateProvider();

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceCode = @"
public class Calculator // tradu[pt-br]:Calculadora|[es]:Calculadora
{
}";

            orchestrator.ApplyTraduAnnotations(sourceCode, "pt-br", new CSharpAdapter());

            OperationResultGeneric<string> ptResult = mapper.GetTranslation("Calculator", "pt-br");
            Assert.True(ptResult.IsSuccess);
            Assert.Equal("Calculadora", ptResult.Value);

            OperationResultGeneric<string> esResult = mapper.GetTranslation("Calculator", "es");
            Assert.True(esResult.IsSuccess);
            Assert.Equal("Calculadora", esResult.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTraduAnnotations_RemovedAnnotation_ClearsStaleMapping()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_stale_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceWithTradu = @"public class Calculator // tradu[pt-br]:Calculadora
{
}";

            OperationResultGeneric<string> firstResult = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceWithTradu, ".cs", "pt-br");

            Assert.True(firstResult.IsSuccess);
            Assert.Contains("Calculadora", firstResult.Value);

            string sourceWithoutTradu = @"public class Calculator
{
}";

            OperationResultGeneric<string> secondResult = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceWithoutTradu, ".cs", "pt-br");

            Assert.True(secondResult.IsSuccess);
            Assert.DoesNotContain("Calculadora", secondResult.Value);
            Assert.Contains("Calculator", secondResult.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTraduAnnotations_AfterTranslation_DoesNotPersistToDisk()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_nopersist_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            // Pre-populate identifier-map with existing mapping
            mapper.SetTranslation("ExistingClass", "pt-br", "ClasseExistente");
            mapper.SaveMap();

            string mapFilePath = Path.Combine(tempDir, ".multilingual", "identifier-map.json");
            string contentBefore = File.ReadAllText(mapFilePath);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceCode = @"public class NewClass // tradu[pt-br]:NovaClasse
{
}";

            OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceCode, ".cs", "pt-br");
            Assert.True(result.IsSuccess);

            // Verify file on disk was NOT modified by translation
            string contentAfter = File.ReadAllText(mapFilePath);
            Assert.Equal(contentBefore, contentAfter);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ModifyTraduInTranslatedCode_ReverseAndForward_UsesNewTradu()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_modtradu_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            // 1. Forward: traduz com tradu original
            string originalSource = @"public class Calculator // tradu[pt-br]:Calculadora
{
}";
            OperationResultGeneric<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(
                originalSource, ".cs", "pt-br");
            Assert.True(forwardResult.IsSuccess);
            Assert.Contains("Calculadora", forwardResult.Value);

            // 2. Utilizador modifica o tradu no código traduzido
            string modifiedTranslated = forwardResult.Value.Replace(
                "tradu[pt-br]:Calculadora", "tradu[pt-br]:Calc");

            // 3. Reverse: converte de volta para original
            OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
                modifiedTranslated, ".cs", "pt-br");
            Assert.True(reverseResult.IsSuccess);
            Assert.Contains("Calculator", reverseResult.Value);
            Assert.Contains("tradu[pt-br]:Calc", reverseResult.Value);

            // 4. Forward novamente: deve usar o novo tradu
            OperationResultGeneric<string> secondForward = await orchestrator.TranslateToNaturalLanguageAsync(
                reverseResult.Value, ".cs", "pt-br");
            Assert.True(secondForward.IsSuccess);
            Assert.Contains("Calc", secondForward.Value);
            Assert.DoesNotContain("Calculadora", secondForward.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindScopedTranslation_InsideMethodRange_ReturnsTranslation()
    {
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        orchestrator.ScopedTranslations.Add(("a", "primeiro", 3, 7));

        string result = orchestrator.FindScopedTranslation("a", 5);
        Assert.Equal("primeiro", result);
    }

    [Fact]
    public void FindScopedTranslation_OutsideMethodRange_ReturnsNull()
    {
        NaturalLanguageProvider provider = CreateProvider();
        TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = Registry, Provider = provider, IdentifierMapperService = Mapper };

        orchestrator.ScopedTranslations.Add(("a", "primeiro", 3, 7));

        string result = orchestrator.FindScopedTranslation("a", 1);
        Assert.Null(result);
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_FieldOutsideMethod_NotTranslatedByParamMapping()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_field_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string sourceCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
    public int a;

    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
    {
        return a + b;
    }
}";

            OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
                sourceCode, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            string translated = result.Value;

            Assert.Contains("Somar", translated);
            Assert.Contains("primeiro", translated);

            // Field "a" outside the method should NOT be translated to "primeiro"
            // Count occurrences of "primeiro" — should only appear inside the method scope
            int primeiroCount = 0;
            int index = 0;
            while ((index = translated.IndexOf("primeiro", index)) >= 0)
            {
                primeiroCount++;
                index += "primeiro".Length;
            }

            // "a" appears as param in Add (translated to "primeiro") and as "a + b" usage inside the method
            // But the field "public int a;" should remain as "a", not become "primeiro"
            // We verify the field declaration line still has "a" by checking mapper has no global mapping for "a"
            OperationResultGeneric<string> globalMapping = mapper.GetTranslation("a", "pt-br");
            Assert.False(globalMapping.IsSuccess);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // === Testes multi-linguagem: verificam perda de dados via ApplyTraduAnnotations ===

    [Fact]
    public void ApplyTraduAnnotations_SequentialFiles_ClearsMemoryBetweenCalls()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_multi_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);
            NaturalLanguageProvider provider = CreateProvider();
            TranslationOrchestrator orchestrator = new TranslationOrchestrator
            {
                Registry = Registry,
                Provider = provider,
                IdentifierMapperService = mapper
            };

            string csCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
    public int Add(int a, int b) // tradu[pt-br]:Somar
    {
        return a + b;
    }
}";

            // First file: C# with tradu annotations
            orchestrator.ApplyTraduAnnotations(csCode, "pt-br", new CSharpAdapter());
            Assert.True(mapper.GetTranslation("Calculator", "pt-br").IsSuccess);
            Assert.True(mapper.GetTranslation("Add", "pt-br").IsSuccess);

            // Second file: different annotations (simulates translating another file)
            string csCode2 = @"public class Student // tradu[pt-br]:Aluno
{
}";
            orchestrator.ApplyTraduAnnotations(csCode2, "pt-br", new CSharpAdapter());

            // After second call: Clear() destroyed Calculator and Add from memory
            Assert.False(mapper.GetTranslation("Calculator", "pt-br").IsSuccess,
                "Calculator mapping should be cleared after second ApplyTraduAnnotations call");
            Assert.False(mapper.GetTranslation("Add", "pt-br").IsSuccess,
                "Add mapping should be cleared after second ApplyTraduAnnotations call");
            Assert.True(mapper.GetTranslation("Student", "pt-br").IsSuccess);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ApplyTraduAnnotations_SequentialFiles_DestroysPersistedData()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_persist_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);
            NaturalLanguageProvider provider = CreateProvider();
            TranslationOrchestrator orchestrator = new TranslationOrchestrator
            {
                Registry = Registry,
                Provider = provider,
                IdentifierMapperService = mapper
            };

            string csCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
}";
            // Translate first file — saves to disk
            orchestrator.ApplyTraduAnnotations(csCode, "pt-br", new CSharpAdapter());
            mapper.SaveMap();

            // Verify data is on disk
            IdentifierMapper verify1 = new IdentifierMapper();
            verify1.LoadMap(tempDir);
            Assert.True(verify1.GetTranslation("Calculator", "pt-br").IsSuccess,
                "Calculator should be persisted after first save");

            // Translate second file — Clear() + rebuild + SaveMap
            string csCode2 = @"public class Student // tradu[pt-br]:Aluno
{
}";
            orchestrator.ApplyTraduAnnotations(csCode2, "pt-br", new CSharpAdapter());
            mapper.SaveMap();

            // Verify data on disk: Calculator was DESTROYED
            IdentifierMapper verify2 = new IdentifierMapper();
            verify2.LoadMap(tempDir);
            Assert.False(verify2.GetTranslation("Calculator", "pt-br").IsSuccess,
                "Calculator mapping was permanently destroyed from disk by second ApplyTraduAnnotations + SaveMap");
            Assert.True(verify2.GetTranslation("Student", "pt-br").IsSuccess);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ApplyTraduAnnotations_RoundTripMultiFile_SecondFileBreaksFirst()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_rt_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);
            NaturalLanguageProvider provider = CreateProvider();
            TranslationOrchestrator orchestrator = new TranslationOrchestrator
            {
                Registry = Registry,
                Provider = provider,
                IdentifierMapperService = mapper
            };

            // Translate .cs file
            string csCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
}";
            orchestrator.ApplyTraduAnnotations(csCode, "pt-br", new CSharpAdapter());

            // Verify forward translation works
            Assert.Equal("Calculadora", mapper.GetTranslation("Calculator", "pt-br").Value);

            // Translate second .cs file
            string csCode2 = @"public class Student // tradu[pt-br]:Aluno
{
}";
            orchestrator.ApplyTraduAnnotations(csCode2, "pt-br", new CSharpAdapter());

            // Reverse translation of first file: Calculator mapping is GONE
            Assert.False(mapper.GetOriginal("Calculadora", "pt-br").IsSuccess,
                "Reverse lookup for Calculadora fails because Clear() destroyed the mapping");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // ========================================================================
    // ApplyTranslatedEditsAsync (diff-based reverse, tarefa 085)
    // ========================================================================

    [Fact]
    public async Task ApplyTranslatedEdits_UnchangedLines_CopiedFromOriginal()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_diff_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string original = "public class Foo\n{\n    public int x;\n}";
            string translated = "publico classe Foo\n{\n    publico inteiro x;\n}";
            string edited = "publico classe Foo\n{\n    publico inteiro x;\n}"; // no changes

            OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
                original, translated, edited, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.Equal(original, result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTranslatedEdits_ModifiedLine_ReverseTranslated()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_diff_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string original = "public class Foo\n{\n}";
            string translated = "publico classe Foo\n{\n}";
            string edited = "publico classe Bar\n{\n}"; // changed Foo to Bar

            OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
                original, translated, edited, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.Contains("public class Bar", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTranslatedEdits_AddedLine_ReverseTranslated()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_diff_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string original = "public class Foo\n{\n}";
            string translated = "publico classe Foo\n{\n}";
            string edited = "publico classe Foo\n{\n    publico inteiro x;\n}"; // added line

            OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
                original, translated, edited, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.Contains("public int x;", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTranslatedEdits_RemovedLine_RemovedFromOriginal()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_diff_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string original = "public class Foo\n{\n    public int x;\n}";
            string translated = "publico classe Foo\n{\n    publico inteiro x;\n}";
            string edited = "publico classe Foo\n{\n}"; // removed line

            OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
                original, translated, edited, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.DoesNotContain("int x", result.Value);
            Assert.Contains("class Foo", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTranslatedEdits_VariableE_NotCorruptedToAnd()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_diff_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            // "e" is a common variable in C# (catch Exception e)
            // The translation of "is" (ID 36) is "igual" in pt-br-ascii,
            // but if it were "e", the old scanner would corrupt variable "e" to "is"
            string original = "try { } catch (Exception e) { }";
            string translated = "tentar { } capturar (Exception e) { }";
            string edited = "tentar { } capturar (Exception e) { }"; // no changes

            OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
                original, translated, edited, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            // Variable "e" should NOT be corrupted
            Assert.Contains("Exception e", result.Value);
            Assert.DoesNotContain("Exception is", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyTranslatedEdits_NewFile_AllLinesReverseTranslated()
    {
        CSharpAdapter realAdapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(realAdapter);
        NaturalLanguageProvider provider = CreateProvider();

        string tempDir = Path.Combine(Path.GetTempPath(), $"orch_diff_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            IdentifierMapper mapper = new IdentifierMapper();
            mapper.LoadMap(tempDir);

            TranslationOrchestrator orchestrator = new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };

            string original = ""; // new file
            string translated = ""; // no previous translation
            string edited = "publico classe Foo\n{\n}"; // user wrote from scratch

            OperationResultGeneric<string> result = await orchestrator.ApplyTranslatedEditsAsync(
                original, translated, edited, ".cs", "pt-br");

            Assert.True(result.IsSuccess);
            Assert.Contains("public class Foo", result.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
