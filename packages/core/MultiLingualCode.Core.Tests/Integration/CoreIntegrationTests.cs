using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.LanguageAdapters.Python;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using MultiLingualCode.Core.Tests.LanguageAdapters;

namespace MultiLingualCode.Core.Tests.Integration;

public class CoreIntegrationTests : IDisposable
{
    public string TranslationsPath;
    public string TempDir;

    public CoreIntegrationTests()
    {
        TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        TempDir = Path.Combine(Path.GetTempPath(), $"core_integ_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    public TranslationOrchestrator CreateOrchestrator(IdentifierMapper mapper)
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = new NaturalLanguageProvider { LanguageCode = "pt-br", TranslationsBasePath = TranslationsPath };
        return new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };
    }

    [Fact]
    public async Task HelloWorld_TranslatesToPtBr()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
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

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
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
        // because IdentifierMapper stores bidirectional mappings via tradu annotations.
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"class Program // tradu[pt-br]:Programa
{
    static void Main() // tradu[pt-br]:Principal
    {
    }
}";

        OperationResultGeneric<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);

        // Forward: identifiers translated
        Assert.Contains("Programa", forwardResult.Value);
        Assert.Contains("Principal", forwardResult.Value);
        Assert.Contains("classe", forwardResult.Value);

        // Reverse: identifiers restored
        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        Assert.True(reverseResult.IsSuccess);

        Assert.Contains("Program", reverseResult.Value);
        Assert.Contains("Main", reverseResult.Value);
    }

    [Fact]
    public async Task Calculator_WithTraduAnnotations_TranslatesIdentifiers()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
    public int operationCount; // tradu[pt-br]:contagemOperacoes

    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }

    public int Subtract(int a, int b) // tradu[pt-br]:Subtrair,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a - b;
    }
}";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
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
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Reporter // tradu[pt-br]:Relator
{
    public string GetLabel() // tradu[pt-br]:ObterRotulo
    {
        string label = ""Total operations: ""; // tradu[pt-br]:""Total de operacoes: ""
        return label;
    }
}";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
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
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"using System;

namespace MyApp
{
    public class MathUtils // tradu[pt-br]:UtilMatematica
    {
        public static int Square(int n) // tradu[pt-br]:Quadrado
        {
            return n * n;
        }

        public static int Abs(int n) // tradu[pt-br]:Absoluto
        {
            if (n < 0)
            {
                return -n;
            }
            return n;
        }
    }
}";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
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
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public class Student // tradu[pt-br]:Aluno
{
    public string Name; // tradu[pt-br]:Nome
    public int Grade; // tradu[pt-br]:Nota

    public bool IsPassing() // tradu[pt-br]:EstaAprovado
    {
        return Grade >= 60;
    }
}";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
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
        mapper.LoadMap(TempDir);
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

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
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
        mapper.LoadMap(TempDir);
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

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        stopwatch.Stop();

        Assert.True(translationResult.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Translation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task TranslateToNaturalLanguage_UnsupportedExtension_ReturnsFailure()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            "code", ".xyz", "pt-br");

        Assert.False(translationResult.IsSuccess);
    }

    [Fact]
    public async Task EmptySourceCode_TranslatesWithoutError()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            "", ".cs", "pt-br");

        Assert.True(translationResult.IsSuccess);
    }

    [Fact]
    public async Task RoundTrip_ForwardTranslation_ContainsTranslatedKeywords()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"class Program
{
    static void Main()
    {
        int x = 10;
        if (x > 5)
        {
            return;
        }
    }
}";

        OperationResultGeneric<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);

        Assert.Contains("classe", forwardResult.Value);
        Assert.Contains("estatico", forwardResult.Value);
        Assert.Contains("vazio", forwardResult.Value);
        Assert.Contains("inteiro", forwardResult.Value);
        Assert.Contains("se", forwardResult.Value);
        Assert.Contains("retornar", forwardResult.Value);

        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        Assert.True(reverseResult.IsSuccess);
    }

    [Fact]
    public async Task FileScopedNamespace_TranslatesCorrectly()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"namespace MyApp;

class Program
{
    static void Main()
    {
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("espaconome", result.Value);
        Assert.Contains("classe", result.Value);
    }

    [Fact]
    public async Task EnumDeclaration_TranslatesKeywords()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public enum Color
{
    Red,
    Green,
    Blue
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("publico", result.Value);
        Assert.Contains("enumeracao", result.Value);
    }

    [Fact]
    public async Task StructDeclaration_TranslatesKeywords()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public struct Point
{
    public int X;
    public int Y;
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("estrutura", result.Value);
    }

    [Fact]
    public async Task InterfaceDeclaration_TranslatesKeywords()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"public interface IShape
{
    int GetArea();
    bool IsValid();
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("interface", result.Value);
        Assert.Contains("inteiro", result.Value);
        Assert.Contains("booleano", result.Value);
    }

    [Fact]
    public async Task TryCatch_TranslatesKeywords()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"class Program
{
    void Run()
    {
        try
        {
            int x = 1;
        }
        catch
        {
        }
        finally
        {
        }
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("tentar", result.Value);
        Assert.Contains("capturar", result.Value);
        Assert.Contains("finalmente", result.Value);
    }

    [Fact]
    public async Task SwitchStatement_TranslatesKeywords()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = @"class Program
{
    string GetName(int x)
    {
        switch (x)
        {
            case 1:
                return ""one"";
            default:
                return ""other"";
        }
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("escolha", result.Value);
        Assert.Contains("caso", result.Value);
        Assert.Contains("padrao", result.Value);
    }

    [Fact]
    public async Task ReverseTranslation_KeywordsRevertToCSharp()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string translatedCode = @"usando System;

espaconome HelloWorld
{
    publico classe Program
    {
        publico estatico vazio Main()
        {
        }
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            translatedCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("using", result.Value);
        Assert.Contains("namespace", result.Value);
        Assert.Contains("public", result.Value);
        Assert.Contains("class", result.Value);
        Assert.Contains("static", result.Value);
        Assert.Contains("void", result.Value);
        Assert.DoesNotContain("usando", result.Value);
        Assert.DoesNotContain("espaconome", result.Value);
        Assert.DoesNotContain("publico", result.Value);
        Assert.DoesNotContain("classe", result.Value);
    }

    [Fact]
    public async Task RoundTrip_FullProgram_ProducesOriginalCode()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string originalCode = @"using System;

namespace HelloWorld
{
    public class Program
    {
        public static void Main()
        {
            if (true)
            {
                int x = 42;
                return;
            }
        }
    }
}";

        OperationResultGeneric<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(
            originalCode, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);

        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        Assert.True(reverseResult.IsSuccess);

        Assert.Equal(originalCode, reverseResult.Value);
    }

    [Fact]
    public async Task ReverseTranslation_KeywordsInStrings_NotReplaced()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string translatedCode = @"classe Program
{
    texto GetMessage()
    {
        retornar ""classe e uma palavra"";
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateFromNaturalLanguageAsync(
            translatedCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.Contains("class", result.Value);
        Assert.Contains("string", result.Value);
        Assert.Contains("return", result.Value);
        Assert.Contains("classe e uma palavra", result.Value);
    }

    [Fact]
    public async Task RoundTrip_WithIdentifiers_ReversesAll()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string originalCode = @"public class Calculator // tradu[pt-br]:Calculadora
{
    public int Add(int a, int b) // tradu[pt-br]:Somar
    {
        return a + b;
    }
}";

        OperationResultGeneric<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(
            originalCode, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);
        Assert.Contains("Calculadora", forwardResult.Value);
        Assert.Contains("Somar", forwardResult.Value);

        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        Assert.True(reverseResult.IsSuccess);
        Assert.Contains("Calculator", reverseResult.Value);
        Assert.Contains("Add", reverseResult.Value);
        Assert.Contains("public", reverseResult.Value);
        Assert.Contains("class", reverseResult.Value);
    }

    // ========================================================================
    // Python integration tests
    // ========================================================================

    public TranslationOrchestrator CreatePythonOrchestrator(IdentifierMapper mapper)
    {
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(new CSharpAdapter());
        registry.RegisterAdapter(new PythonAdapter());
        NaturalLanguageProvider provider = new NaturalLanguageProvider { LanguageCode = "pt-br", TranslationsBasePath = TranslationsPath };
        return new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };
    }

    [RequiresPythonFact]
    public async Task Python_SimpleFunction_TranslatesToPtBr()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreatePythonOrchestrator(mapper);

        string sourceCode = "def foo():\n    if True:\n        return False";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".py", "pt-br");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains("definir", result.Value);
        Assert.Contains("se", result.Value);
        Assert.Contains("verdadeiro", result.Value);
        Assert.Contains("retornar", result.Value);
        Assert.Contains("falso", result.Value);
    }

    [RequiresPythonFact]
    public async Task Python_ForLoop_TranslatesToPtBr()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreatePythonOrchestrator(mapper);

        string sourceCode = "for i in range(10):\n    pass";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".py", "pt-br");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains("para", result.Value);
        Assert.Contains("em", result.Value);
        Assert.Contains("passar", result.Value);
    }

    [RequiresPythonFact]
    public async Task Python_RoundTrip_KeywordsPreserved()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreatePythonOrchestrator(mapper);

        string sourceCode = "def foo():\n    while True:\n        break";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".py", "pt-br");
        Assert.True(translationResult.IsSuccess, translationResult.ErrorMessage);

        string translated = translationResult.Value;
        Assert.Contains("definir", translated);
        Assert.Contains("enquanto", translated);
        Assert.Contains("parar", translated);

        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            translated, ".py", "pt-br");
        Assert.True(reverseResult.IsSuccess, reverseResult.ErrorMessage);

        Assert.Equal(sourceCode, reverseResult.Value);
    }

    [RequiresPythonFact]
    public async Task Python_RoundTrip_SingleQuoteString_PreservesQuotes()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreatePythonOrchestrator(mapper);

        string sourceCode = "x = 'hello world'";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".py", "pt-br");
        Assert.True(translationResult.IsSuccess, translationResult.ErrorMessage);

        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            translationResult.Value, ".py", "pt-br");
        Assert.True(reverseResult.IsSuccess, reverseResult.ErrorMessage);

        Assert.Equal(sourceCode, reverseResult.Value);
    }

    [Fact]
    public async Task CSharp_RoundTrip_StringLiteral_PreservesQuotes()
    {
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        TranslationOrchestrator orchestrator = CreateOrchestrator(mapper);

        string sourceCode = "string x = \"hello\";";

        OperationResultGeneric<string> translationResult = await orchestrator.TranslateToNaturalLanguageAsync(
            sourceCode, ".cs", "pt-br");
        Assert.True(translationResult.IsSuccess, translationResult.ErrorMessage);

        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            translationResult.Value, ".cs", "pt-br");
        Assert.True(reverseResult.IsSuccess, reverseResult.ErrorMessage);

        Assert.Equal(sourceCode, reverseResult.Value);
    }
}
