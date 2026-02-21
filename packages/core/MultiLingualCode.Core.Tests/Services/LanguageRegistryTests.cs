using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class LanguageRegistryTests
{
    public LanguageRegistry _registry = new();

    private static ILanguageAdapter CreateMockAdapter(string name, params string[] extensions)
    {
        ILanguageAdapter adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns(name);
        adapter.FileExtensions.Returns(extensions);
        return adapter;
    }

    [Fact]
    public void RegisterAdapter_MakesExtensionsAvailable()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");

        OperationResult result = _registry.RegisterAdapter(adapter);

        Assert.True(result.IsSuccess);
        Assert.True(_registry.IsSupported(".cs"));
    }

    [Fact]
    public void RegisterAdapter_MultipleExtensions()
    {
        ILanguageAdapter adapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        OperationResult result = _registry.RegisterAdapter(adapter);

        Assert.True(result.IsSuccess);
        Assert.True(_registry.IsSupported(".ts"));
        Assert.True(_registry.IsSupported(".tsx"));
    }

    [Fact]
    public void RegisterAdapter_NullAdapter_ReturnsFailure()
    {
        OperationResult result = _registry.RegisterAdapter(null!);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_EmptyExtensions_ReturnsFailure()
    {
        ILanguageAdapter adapter = CreateMockAdapter("Empty");

        OperationResult result = _registry.RegisterAdapter(adapter);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_NullExtensions_ReturnsFailure()
    {
        ILanguageAdapter adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("Null");
        adapter.FileExtensions.Returns((string[]?)null);

        OperationResult result = _registry.RegisterAdapter(adapter);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_ReplacesExistingAdapter()
    {
        ILanguageAdapter first = CreateMockAdapter("First", ".cs");
        ILanguageAdapter second = CreateMockAdapter("Second", ".cs");

        _registry.RegisterAdapter(first);
        _registry.RegisterAdapter(second);

        OperationResult<ILanguageAdapter> result = _registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Same(second, result.Value);
    }

    [Fact]
    public void GetAdapter_ReturnsCorrectAdapter()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");

        _registry.RegisterAdapter(csAdapter);
        _registry.RegisterAdapter(pyAdapter);

        OperationResult<ILanguageAdapter> csResult = _registry.GetAdapter(".cs");
        OperationResult<ILanguageAdapter> pyResult = _registry.GetAdapter(".py");

        Assert.True(csResult.IsSuccess);
        Assert.True(pyResult.IsSuccess);
        Assert.Same(csAdapter, csResult.Value);
        Assert.Same(pyAdapter, pyResult.Value);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForUnknownExtension()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");
        _registry.RegisterAdapter(adapter);

        OperationResult<ILanguageAdapter> result = _registry.GetAdapter(".py");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForEmptyString()
    {
        OperationResult<ILanguageAdapter> result = _registry.GetAdapter("");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForNullString()
    {
        OperationResult<ILanguageAdapter> result = _registry.GetAdapter(null!);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_CaseInsensitive()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");
        _registry.RegisterAdapter(adapter);

        OperationResult<ILanguageAdapter> upperResult = _registry.GetAdapter(".CS");
        OperationResult<ILanguageAdapter> mixedResult = _registry.GetAdapter(".Cs");

        Assert.True(upperResult.IsSuccess);
        Assert.True(mixedResult.IsSuccess);
        Assert.Same(adapter, upperResult.Value);
        Assert.Same(adapter, mixedResult.Value);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsAllRegistered()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");

        _registry.RegisterAdapter(csAdapter);
        _registry.RegisterAdapter(pyAdapter);

        string[] extensions = _registry.GetSupportedExtensions();
        Assert.Equal(2, extensions.Length);
        Assert.Contains(".cs", extensions);
        Assert.Contains(".py", extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsEmptyWhenNoneRegistered()
    {
        string[] extensions = _registry.GetSupportedExtensions();
        Assert.Empty(extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsSorted()
    {
        _registry.RegisterAdapter(CreateMockAdapter("Python", ".py"));
        _registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));
        _registry.RegisterAdapter(CreateMockAdapter("TypeScript", ".ts", ".tsx"));

        string[] extensions = _registry.GetSupportedExtensions();
        Assert.Equal(new[] { ".cs", ".py", ".ts", ".tsx" }, extensions);
    }

    [Fact]
    public void IsSupported_ReturnsTrueForRegistered()
    {
        _registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.True(_registry.IsSupported(".cs"));
    }

    [Fact]
    public void IsSupported_ReturnsFalseForUnregistered()
    {
        Assert.False(_registry.IsSupported(".cs"));
    }

    [Fact]
    public void IsSupported_ReturnsFalseForEmptyString()
    {
        Assert.False(_registry.IsSupported(""));
    }

    [Fact]
    public void IsSupported_CaseInsensitive()
    {
        _registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.True(_registry.IsSupported(".CS"));
        Assert.True(_registry.IsSupported(".Cs"));
    }

    [Fact]
    public void RegisterAdapter_NormalizesExtensionWithoutDot()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", "cs");

        _registry.RegisterAdapter(adapter);

        Assert.True(_registry.IsSupported(".cs"));
        OperationResult<ILanguageAdapter> result = _registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Same(adapter, result.Value);
    }

    [Fact]
    public void GetAdapter_NormalizesExtensionWithoutDot()
    {
        _registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        OperationResult<ILanguageAdapter> result = _registry.GetAdapter("cs");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MultipleAdapters_IndependentRegistrations()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");
        ILanguageAdapter tsAdapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        _registry.RegisterAdapter(csAdapter);
        _registry.RegisterAdapter(pyAdapter);
        _registry.RegisterAdapter(tsAdapter);

        OperationResult<ILanguageAdapter> csResult = _registry.GetAdapter(".cs");
        OperationResult<ILanguageAdapter> pyResult = _registry.GetAdapter(".py");
        OperationResult<ILanguageAdapter> tsResult = _registry.GetAdapter(".ts");
        OperationResult<ILanguageAdapter> tsxResult = _registry.GetAdapter(".tsx");

        Assert.True(csResult.IsSuccess);
        Assert.True(pyResult.IsSuccess);
        Assert.True(tsResult.IsSuccess);
        Assert.True(tsxResult.IsSuccess);
        Assert.Same(csAdapter, csResult.Value);
        Assert.Same(pyAdapter, pyResult.Value);
        Assert.Same(tsAdapter, tsResult.Value);
        Assert.Same(tsAdapter, tsxResult.Value);
        Assert.Equal(4, _registry.GetSupportedExtensions().Length);
    }
}
