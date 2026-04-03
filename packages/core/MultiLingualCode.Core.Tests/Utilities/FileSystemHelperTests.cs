using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Tests.Utilities;

public class FileSystemHelperTests : IDisposable
{
    public string TempDir;

    public FileSystemHelperTests()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"babel-tcc-fs-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(TempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    [Fact]
    public void ResolvePath_WithAbsolutePath_ReturnsSamePath()
    {
        string absolute = Path.Combine(TempDir, "file.txt");
        string result = FileSystemHelper.ResolvePath(absolute);
        Assert.Equal(Path.GetFullPath(absolute), result);
    }

    [Fact]
    public void ResolvePath_WithRelativePath_ResolvesAgainstBaseDirectory()
    {
        string result = FileSystemHelper.ResolvePath("sub/file.txt", TempDir);
        string expected = Path.GetFullPath(Path.Combine(TempDir, "sub/file.txt"));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetExtension_WithMixedCasePaths_ReturnsLowercaseExtension()
    {
        Assert.Equal(".cs", FileSystemHelper.GetExtension("File.CS"));
        Assert.Equal(".py", FileSystemHelper.GetExtension("script.Py"));
        Assert.Equal(".ts", FileSystemHelper.GetExtension("app.ts"));
    }

    [Fact]
    public void GetExtension_WithNoExtension_ReturnsEmptyString()
    {
        Assert.Equal("", FileSystemHelper.GetExtension("Makefile"));
    }

    [Fact]
    public void HasExtension_WithDifferentCases_MatchesCaseInsensitive()
    {
        Assert.True(FileSystemHelper.HasExtension("file.CS", ".cs"));
        Assert.True(FileSystemHelper.HasExtension("file.py", ".PY"));
        Assert.False(FileSystemHelper.HasExtension("file.cs", ".py"));
    }

    [Fact]
    public void HasExtension_WithMultipleExtensions_MatchesAnyOfThem()
    {
        Assert.True(FileSystemHelper.HasExtension("file.cs", ".cs", ".py", ".ts"));
        Assert.True(FileSystemHelper.HasExtension("file.py", ".cs", ".py", ".ts"));
        Assert.False(FileSystemHelper.HasExtension("file.rb", ".cs", ".py", ".ts"));
    }

    [Fact]
    public void FindProjectRoot_WithMarkerInParent_ReturnsProjectDirectory()
    {
        string projectDir = Path.Combine(TempDir, "myproject");
        string subDir = Path.Combine(projectDir, "src", "deep");
        Directory.CreateDirectory(subDir);
        Directory.CreateDirectory(Path.Combine(projectDir, ".multilingual"));

        string result = FileSystemHelper.FindProjectRoot(subDir);

        Assert.Equal(projectDir, result);
    }

    [Fact]
    public void FindProjectRoot_WithNoMarker_ReturnsEmptyString()
    {
        string result = FileSystemHelper.FindProjectRoot(TempDir, ".nonexistent-marker");
        Assert.Equal("", result);
    }

    [Fact]
    public void FindProjectRoot_WithFilePath_ResolvesToProjectDirectory()
    {
        string projectDir = Path.Combine(TempDir, "proj2");
        string srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(Path.Combine(projectDir, ".multilingual"));
        string filePath = Path.Combine(srcDir, "Program.cs");
        File.WriteAllText(filePath, "");

        string result = FileSystemHelper.FindProjectRoot(filePath);

        Assert.Equal(projectDir, result);
    }
}
