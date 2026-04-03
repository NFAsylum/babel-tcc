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
}
