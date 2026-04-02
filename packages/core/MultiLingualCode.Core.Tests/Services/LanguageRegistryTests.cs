using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class LanguageRegistryTests
{
    public LanguageRegistry registry = new();

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

        OperationResult result = registry.RegisterAdapter(adapter);

        Assert.True(result.IsSuccess);
        Assert.True(registry.IsSupported(".cs"));
    }

    [Fact]
    public void RegisterAdapter_MultipleExtensions()
    {
        ILanguageAdapter adapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        OperationResult result = registry.RegisterAdapter(adapter);

        Assert.True(result.IsSuccess);
        Assert.True(registry.IsSupported(".ts"));
        Assert.True(registry.IsSupported(".tsx"));
    }

    [Fact]
    public void RegisterAdapter_NullAdapter_ReturnsFailure()
    {
        OperationResult result = registry.RegisterAdapter(null!);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_EmptyExtensions_ReturnsFailure()
    {
        ILanguageAdapter adapter = CreateMockAdapter("Empty");

        OperationResult result = registry.RegisterAdapter(adapter);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_NullExtensions_ReturnsFailure()
    {
        ILanguageAdapter adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("Null");
        adapter.FileExtensions.Returns((string[]?)null);

        OperationResult result = registry.RegisterAdapter(adapter);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_ReplacesExistingAdapter()
    {
        ILanguageAdapter first = CreateMockAdapter("First", ".cs");
        ILanguageAdapter second = CreateMockAdapter("Second", ".cs");

        registry.RegisterAdapter(first);
        registry.RegisterAdapter(second);

        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Same(second, result.Value);
    }

    [Fact]
    public void GetAdapter_ReturnsCorrectAdapter()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");

        registry.RegisterAdapter(csAdapter);
        registry.RegisterAdapter(pyAdapter);

        OperationResultGeneric<ILanguageAdapter> csResult = registry.GetAdapter(".cs");
        OperationResultGeneric<ILanguageAdapter> pyResult = registry.GetAdapter(".py");

        Assert.True(csResult.IsSuccess);
        Assert.True(pyResult.IsSuccess);
        Assert.Same(csAdapter, csResult.Value);
        Assert.Same(pyAdapter, pyResult.Value);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForUnknownExtension()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");
        registry.RegisterAdapter(adapter);

        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(".py");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForEmptyString()
    {
        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter("");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForNullString()
    {
        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(null!);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_CaseInsensitive()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");
        registry.RegisterAdapter(adapter);

        OperationResultGeneric<ILanguageAdapter> upperResult = registry.GetAdapter(".CS");
        OperationResultGeneric<ILanguageAdapter> mixedResult = registry.GetAdapter(".Cs");

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

        registry.RegisterAdapter(csAdapter);
        registry.RegisterAdapter(pyAdapter);

        string[] extensions = registry.GetSupportedExtensions();
        Assert.Equal(2, extensions.Length);
        Assert.Contains(".cs", extensions);
        Assert.Contains(".py", extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsEmptyWhenNoneRegistered()
    {
        string[] extensions = registry.GetSupportedExtensions();
        Assert.Empty(extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsSorted()
    {
        registry.RegisterAdapter(CreateMockAdapter("Python", ".py"));
        registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));
        registry.RegisterAdapter(CreateMockAdapter("TypeScript", ".ts", ".tsx"));

        string[] extensions = registry.GetSupportedExtensions();
        Assert.Equal(new[] { ".cs", ".py", ".ts", ".tsx" }, extensions);
    }

    [Fact]
    public void IsSupported_ReturnsTrueForRegistered()
    {
        registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.True(registry.IsSupported(".cs"));
    }

    [Fact]
    public void IsSupported_ReturnsFalseForUnregistered()
    {
        Assert.False(registry.IsSupported(".cs"));
    }

    [Fact]
    public void IsSupported_ReturnsFalseForEmptyString()
    {
        Assert.False(registry.IsSupported(""));
    }

    [Fact]
    public void IsSupported_CaseInsensitive()
    {
        registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.True(registry.IsSupported(".CS"));
        Assert.True(registry.IsSupported(".Cs"));
    }

    [Fact]
    public void RegisterAdapter_NormalizesExtensionWithoutDot()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", "cs");

        registry.RegisterAdapter(adapter);

        Assert.True(registry.IsSupported(".cs"));
        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Same(adapter, result.Value);
    }

    [Fact]
    public void GetAdapter_NormalizesExtensionWithoutDot()
    {
        registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter("cs");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MultipleAdapters_IndependentRegistrations()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");
        ILanguageAdapter tsAdapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        registry.RegisterAdapter(csAdapter);
        registry.RegisterAdapter(pyAdapter);
        registry.RegisterAdapter(tsAdapter);

        OperationResultGeneric<ILanguageAdapter> csResult = registry.GetAdapter(".cs");
        OperationResultGeneric<ILanguageAdapter> pyResult = registry.GetAdapter(".py");
        OperationResultGeneric<ILanguageAdapter> tsResult = registry.GetAdapter(".ts");
        OperationResultGeneric<ILanguageAdapter> tsxResult = registry.GetAdapter(".tsx");

        Assert.True(csResult.IsSuccess);
        Assert.True(pyResult.IsSuccess);
        Assert.True(tsResult.IsSuccess);
        Assert.True(tsxResult.IsSuccess);
        Assert.Same(csAdapter, csResult.Value);
        Assert.Same(pyAdapter, pyResult.Value);
        Assert.Same(tsAdapter, tsResult.Value);
        Assert.Same(tsAdapter, tsxResult.Value);
        Assert.Equal(4, registry.GetSupportedExtensions().Length);
    }
}
