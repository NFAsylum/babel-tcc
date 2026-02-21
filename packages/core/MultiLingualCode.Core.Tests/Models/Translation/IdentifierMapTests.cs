using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Tests.Models.Translation;

public class IdentifierMapTests
{
    private static string GetTestDataPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    [Fact]
    public void LoadFrom_ValidFile_LoadsIdentifiers()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Equal(5, map.Count);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            IdentifierMap.LoadFrom("nonexistent.json"));
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsIdentifiers()
    {
        var map = await IdentifierMap.LoadFromAsync(GetTestDataPath("identifier-map.json"));

        Assert.Equal(5, map.Count);
    }

    [Fact]
    public void GetTranslated_KnownOriginal_ReturnsTranslation()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Equal("ObterNome", map.GetTranslated("GetName"));
        Assert.Equal("DefinirValor", map.GetTranslated("SetValue"));
        Assert.Equal("EhValido", map.GetTranslated("IsValid"));
    }

    [Fact]
    public void GetTranslated_UnknownOriginal_ReturnsNull()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Null(map.GetTranslated("NonExistent"));
    }

    [Fact]
    public void GetTranslated_EmptyOrNull_ReturnsNull()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Null(map.GetTranslated(""));
        Assert.Null(map.GetTranslated(null!));
    }

    [Fact]
    public void GetOriginal_KnownTranslation_ReturnsOriginal()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Equal("GetName", map.GetOriginal("ObterNome"));
        Assert.Equal("SetValue", map.GetOriginal("DefinirValor"));
    }

    [Fact]
    public void GetOriginal_UnknownTranslation_ReturnsNull()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Null(map.GetOriginal("Desconhecido"));
    }

    [Fact]
    public void GetOriginal_EmptyOrNull_ReturnsNull()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.Null(map.GetOriginal(""));
        Assert.Null(map.GetOriginal(null!));
    }

    [Fact]
    public void BidirectionalLookup_IsConsistent()
    {
        var map = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        var translated = map.GetTranslated("ToString");
        Assert.Equal("ParaTexto", translated);

        var original = map.GetOriginal(translated!);
        Assert.Equal("ToString", original);
    }

    [Fact]
    public void Set_AddsNewMapping()
    {
        var map = new IdentifierMap();

        map.Set("MyMethod", "MeuMetodo");

        Assert.Equal("MeuMetodo", map.GetTranslated("MyMethod"));
        Assert.Equal("MyMethod", map.GetOriginal("MeuMetodo"));
        Assert.Equal(1, map.Count);
    }

    [Fact]
    public void Set_UpdatesExistingMapping()
    {
        var map = new IdentifierMap();

        map.Set("MyMethod", "MeuMetodo");
        map.Set("MyMethod", "MinhaFuncao");

        Assert.Equal("MinhaFuncao", map.GetTranslated("MyMethod"));
        Assert.Equal("MyMethod", map.GetOriginal("MinhaFuncao"));
        Assert.Null(map.GetOriginal("MeuMetodo")); // old reverse mapping removed
        Assert.Equal(1, map.Count);
    }

    [Fact]
    public void Set_ThrowsOnNull()
    {
        var map = new IdentifierMap();

        Assert.Throws<ArgumentNullException>(() => map.Set(null!, "value"));
        Assert.Throws<ArgumentNullException>(() => map.Set("key", null!));
    }

    [Fact]
    public void Remove_ExistingMapping_ReturnsTrue()
    {
        var map = new IdentifierMap();
        map.Set("MyMethod", "MeuMetodo");

        Assert.True(map.Remove("MyMethod"));
        Assert.Null(map.GetTranslated("MyMethod"));
        Assert.Null(map.GetOriginal("MeuMetodo"));
        Assert.Equal(0, map.Count);
    }

    [Fact]
    public void Remove_NonExistentMapping_ReturnsFalse()
    {
        var map = new IdentifierMap();

        Assert.False(map.Remove("NonExistent"));
    }

    [Fact]
    public void IsCaseSensitive_ForIdentifiers()
    {
        var map = new IdentifierMap();
        map.Set("GetName", "ObterNome");

        // Identifiers are case-sensitive (unlike keywords)
        Assert.Null(map.GetTranslated("getname"));
        Assert.Null(map.GetTranslated("GETNAME"));
    }
}
