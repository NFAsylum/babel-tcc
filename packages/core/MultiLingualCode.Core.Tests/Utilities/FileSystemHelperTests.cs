using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Tests.Utilities;

public class FileSystemHelperTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemHelperTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"babel-tcc-fs-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void ResolvePath_AbsolutePathReturnedAsIs()
    {
        var absolute = Path.Combine(_tempDir, "file.txt");
        var result = FileSystemHelper.ResolvePath(absolute);
        Assert.Equal(Path.GetFullPath(absolute), result);
    }

    [Fact]
    public void ResolvePath_RelativePathResolvedAgainstBase()
    {
        var result = FileSystemHelper.ResolvePath("sub/file.txt", _tempDir);
        var expected = Path.GetFullPath(Path.Combine(_tempDir, "sub/file.txt"));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetExtension_ReturnsLowercase()
    {
        Assert.Equal(".cs", FileSystemHelper.GetExtension("File.CS"));
        Assert.Equal(".py", FileSystemHelper.GetExtension("script.Py"));
        Assert.Equal(".ts", FileSystemHelper.GetExtension("app.ts"));
    }

    [Fact]
    public void GetExtension_ReturnsEmptyForNoExtension()
    {
        Assert.Equal("", FileSystemHelper.GetExtension("Makefile"));
    }

    [Fact]
    public void HasExtension_MatchesCaseInsensitive()
    {
        Assert.True(FileSystemHelper.HasExtension("file.CS", ".cs"));
        Assert.True(FileSystemHelper.HasExtension("file.py", ".PY"));
        Assert.False(FileSystemHelper.HasExtension("file.cs", ".py"));
    }

    [Fact]
    public void HasExtension_MatchesMultipleExtensions()
    {
        Assert.True(FileSystemHelper.HasExtension("file.cs", ".cs", ".py", ".ts"));
        Assert.True(FileSystemHelper.HasExtension("file.py", ".cs", ".py", ".ts"));
        Assert.False(FileSystemHelper.HasExtension("file.rb", ".cs", ".py", ".ts"));
    }

    [Fact]
    public void FindProjectRoot_FindsMarkerDirectory()
    {
        var projectDir = Path.Combine(_tempDir, "myproject");
        var subDir = Path.Combine(projectDir, "src", "deep");
        Directory.CreateDirectory(subDir);
        Directory.CreateDirectory(Path.Combine(projectDir, ".multilingual"));

        var result = FileSystemHelper.FindProjectRoot(subDir);

        Assert.Equal(projectDir, result);
    }

    [Fact]
    public void FindProjectRoot_ReturnsNullIfNotFound()
    {
        var result = FileSystemHelper.FindProjectRoot(_tempDir, ".nonexistent-marker");
        Assert.Null(result);
    }

    [Fact]
    public void FindProjectRoot_WorksFromFilePath()
    {
        var projectDir = Path.Combine(_tempDir, "proj2");
        var srcDir = Path.Combine(projectDir, "src");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(Path.Combine(projectDir, ".multilingual"));
        var filePath = Path.Combine(srcDir, "Program.cs");
        File.WriteAllText(filePath, "");

        var result = FileSystemHelper.FindProjectRoot(filePath);

        Assert.Equal(projectDir, result);
    }
}
