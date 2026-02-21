using System.Text.Json;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Tests.Utilities;

public class JsonLoaderTests : IDisposable
{
    private readonly JsonLoader _loader = new();
    private readonly string _tempDir;

    public JsonLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"babel-tcc-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTempJson(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Load_DeserializesValidJson()
    {
        var path = CreateTempJson("test.json", """{"name":"test","value":42}""");

        var result = _loader.Load<TestData>(path);

        Assert.Equal("test", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Load_CachesResult()
    {
        var path = CreateTempJson("cached.json", """{"name":"cached","value":1}""");

        var first = _loader.Load<TestData>(path);
        var second = _loader.Load<TestData>(path);

        Assert.Same(first, second);
    }

    [Fact]
    public void Load_ThrowsOnFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            _loader.Load<TestData>("/nonexistent/path.json"));
    }

    [Fact]
    public void Load_ThrowsOnInvalidJson()
    {
        var path = CreateTempJson("invalid.json", "not valid json{{{");

        Assert.Throws<JsonException>(() => _loader.Load<TestData>(path));
    }

    [Fact]
    public async Task LoadAsync_DeserializesValidJson()
    {
        var path = CreateTempJson("async.json", """{"name":"async","value":99}""");

        var result = await _loader.LoadAsync<TestData>(path);

        Assert.Equal("async", result.Name);
        Assert.Equal(99, result.Value);
    }

    [Fact]
    public async Task LoadAsync_CachesResult()
    {
        var path = CreateTempJson("async-cached.json", """{"name":"c","value":0}""");

        var first = await _loader.LoadAsync<TestData>(path);
        var second = await _loader.LoadAsync<TestData>(path);

        Assert.Same(first, second);
    }

    [Fact]
    public void Invalidate_RemovesFromCache()
    {
        var path = CreateTempJson("invalidate.json", """{"name":"v1","value":1}""");

        var first = _loader.Load<TestData>(path);
        Assert.Equal("v1", first.Name);

        File.WriteAllText(path, """{"name":"v2","value":2}""");
        _loader.Invalidate(path);

        var second = _loader.Load<TestData>(path);
        Assert.Equal("v2", second.Name);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void ClearCache_RemovesAllEntries()
    {
        var path1 = CreateTempJson("a.json", """{"name":"a","value":1}""");
        var path2 = CreateTempJson("b.json", """{"name":"b","value":2}""");

        var a1 = _loader.Load<TestData>(path1);
        var b1 = _loader.Load<TestData>(path2);

        _loader.ClearCache();

        var a2 = _loader.Load<TestData>(path1);
        var b2 = _loader.Load<TestData>(path2);

        Assert.NotSame(a1, a2);
        Assert.NotSame(b1, b2);
    }

    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
