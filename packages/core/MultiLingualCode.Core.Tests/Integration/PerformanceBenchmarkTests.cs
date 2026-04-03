using System.Diagnostics;
using System.Text;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Integration;

[Trait("Category", "Performance")]
public class PerformanceBenchmarkTests : IDisposable
{
    public string TranslationsPath;
    public string TempDir;
    public TranslationOrchestrator Orchestrator;

    public PerformanceBenchmarkTests()
    {
        TranslationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        TempDir = Path.Combine(Path.GetTempPath(), $"perf_bench_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
        Orchestrator = CreateOrchestrator();

        // Warmup: first translation includes JIT compilation and table loading
        string warmupCode = GenerateCSharpCode(1);
        Orchestrator.TranslateToNaturalLanguageAsync(warmupCode, ".cs", "pt-br").GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    public TranslationOrchestrator CreateOrchestrator()
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = new NaturalLanguageProvider { LanguageCode = "pt-br", TranslationsBasePath = TranslationsPath };
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(TempDir);
        return new TranslationOrchestrator { Registry = registry, Provider = provider, IdentifierMapperService = mapper };
    }

    public static string GenerateCSharpCode(int methodCount)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("namespace Performance.Test");
        sb.AppendLine("{");
        sb.AppendLine("    public class GeneratedClass");
        sb.AppendLine("    {");

        for (int i = 0; i < methodCount; i++)
        {
            sb.AppendLine($"        public int Method{i}(int param{i})");
            sb.AppendLine("        {");
            sb.AppendLine($"            int result = param{i} * 2;");
            sb.AppendLine($"            if (result > 100)");
            sb.AppendLine("            {");
            sb.AppendLine("                return result;");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine($"                for (int j = 0; j < param{i}; j++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    result += j;");
            sb.AppendLine("                }");
            sb.AppendLine("                return result;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    [Fact]
    public async Task SmallFile_Under100Lines_CompletesUnder2Seconds()
    {
        string code = GenerateCSharpCode(5);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await Orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Small file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }

    [Fact]
    public async Task MediumFile_100To500Lines_CompletesUnder3Seconds()
    {
        string code = GenerateCSharpCode(25);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await Orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000,
            $"Medium file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
    }

    [Fact]
    public async Task LargeFile_500To2000Lines_CompletesUnder5Seconds()
    {
        string code = GenerateCSharpCode(100);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await Orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Large file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public async Task VeryLargeFile_2000PlusLines_CompletesUnder10Seconds()
    {
        string code = GenerateCSharpCode(200);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await Orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 10000,
            $"Very large file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");
    }

    [Fact]
    public async Task MultipleTranslations_FiftyIterations_NoMemoryLeak()
    {
        string code = GenerateCSharpCode(10);

        long memoryBefore = GC.GetTotalMemory(true);

        for (int i = 0; i < 50; i++)
        {
            OperationResultGeneric<string> result = await Orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
            Assert.True(result.IsSuccess);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memoryAfter = GC.GetTotalMemory(true);
        long memoryDelta = memoryAfter - memoryBefore;

        Assert.True(memoryDelta < 50 * 1024 * 1024,
            $"Memory grew by {memoryDelta / 1024 / 1024}MB after 50 translations, possible leak");
    }

    [Fact]
    public async Task ReverseTranslation_MediumFile_SamePerformanceAsForward()
    {
        string code = GenerateCSharpCode(25);

        OperationResultGeneric<string> forwardResult = await Orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> reverseResult = await Orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(reverseResult.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000,
            $"Reverse translation took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
    }
}
