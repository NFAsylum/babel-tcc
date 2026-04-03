using MultiLingualCode.Core.LanguageAdapters.Python;

namespace MultiLingualCode.Core.Tests.Integration;

public class BuildDeployTests
{
    [Fact]
    public void TokenizerScript_ExistsInOutputDirectory()
    {
        string scriptPath = PythonTokenizerService.GetScriptPath();
        Assert.True(File.Exists(scriptPath),
            $"tokenizer_service.py not found at {scriptPath}. Check CopyToOutputDirectory in .csproj.");
    }

    [Fact]
    public void TokenizerScript_IsNotEmpty()
    {
        string scriptPath = PythonTokenizerService.GetScriptPath();
        FileInfo info = new FileInfo(scriptPath);
        Assert.True(info.Length > 0, "tokenizer_service.py exists but is empty.");
    }

    [Fact]
    public void TokenizerScript_ContainsExpectedEntryPoint()
    {
        string scriptPath = PythonTokenizerService.GetScriptPath();
        string content = File.ReadAllText(scriptPath);
        Assert.Contains("tokenize_source", content);
        Assert.Contains("sys.stdin", content);
    }

    [Fact]
    public void AllScriptFiles_ExistInOutputDirectory()
    {
        string outputDir = AppContext.BaseDirectory;
        string[] expectedScripts = new[]
        {
            Path.Combine("LanguageAdapters", "Python", "tokenizer_service.py"),
        };

        foreach (string script in expectedScripts)
        {
            string fullPath = Path.Combine(outputDir, script);
            Assert.True(File.Exists(fullPath),
                $"Script not found in output: {script}. Add CopyToOutputDirectory in .csproj.");
        }
    }

    /// <summary>
    /// Dynamically verifies that every non-.cs file in the LanguageAdapters source
    /// directory has been copied to the output directory. Catches cases where someone
    /// adds a new script but forgets to configure CopyToOutputDirectory in .csproj.
    /// </summary>
    [Fact]
    public void AllNonCSharpSourceFiles_CopiedToOutput()
    {
        string outputDir = AppContext.BaseDirectory;
        string sourceAdaptersDir = Path.GetFullPath(
            Path.Combine(outputDir, "..", "..", "..", "..",
                "MultiLingualCode.Core", "LanguageAdapters"));

        if (!Directory.Exists(sourceAdaptersDir))
        {
            // Running outside of repo context (e.g. CI with published output).
            // Fall back to verifying known scripts exist.
            Assert.True(File.Exists(PythonTokenizerService.GetScriptPath()),
                "tokenizer_service.py not found in output.");
            return;
        }

        string[] sourceScripts = Directory.GetFiles(sourceAdaptersDir, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".cs") && !Path.GetFileName(f).StartsWith("."))
            .ToArray();

        foreach (string sourceScript in sourceScripts)
        {
            string relativePath = Path.GetRelativePath(sourceAdaptersDir, sourceScript);
            string outputPath = Path.Combine(outputDir, "LanguageAdapters", relativePath);
            Assert.True(File.Exists(outputPath),
                $"Source script {relativePath} exists in LanguageAdapters/ but was not copied to output. "
                + "Add <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory> in .csproj.");
        }
    }
}
