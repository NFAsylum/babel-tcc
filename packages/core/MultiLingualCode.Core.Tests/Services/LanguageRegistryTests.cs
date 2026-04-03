using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;
using Host = MultiLingualCode.Core.Host;

namespace MultiLingualCode.Core.Tests.Services;

public class LanguageRegistryTests
{
    public LanguageRegistry Registry = new();

    public static ILanguageAdapter CreateMockAdapter(string name, params string[] extensions)
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

        OperationResult result = Registry.RegisterAdapter(adapter);

        Assert.True(result.IsSuccess);
        Assert.True(Registry.IsSupported(".cs"));
    }

    [Fact]
    public void RegisterAdapter_MultipleExtensions()
    {
        ILanguageAdapter adapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        OperationResult result = Registry.RegisterAdapter(adapter);

        Assert.True(result.IsSuccess);
        Assert.True(Registry.IsSupported(".ts"));
        Assert.True(Registry.IsSupported(".tsx"));
    }

    [Fact]
    public void RegisterAdapter_NullAdapter_ReturnsFailure()
    {
        OperationResult result = Registry.RegisterAdapter(null!);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_EmptyExtensions_ReturnsFailure()
    {
        ILanguageAdapter adapter = CreateMockAdapter("Empty");

        OperationResult result = Registry.RegisterAdapter(adapter);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_NullExtensions_ReturnsFailure()
    {
        ILanguageAdapter adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("Null");
        adapter.FileExtensions.Returns((string[]?)null);

        OperationResult result = Registry.RegisterAdapter(adapter);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RegisterAdapter_ReplacesExistingAdapter()
    {
        ILanguageAdapter first = CreateMockAdapter("First", ".cs");
        ILanguageAdapter second = CreateMockAdapter("Second", ".cs");

        Registry.RegisterAdapter(first);
        Registry.RegisterAdapter(second);

        OperationResultGeneric<ILanguageAdapter> result = Registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Same(second, result.Value);
    }

    [Fact]
    public void GetAdapter_ReturnsCorrectAdapter()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");

        Registry.RegisterAdapter(csAdapter);
        Registry.RegisterAdapter(pyAdapter);

        OperationResultGeneric<ILanguageAdapter> csResult = Registry.GetAdapter(".cs");
        OperationResultGeneric<ILanguageAdapter> pyResult = Registry.GetAdapter(".py");

        Assert.True(csResult.IsSuccess);
        Assert.True(pyResult.IsSuccess);
        Assert.Same(csAdapter, csResult.Value);
        Assert.Same(pyAdapter, pyResult.Value);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForUnknownExtension()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");
        Registry.RegisterAdapter(adapter);

        OperationResultGeneric<ILanguageAdapter> result = Registry.GetAdapter(".py");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForEmptyString()
    {
        OperationResultGeneric<ILanguageAdapter> result = Registry.GetAdapter("");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_ReturnsFailureForNullString()
    {
        OperationResultGeneric<ILanguageAdapter> result = Registry.GetAdapter(null!);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetAdapter_CaseInsensitive()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", ".cs");
        Registry.RegisterAdapter(adapter);

        OperationResultGeneric<ILanguageAdapter> upperResult = Registry.GetAdapter(".CS");
        OperationResultGeneric<ILanguageAdapter> mixedResult = Registry.GetAdapter(".Cs");

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

        Registry.RegisterAdapter(csAdapter);
        Registry.RegisterAdapter(pyAdapter);

        string[] extensions = Registry.GetSupportedExtensions();
        Assert.Equal(2, extensions.Length);
        Assert.Contains(".cs", extensions);
        Assert.Contains(".py", extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsEmptyWhenNoneRegistered()
    {
        string[] extensions = Registry.GetSupportedExtensions();
        Assert.Empty(extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsSorted()
    {
        Registry.RegisterAdapter(CreateMockAdapter("Python", ".py"));
        Registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));
        Registry.RegisterAdapter(CreateMockAdapter("TypeScript", ".ts", ".tsx"));

        string[] extensions = Registry.GetSupportedExtensions();
        Assert.Equal(new[] { ".cs", ".py", ".ts", ".tsx" }, extensions);
    }

    [Fact]
    public void IsSupported_ReturnsTrueForRegistered()
    {
        Registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.True(Registry.IsSupported(".cs"));
    }

    [Fact]
    public void IsSupported_ReturnsFalseForUnregistered()
    {
        Assert.False(Registry.IsSupported(".cs"));
    }

    [Fact]
    public void IsSupported_ReturnsFalseForEmptyString()
    {
        Assert.False(Registry.IsSupported(""));
    }

    [Fact]
    public void IsSupported_CaseInsensitive()
    {
        Registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.True(Registry.IsSupported(".CS"));
        Assert.True(Registry.IsSupported(".Cs"));
    }

    [Fact]
    public void RegisterAdapter_NormalizesExtensionWithoutDot()
    {
        ILanguageAdapter adapter = CreateMockAdapter("CSharp", "cs");

        Registry.RegisterAdapter(adapter);

        Assert.True(Registry.IsSupported(".cs"));
        OperationResultGeneric<ILanguageAdapter> result = Registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Same(adapter, result.Value);
    }

    [Fact]
    public void GetAdapter_NormalizesExtensionWithoutDot()
    {
        Registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        OperationResultGeneric<ILanguageAdapter> result = Registry.GetAdapter("cs");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MultipleAdapters_IndependentRegistrations()
    {
        ILanguageAdapter csAdapter = CreateMockAdapter("CSharp", ".cs");
        ILanguageAdapter pyAdapter = CreateMockAdapter("Python", ".py");
        ILanguageAdapter tsAdapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        Registry.RegisterAdapter(csAdapter);
        Registry.RegisterAdapter(pyAdapter);
        Registry.RegisterAdapter(tsAdapter);

        OperationResultGeneric<ILanguageAdapter> csResult = Registry.GetAdapter(".cs");
        OperationResultGeneric<ILanguageAdapter> pyResult = Registry.GetAdapter(".py");
        OperationResultGeneric<ILanguageAdapter> tsResult = Registry.GetAdapter(".ts");
        OperationResultGeneric<ILanguageAdapter> tsxResult = Registry.GetAdapter(".tsx");

        Assert.True(csResult.IsSuccess);
        Assert.True(pyResult.IsSuccess);
        Assert.True(tsResult.IsSuccess);
        Assert.True(tsxResult.IsSuccess);
        Assert.Same(csAdapter, csResult.Value);
        Assert.Same(pyAdapter, pyResult.Value);
        Assert.Same(tsAdapter, tsResult.Value);
        Assert.Same(tsAdapter, tsxResult.Value);
        Assert.Equal(4, Registry.GetSupportedExtensions().Length);
    }

    [Fact]
    public void CreateRegistry_RegistersBothCSharpAndPython()
    {
        LanguageRegistry registry = Host.Program.CreateRegistry();

        Assert.True(registry.IsSupported(".cs"));
        Assert.True(registry.IsSupported(".py"));
    }

    [Fact]
    public void CreateRegistry_CSharpAdapterResolvesCorrectly()
    {
        LanguageRegistry registry = Host.Program.CreateRegistry();

        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(".cs");
        Assert.True(result.IsSuccess);
        Assert.Equal("CSharp", result.Value.LanguageName);
    }

    [Fact]
    public void CreateRegistry_PythonAdapterResolvesCorrectly()
    {
        LanguageRegistry registry = Host.Program.CreateRegistry();

        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(".py");
        Assert.True(result.IsSuccess);
        Assert.Equal("Python", result.Value.LanguageName);
    }

    [Fact]
    public void CreateRegistry_UnsupportedExtensionFails()
    {
        LanguageRegistry registry = Host.Program.CreateRegistry();

        OperationResultGeneric<ILanguageAdapter> result = registry.GetAdapter(".js");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CreateRegistry_SupportedExtensionsMatchExpectedList()
    {
        LanguageRegistry registry = Host.Program.CreateRegistry();

        string[] expected = new[] { ".cs", ".py" };
        string[] actual = registry.GetSupportedExtensions();

        Array.Sort(expected);
        Array.Sort(actual);
        Assert.Equal(expected, actual);
    }
}
