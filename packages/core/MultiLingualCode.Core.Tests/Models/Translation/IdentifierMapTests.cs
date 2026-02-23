using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Models.Translation;

namespace MultiLingualCode.Core.Tests.Models.Translation;

public class IdentifierMapTests
{
    static string GetTestDataPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    [Fact]
    public void LoadFrom_ValidFile_LoadsIdentifiers()
    {
        OperationResultGeneric<IdentifierMap> result = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Count);
    }

    [Fact]
    public void LoadFrom_NonExistentFile_ReturnsFailure()
    {
        OperationResultGeneric<IdentifierMap> result = IdentifierMap.LoadFrom("nonexistent.json");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadFromAsync_ValidFile_LoadsIdentifiers()
    {
        OperationResultGeneric<IdentifierMap> result = await IdentifierMap.LoadFromAsync(GetTestDataPath("identifier-map.json"));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Count);
    }

    [Fact]
    public void GetTranslated_KnownOriginal_ReturnsTranslation()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> getNameResult = map.GetTranslated("GetName");
        Assert.True(getNameResult.IsSuccess);
        Assert.Equal("ObterNome", getNameResult.Value);

        OperationResultGeneric<string> setValueResult = map.GetTranslated("SetValue");
        Assert.True(setValueResult.IsSuccess);
        Assert.Equal("DefinirValor", setValueResult.Value);

        OperationResultGeneric<string> isValidResult = map.GetTranslated("IsValid");
        Assert.True(isValidResult.IsSuccess);
        Assert.Equal("EhValido", isValidResult.Value);
    }

    [Fact]
    public void GetTranslated_UnknownOriginal_ReturnsFailure()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> result = map.GetTranslated("NonExistent");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetTranslated_EmptyOrNull_ReturnsFailure()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> emptyResult = map.GetTranslated("");
        Assert.False(emptyResult.IsSuccess);

        OperationResultGeneric<string> nullResult = map.GetTranslated(null!);
        Assert.False(nullResult.IsSuccess);
    }

    [Fact]
    public void GetOriginal_KnownTranslation_ReturnsOriginal()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> getNameResult = map.GetOriginal("ObterNome");
        Assert.True(getNameResult.IsSuccess);
        Assert.Equal("GetName", getNameResult.Value);

        OperationResultGeneric<string> setValueResult = map.GetOriginal("DefinirValor");
        Assert.True(setValueResult.IsSuccess);
        Assert.Equal("SetValue", setValueResult.Value);
    }

    [Fact]
    public void GetOriginal_UnknownTranslation_ReturnsFailure()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> result = map.GetOriginal("Desconhecido");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetOriginal_EmptyOrNull_ReturnsFailure()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> emptyResult = map.GetOriginal("");
        Assert.False(emptyResult.IsSuccess);

        OperationResultGeneric<string> nullResult = map.GetOriginal(null!);
        Assert.False(nullResult.IsSuccess);
    }

    [Fact]
    public void BidirectionalLookup_IsConsistent()
    {
        OperationResultGeneric<IdentifierMap> loadResult = IdentifierMap.LoadFrom(GetTestDataPath("identifier-map.json"));
        Assert.True(loadResult.IsSuccess);
        IdentifierMap map = loadResult.Value;

        OperationResultGeneric<string> translatedResult = map.GetTranslated("ToString");
        Assert.True(translatedResult.IsSuccess);
        Assert.Equal("ParaTexto", translatedResult.Value);

        OperationResultGeneric<string> originalResult = map.GetOriginal(translatedResult.Value);
        Assert.True(originalResult.IsSuccess);
        Assert.Equal("ToString", originalResult.Value);
    }

    [Fact]
    public void Set_AddsNewMapping()
    {
        IdentifierMap map = new IdentifierMap();

        map.Set("MyMethod", "MeuMetodo");

        OperationResultGeneric<string> translatedResult = map.GetTranslated("MyMethod");
        Assert.True(translatedResult.IsSuccess);
        Assert.Equal("MeuMetodo", translatedResult.Value);

        OperationResultGeneric<string> originalResult = map.GetOriginal("MeuMetodo");
        Assert.True(originalResult.IsSuccess);
        Assert.Equal("MyMethod", originalResult.Value);

        Assert.Equal(1, map.Count);
    }

    [Fact]
    public void Set_UpdatesExistingMapping()
    {
        IdentifierMap map = new IdentifierMap();

        map.Set("MyMethod", "MeuMetodo");
        map.Set("MyMethod", "MinhaFuncao");

        OperationResultGeneric<string> translatedResult = map.GetTranslated("MyMethod");
        Assert.True(translatedResult.IsSuccess);
        Assert.Equal("MinhaFuncao", translatedResult.Value);

        OperationResultGeneric<string> originalResult = map.GetOriginal("MinhaFuncao");
        Assert.True(originalResult.IsSuccess);
        Assert.Equal("MyMethod", originalResult.Value);

        OperationResultGeneric<string> oldReverseResult = map.GetOriginal("MeuMetodo"); // old reverse mapping removed
        Assert.False(oldReverseResult.IsSuccess);

        Assert.Equal(1, map.Count);
    }

    [Fact]
    public void Set_NullValues_DoesNotAdd()
    {
        IdentifierMap map = new IdentifierMap();

        map.Set(null!, "value");
        map.Set("key", null!);
        map.Set("", "value");
        map.Set("key", "");

        Assert.Equal(0, map.Count);
    }

    [Fact]
    public void Remove_ExistingMapping_ReturnsTrue()
    {
        IdentifierMap map = new IdentifierMap();
        map.Set("MyMethod", "MeuMetodo");

        Assert.True(map.Remove("MyMethod"));

        OperationResultGeneric<string> translatedResult = map.GetTranslated("MyMethod");
        Assert.False(translatedResult.IsSuccess);

        OperationResultGeneric<string> originalResult = map.GetOriginal("MeuMetodo");
        Assert.False(originalResult.IsSuccess);

        Assert.Equal(0, map.Count);
    }

    [Fact]
    public void Remove_NonExistentMapping_ReturnsFalse()
    {
        IdentifierMap map = new IdentifierMap();

        Assert.False(map.Remove("NonExistent"));
    }

    [Fact]
    public void IsCaseSensitive_ForIdentifiers()
    {
        IdentifierMap map = new IdentifierMap();
        map.Set("GetName", "ObterNome");

        // Identifiers are case-sensitive (unlike keywords)
        OperationResultGeneric<string> lowerResult = map.GetTranslated("getname");
        Assert.False(lowerResult.IsSuccess);

        OperationResultGeneric<string> upperResult = map.GetTranslated("GETNAME");
        Assert.False(upperResult.IsSuccess);
    }
}
