using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.AST;
using MultiLingualCode.Core.Services;
using NSubstitute;

namespace MultiLingualCode.Core.Tests.Services;

public class LanguageRegistryTests
{
    private readonly LanguageRegistry _registry = new();

    private static ILanguageAdapter CreateMockAdapter(string name, params string[] extensions)
    {
        var adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns(name);
        adapter.FileExtensions.Returns(extensions);
        return adapter;
    }

    [Fact]
    public void RegisterAdapter_MakesExtensionsAvailable()
    {
        var adapter = CreateMockAdapter("CSharp", ".cs");

        _registry.RegisterAdapter(adapter);

        Assert.True(_registry.IsSupported(".cs"));
    }

    [Fact]
    public void RegisterAdapter_MultipleExtensions()
    {
        var adapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        _registry.RegisterAdapter(adapter);

        Assert.True(_registry.IsSupported(".ts"));
        Assert.True(_registry.IsSupported(".tsx"));
    }

    [Fact]
    public void RegisterAdapter_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => _registry.RegisterAdapter(null!));
    }

    [Fact]
    public void RegisterAdapter_ThrowsOnEmptyExtensions()
    {
        var adapter = CreateMockAdapter("Empty");

        Assert.Throws<ArgumentException>(() => _registry.RegisterAdapter(adapter));
    }

    [Fact]
    public void RegisterAdapter_ThrowsOnNullExtensions()
    {
        var adapter = Substitute.For<ILanguageAdapter>();
        adapter.LanguageName.Returns("Null");
        adapter.FileExtensions.Returns((string[]?)null);

        Assert.Throws<ArgumentException>(() => _registry.RegisterAdapter(adapter));
    }

    [Fact]
    public void RegisterAdapter_ReplacesExistingAdapter()
    {
        var first = CreateMockAdapter("First", ".cs");
        var second = CreateMockAdapter("Second", ".cs");

        _registry.RegisterAdapter(first);
        _registry.RegisterAdapter(second);

        var result = _registry.GetAdapter(".cs");
        Assert.Same(second, result);
    }

    [Fact]
    public void GetAdapter_ReturnsCorrectAdapter()
    {
        var csAdapter = CreateMockAdapter("CSharp", ".cs");
        var pyAdapter = CreateMockAdapter("Python", ".py");

        _registry.RegisterAdapter(csAdapter);
        _registry.RegisterAdapter(pyAdapter);

        Assert.Same(csAdapter, _registry.GetAdapter(".cs"));
        Assert.Same(pyAdapter, _registry.GetAdapter(".py"));
    }

    [Fact]
    public void GetAdapter_ReturnsNullForUnknownExtension()
    {
        var adapter = CreateMockAdapter("CSharp", ".cs");
        _registry.RegisterAdapter(adapter);

        Assert.Null(_registry.GetAdapter(".py"));
    }

    [Fact]
    public void GetAdapter_ReturnsNullForEmptyString()
    {
        Assert.Null(_registry.GetAdapter(""));
    }

    [Fact]
    public void GetAdapter_ReturnsNullForNullString()
    {
        Assert.Null(_registry.GetAdapter(null!));
    }

    [Fact]
    public void GetAdapter_CaseInsensitive()
    {
        var adapter = CreateMockAdapter("CSharp", ".cs");
        _registry.RegisterAdapter(adapter);

        Assert.Same(adapter, _registry.GetAdapter(".CS"));
        Assert.Same(adapter, _registry.GetAdapter(".Cs"));
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsAllRegistered()
    {
        var csAdapter = CreateMockAdapter("CSharp", ".cs");
        var pyAdapter = CreateMockAdapter("Python", ".py");

        _registry.RegisterAdapter(csAdapter);
        _registry.RegisterAdapter(pyAdapter);

        var extensions = _registry.GetSupportedExtensions();
        Assert.Equal(2, extensions.Length);
        Assert.Contains(".cs", extensions);
        Assert.Contains(".py", extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsEmptyWhenNoneRegistered()
    {
        var extensions = _registry.GetSupportedExtensions();
        Assert.Empty(extensions);
    }

    [Fact]
    public void GetSupportedExtensions_ReturnsSorted()
    {
        _registry.RegisterAdapter(CreateMockAdapter("Python", ".py"));
        _registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));
        _registry.RegisterAdapter(CreateMockAdapter("TypeScript", ".ts", ".tsx"));

        var extensions = _registry.GetSupportedExtensions();
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
        var adapter = CreateMockAdapter("CSharp", "cs");

        _registry.RegisterAdapter(adapter);

        Assert.True(_registry.IsSupported(".cs"));
        Assert.Same(adapter, _registry.GetAdapter(".cs"));
    }

    [Fact]
    public void GetAdapter_NormalizesExtensionWithoutDot()
    {
        _registry.RegisterAdapter(CreateMockAdapter("CSharp", ".cs"));

        Assert.NotNull(_registry.GetAdapter("cs"));
    }

    [Fact]
    public void MultipleAdapters_IndependentRegistrations()
    {
        var csAdapter = CreateMockAdapter("CSharp", ".cs");
        var pyAdapter = CreateMockAdapter("Python", ".py");
        var tsAdapter = CreateMockAdapter("TypeScript", ".ts", ".tsx");

        _registry.RegisterAdapter(csAdapter);
        _registry.RegisterAdapter(pyAdapter);
        _registry.RegisterAdapter(tsAdapter);

        Assert.Same(csAdapter, _registry.GetAdapter(".cs"));
        Assert.Same(pyAdapter, _registry.GetAdapter(".py"));
        Assert.Same(tsAdapter, _registry.GetAdapter(".ts"));
        Assert.Same(tsAdapter, _registry.GetAdapter(".tsx"));
        Assert.Equal(4, _registry.GetSupportedExtensions().Length);
    }
}
