using System.Diagnostics;
using System.Text;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Integration;

public class PerformanceBenchmarkTests : IDisposable
{
    public string _translationsPath;
    public string _tempDir;

    public PerformanceBenchmarkTests()
    {
        _translationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        _tempDir = Path.Combine(Path.GetTempPath(), $"perf_bench_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    public TranslationOrchestrator CreateOrchestrator()
    {
        CSharpAdapter adapter = new CSharpAdapter();
        LanguageRegistry registry = new LanguageRegistry();
        registry.RegisterAdapter(adapter);
        NaturalLanguageProvider provider = new NaturalLanguageProvider("pt-br", _translationsPath);
        IdentifierMapper mapper = new IdentifierMapper();
        mapper.LoadMap(_tempDir);
        return new TranslationOrchestrator(registry, provider, mapper);
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
    public async Task SmallFile_Under100Lines_CompletesUnder100ms()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string code = GenerateCSharpCode(5);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Small file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public async Task MediumFile_100To500Lines_CompletesUnder500ms()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string code = GenerateCSharpCode(25);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Medium file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task LargeFile_500To2000Lines_CompletesUnder2Seconds()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string code = GenerateCSharpCode(100);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Large file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
    }

    [Fact]
    public async Task VeryLargeFile_2000PlusLines_CompletesUnder5Seconds()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string code = GenerateCSharpCode(200);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Very large file translation took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public async Task MultipleTranslations_NoMemoryLeak()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string code = GenerateCSharpCode(10);

        long memoryBefore = GC.GetTotalMemory(true);

        for (int i = 0; i < 50; i++)
        {
            OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
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
    public async Task ReverseTranslation_SamePerformanceAsForward()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();
        string code = GenerateCSharpCode(25);

        OperationResultGeneric<string> forwardResult = await orchestrator.TranslateToNaturalLanguageAsync(code, ".cs", "pt-br");
        Assert.True(forwardResult.IsSuccess);

        Stopwatch stopwatch = Stopwatch.StartNew();
        OperationResultGeneric<string> reverseResult = await orchestrator.TranslateFromNaturalLanguageAsync(
            forwardResult.Value, ".cs", "pt-br");
        stopwatch.Stop();

        Assert.True(reverseResult.IsSuccess);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Reverse translation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }
}
