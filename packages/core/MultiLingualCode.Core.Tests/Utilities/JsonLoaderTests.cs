using System.Text.Json;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Tests.Utilities;

public class JsonLoaderTests : IDisposable
{
    public JsonLoader _loader = new();
    public string _tempDir;

    public JsonLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"babel-tcc-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    public string CreateTempJson(string fileName, string content)
    {
        string path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Load_DeserializesValidJson()
    {
        string path = CreateTempJson("test.json", """{"name":"test","value":42}""");

        OperationResult<TestData> result = _loader.Load<TestData>(path);

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value.Name);
        Assert.Equal(42, result.Value.Value);
    }

    [Fact]
    public void Load_CachesResult()
    {
        string path = CreateTempJson("cached.json", """{"name":"cached","value":1}""");

        OperationResult<TestData> first = _loader.Load<TestData>(path);
        OperationResult<TestData> second = _loader.Load<TestData>(path);

        Assert.Same(first.Value, second.Value);
    }

    [Fact]
    public void Load_ReturnsFailureOnFileNotFound()
    {
        OperationResult<TestData> result = _loader.Load<TestData>("/nonexistent/path.json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Load_ReturnsFailureOnInvalidJson()
    {
        string path = CreateTempJson("invalid.json", "not valid json{{{");

        OperationResult<TestData> result = _loader.Load<TestData>(path);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadAsync_DeserializesValidJson()
    {
        string path = CreateTempJson("async.json", """{"name":"async","value":99}""");

        OperationResult<TestData> result = await _loader.LoadAsync<TestData>(path);

        Assert.True(result.IsSuccess);
        Assert.Equal("async", result.Value.Name);
        Assert.Equal(99, result.Value.Value);
    }

    [Fact]
    public async Task LoadAsync_CachesResult()
    {
        string path = CreateTempJson("async-cached.json", """{"name":"c","value":0}""");

        OperationResult<TestData> first = await _loader.LoadAsync<TestData>(path);
        OperationResult<TestData> second = await _loader.LoadAsync<TestData>(path);

        Assert.Same(first.Value, second.Value);
    }

    [Fact]
    public void Invalidate_RemovesFromCache()
    {
        string path = CreateTempJson("invalidate.json", """{"name":"v1","value":1}""");

        OperationResult<TestData> first = _loader.Load<TestData>(path);
        Assert.Equal("v1", first.Value.Name);

        File.WriteAllText(path, """{"name":"v2","value":2}""");
        _loader.Invalidate(path);

        OperationResult<TestData> second = _loader.Load<TestData>(path);
        Assert.Equal("v2", second.Value.Name);
        Assert.NotSame(first.Value, second.Value);
    }

    [Fact]
    public void ClearCache_RemovesAllEntries()
    {
        string path1 = CreateTempJson("a.json", """{"name":"a","value":1}""");
        string path2 = CreateTempJson("b.json", """{"name":"b","value":2}""");

        OperationResult<TestData> a1 = _loader.Load<TestData>(path1);
        OperationResult<TestData> b1 = _loader.Load<TestData>(path2);

        _loader.ClearCache();

        OperationResult<TestData> a2 = _loader.Load<TestData>(path1);
        OperationResult<TestData> b2 = _loader.Load<TestData>(path2);

        Assert.NotSame(a1.Value, a2.Value);
        Assert.NotSame(b1.Value, b2.Value);
    }

    public class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
