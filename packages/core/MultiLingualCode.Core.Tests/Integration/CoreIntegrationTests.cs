using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Integration;

public class CoreIntegrationTests : IDisposable
{
    public string _translationsPath;
    public string _tempDir;

    public CoreIntegrationTests()
    {
        _translationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        _tempDir = Path.Combine(Path.GetTempPath(), $"core_integ_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    public TranslationOrchestrator CreateOrchestrator(IdentifierMapper mapper)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", _translationsPath);
        return new TranslationOrchestrator(registry, provider, mapper);
    }

    [Fact]
    public async Task HelloWorld_TranslatesToPtBr()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"using System;

namespace HelloWorld
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
        string translated = translationResult.Value;

        Assert.Contains("usando", translated);
        Assert.Contains("espaconome", translated);
        Assert.Contains("classe", translated);
        Assert.Contains("estatico", translated);
        Assert.Contains("vazio", translated);
    }

    [Fact]
    public async Task HelloWorld_RoundTrip_IdentifiersPreserved()
    {
        // Round-trip with real CSharpAdapter: translated keywords become identifiers
        // in Roslyn (e.g. "usando" is not a C# keyword). Identifier round-trip works
        // because IdentifierMapper stores bidirectional mappings.
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        mapper.SetTranslation("Program", "pt-br", "Programa");
        mapper.SetTranslation("Main", "pt-br", "Principal");
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"class Program
{
    static void Main()
    {
    }
}";

        OperationResult<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);

        // Forward: identifiers translated
        Assert.Contains("Programa", forwardResult.Value);
        Assert.Contains("Principal", forwardResult.Value);
        Assert.Contains("classe", forwardResult.Value);

        // Reverse: identifiers restored
        OperationResult<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        Assert.True(reverseResult.IsSuccess);

        Assert.Contains("Program", reverseResult.Value);
        Assert.Contains("Main", reverseResult.Value);
    }

    [Fact]
    public async Task Calculator_WithTraduAnnotations_TranslatesIdentifiers()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Calculator // tradu:Calculadora
{
    public int operationCount; // tradu:contagemOperacoes

    public int Add(int a, int b) // tradu:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }

    public int Subtract(int a, int b) // tradu:Subtrair,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a - b;
    }
}";

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
        string translated = translationResult.Value;

        // Keywords translated
        Assert.Contains("publico", translated);
        Assert.Contains("classe", translated);
        Assert.Contains("inteiro", translated);
        Assert.Contains("retornar", translated);

        // Identifiers translated via tradu
        Assert.Contains("Calculadora", translated);
        Assert.Contains("contagemOperacoes", translated);
        Assert.Contains("Somar", translated);
        Assert.Contains("Subtrair", translated);
        Assert.Contains("primeiroNumero", translated);
        Assert.Contains("segundoNumero", translated);
    }

    [Fact]
    public async Task Calculator_WithTraduLiteral_TranslatesStringLiteral()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Reporter // tradu:Relator
{
    public string GetLabel() // tradu:ObterRotulo
    {
        string label = ""Total operations: ""; // tradu:""Total de operacoes: ""
        return label;
    }
}";

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
        string translated = translationResult.Value;

        Assert.Contains("Relator", translated);
        Assert.Contains("ObterRotulo", translated);
        Assert.Contains("Total de operacoes: ", translated);
    }

    [Fact]
    public async Task ComplexClass_WithNamespace_TranslatesCorrectly()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        mapper.SetTranslation("MathUtils", "pt-br", "UtilMatematica");
        mapper.SetTranslation("Square", "pt-br", "Quadrado");
        mapper.SetTranslation("Abs", "pt-br", "Absoluto");
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"using System;

namespace MyApp
{
    public class MathUtils
    {
        public static int Square(int n)
        {
            return n * n;
        }

        public static int Abs(int n)
        {
            if (n < 0)
            {
                return -n;
            }
            return n;
        }
    }
}";

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
        string translated = translationResult.Value;

        Assert.Contains("usando", translated);
        Assert.Contains("espaconome", translated);
        Assert.Contains("publico", translated);
        Assert.Contains("classe", translated);
        Assert.Contains("estatico", translated);
        Assert.Contains("inteiro", translated);
        Assert.Contains("se", translated);
        Assert.Contains("retornar", translated);

        Assert.Contains("UtilMatematica", translated);
        Assert.Contains("Quadrado", translated);
        Assert.Contains("Absoluto", translated);
    }

    [Fact]
    public async Task MultipleAnnotations_AllApplied()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Student // tradu:Aluno
{
    public string Name; // tradu:Nome
    public int Grade; // tradu:Nota

    public bool IsPassing() // tradu:EstaAprovado
    {
        return Grade >= 60;
    }
}";

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
        string translated = translationResult.Value;

        Assert.Contains("Aluno", translated);
        Assert.Contains("Nome", translated);
        Assert.Contains("Nota", translated);
        Assert.Contains("EstaAprovado", translated);
        Assert.Contains("booleano", translated);
    }

    [Fact]
    public async Task IfElseForWhile_AllKeywordsTranslated()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Logic
{
    public void Run()
    {
        int x = 0;
        if (x > 0)
        {
            return;
        }
        else
        {
            while (x < 10)
            {
                x++;
            }
        }
        for (int i = 0; i < 5; i++)
        {
        }
    }
}";

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
        string translated = translationResult.Value;

        Assert.Contains("se", translated);
        Assert.Contains("senao", translated);
        Assert.Contains("enquanto", translated);
        Assert.Contains("para", translated);
        Assert.Contains("retornar", translated);
        Assert.Contains("vazio", translated);
        Assert.Contains("inteiro", translated);
    }

    [Fact]
    public async Task Performance_MediumFile_CompletesQuickly()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        // Generate a ~100 line class
        string sourceCode = @"using System;
using System.Collections.Generic;

namespace Performance.Test
{
    public class DataProcessor
    {
        public int Count = 0;
        public List<int> Items = new List<int>();

        public void ProcessAll()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                int value = Items[i];
                if (value > 0)
                {
                    Count++;
                }
                else if (value < 0)
                {
                    Count--;
                }
            }
        }

        public int GetPositiveCount()
        {
            int positive = 0;
            foreach (int item in Items)
            {
                if (item > 0)
                {
                    positive++;
                }
            }
            return positive;
        }

        public int GetNegativeCount()
        {
            int negative = 0;
            foreach (int item in Items)
            {
                if (item < 0)
                {
                    negative++;
                }
            }
            return negative;
        }

        public bool HasItems()
        {
            return Items.Count > 0;
        }

        public void Clear()
        {
            Items.Clear();
            Count = 0;
        }

        public void AddRange(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                Items.Add(i);
            }
        }
    }
}";

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        stopwatch.Stop();

        Assert.True(translationResult.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Translation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_UnsupportedExtension_ReturnsFailure()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            "code", ".xyz", "pt-br");

        Assert.False(translationResult.IsSuccess);
    }

    [Fact]
    public async Task EmptySourceCode_TranslatesWithoutError()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        OperationResult<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            "", ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
    }
}
