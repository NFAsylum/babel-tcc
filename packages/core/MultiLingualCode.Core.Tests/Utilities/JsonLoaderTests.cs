using System.Text.Json;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Tests.Utilities;

public class JsonLoaderTests : IDisposable
{
    public JsonLoader loader = new();
    public string tempDir;

    public JsonLoaderTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), $"babel-tcc-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    public string CreateTempJson(string fileName, string content)
    {
        string path = Path.Combine(tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Load_DeserializesValidJson()
    {
        string path = CreateTempJson("test.json", """{"name":"test","value":42}""");

        OperationResultGeneric<TestData> result = loader.Load<TestData>(path);

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value.Name);
        Assert.Equal(42, result.Value.Value);
    }

    [Fact]
    public void Load_CachesResult()
    {
        string path = CreateTempJson("cached.json", """{"name":"cached","value":1}""");

        OperationResultGeneric<TestData> first = loader.Load<TestData>(path);
        OperationResultGeneric<TestData> second = loader.Load<TestData>(path);

        Assert.Same(first.Value, second.Value);
    }

    [Fact]
    public void Load_ReturnsFailureOnFileNotFound()
    {
        OperationResultGeneric<TestData> result = loader.Load<TestData>("/nonexistent/path.json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Load_ReturnsFailureOnInvalidJson()
    {
        string path = CreateTempJson("invalid.json", "not valid json{{{");

        OperationResultGeneric<TestData> result = loader.Load<TestData>(path);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadAsync_DeserializesValidJson()
    {
        string path = CreateTempJson("async.json", """{"name":"async","value":99}""");

        OperationResultGeneric<TestData> result = await loader.LoadAsync<TestData>(path);

        Assert.True(result.IsSuccess);
        Assert.Equal("async", result.Value.Name);
        Assert.Equal(99, result.Value.Value);
    }

    [Fact]
    public async Task LoadAsync_CachesResult()
    {
        string path = CreateTempJson("async-cached.json", """{"name":"c","value":0}""");

        OperationResultGeneric<TestData> first = await loader.LoadAsync<TestData>(path);
        OperationResultGeneric<TestData> second = await loader.LoadAsync<TestData>(path);

        Assert.Same(first.Value, second.Value);
    }

    [Fact]
    public void Invalidate_RemovesFromCache()
    {
        string path = CreateTempJson("invalidate.json", """{"name":"v1","value":1}""");

        OperationResultGeneric<TestData> first = loader.Load<TestData>(path);
        Assert.Equal("v1", first.Value.Name);

        File.WriteAllText(path, """{"name":"v2","value":2}""");
        loader.Invalidate(path);

        OperationResultGeneric<TestData> second = loader.Load<TestData>(path);
        Assert.Equal("v2", second.Value.Name);
        Assert.NotSame(first.Value, second.Value);
    }

    [Fact]
    public void ClearCache_RemovesAllEntries()
    {
        string path1 = CreateTempJson("a.json", """{"name":"a","value":1}""");
        string path2 = CreateTempJson("b.json", """{"name":"b","value":2}""");

        OperationResultGeneric<TestData> a1 = loader.Load<TestData>(path1);
        OperationResultGeneric<TestData> b1 = loader.Load<TestData>(path2);

        loader.ClearCache();

        OperationResultGeneric<TestData> a2 = loader.Load<TestData>(path1);
        OperationResultGeneric<TestData> b2 = loader.Load<TestData>(path2);

        Assert.NotSame(a1.Value, a2.Value);
        Assert.NotSame(b1.Value, b2.Value);
    }

    public class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
