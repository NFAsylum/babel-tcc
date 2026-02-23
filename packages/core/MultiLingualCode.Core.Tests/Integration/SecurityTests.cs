using System.Text;
using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Integration;

public class SecurityTests : IDisposable
{
    public string _translationsPath;
    public string _tempDir;

    public SecurityTests()
    {
        _translationsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "translations");
        _tempDir = Path.Combine(Path.GetTempPath(), $"sec_test_{Guid.NewGuid()}");
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

    [Fact]
    public async Task MaliciousCode_DoesNotCrash()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        string maliciousCode = @"
class Evil
{
    void Attack()
    {
        System.IO.File.Delete(""C:\\Windows\\System32"");
        System.Diagnostics.Process.Start(""cmd.exe"", ""/c format C:"");
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            maliciousCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ControlCharacters_DoNotCrash()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        string codeWithControlChars = "class Test\n{\n    string x = \"hello\\0\\r\\n\\t\";\n}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            codeWithControlChars, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ExtremelyLongIdentifier_DoesNotCrash()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        string longName = new string('a', 10000);
        string code = $"class {longName} {{ }}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            code, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeeplyNestedCode_DoesNotStackOverflow()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("class Test {");
        sb.AppendLine("    void Method() {");

        for (int i = 0; i < 100; i++)
        {
            sb.AppendLine(new string(' ', (i + 2) * 4) + "if (true) {");
        }

        for (int i = 99; i >= 0; i--)
        {
            sb.AppendLine(new string(' ', (i + 2) * 4) + "}");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            sb.ToString(), ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EmptyFile_HandledGracefully()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WhitespaceOnlyFile_HandledGracefully()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            "   \n\n\t\t  \r\n  ", ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UnicodeSourceCode_HandledCorrectly()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        string unicodeCode = @"
class Programa
{
    string mensagem = ""Olá Mundo 你好世界 مرحبا"";
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            unicodeCode, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MalformedJson_LoadFrom_DoesNotCrash()
    {
        string tempPath = Path.Combine(_tempDir, "malformed.json");
        File.WriteAllText(tempPath, "{{{{invalid json]]]]");

        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(tempPath);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void DeeplyNestedJson_LoadFrom_DoesNotCrash()
    {
        string tempPath = Path.Combine(_tempDir, "deep.json");
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < 50; i++)
        {
            sb.Append("{\"a\":");
        }
        sb.Append("1");
        for (int i = 0; i < 50; i++)
        {
            sb.Append("}");
        }

        File.WriteAllText(tempPath, sb.ToString());

        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(tempPath);

        // Deeply nested JSON should not crash - result may succeed or fail
        // The important thing is no exception/crash
        Assert.NotNull(result);
    }

    [Fact]
    public void LargeJsonFile_LoadFrom_DoesNotCrash()
    {
        string tempPath = Path.Combine(_tempDir, "large.json");
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"keywords\":{");

        for (int i = 0; i < 10000; i++)
        {
            if (i > 0) sb.Append(",");
            sb.Append($"\"keyword{i}\":{i}");
        }

        sb.Append("}}");
        File.WriteAllText(tempPath, sb.ToString());

        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(tempPath);

        Assert.True(result.IsSuccess);
        Assert.Equal(10000, result.Value.Count);
    }

    [Fact]
    public void PathTraversal_InFilePath_DoesNotEscape()
    {
        string maliciousPath = Path.Combine(_tempDir, "..", "..", "..", "etc", "passwd");

        OperationResultGeneric<KeywordTable> result = KeywordTable.LoadFrom(maliciousPath);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task InjectionInTraduComment_DoesNotExecute()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        string code = @"
class Test // tradu:Teste<script>alert('xss')</script>
{
    int value; // tradu:valor'; DROP TABLE users; --
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            code, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TranslatedOutput_DoesNotIntroduceExecutableCode()
    {
        TranslationOrchestrator orchestrator = CreateOrchestrator();

        string code = @"
class Program
{
    static void Main()
    {
        int x = 42;
        return;
    }
}";

        OperationResultGeneric<string> result = await orchestrator.TranslateToNaturalLanguageAsync(
            code, ".cs", "pt-br");

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("Process.Start", result.Value);
        Assert.DoesNotContain("File.Delete", result.Value);
        Assert.DoesNotContain("exec(", result.Value);
    }
}
