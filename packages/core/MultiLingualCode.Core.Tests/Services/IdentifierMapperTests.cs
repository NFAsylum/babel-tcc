using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Services;

public class IdentifierMapperTests : IDisposable
{
    public string tempDir;
    public IdentifierMapper mapper = new();

    public IdentifierMapperTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), $"mapper_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private void CreateMapFile(string json)
    {
        string dir = Path.Combine(tempDir, ".multilingual");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "identifier-map.json"), json);
    }

    [Fact]
    public void LoadMap_EmptyProject_StartsEmpty()
    {
        OperationResult result = mapper.LoadMap(tempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, mapper.IdentifierCount);
        Assert.Equal(0, mapper.LiteralCount);
    }

    [Fact]
    public void LoadMap_WithExistingFile_LoadsData()
    {
        CreateMapFile("""
        {
          "identifiers": {
            "GetName": { "pt-br": "ObterNome" },
            "SetValue": { "pt-br": "DefinirValor", "es-es": "EstablecerValor" }
          },
          "literals": {
            "Calculator": { "pt-br": "Calculadora" }
          }
        }
        """);

        OperationResult result = mapper.LoadMap(tempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, mapper.IdentifierCount);
        Assert.Equal(1, mapper.LiteralCount);
    }

    [Fact]
    public void LoadMap_NullPath_ReturnsFailure()
    {
        OperationResult result = mapper.LoadMap(null!);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadMapAsync_WithExistingFile_LoadsData()
    {
        CreateMapFile("""
        {
          "identifiers": {
            "GetName": { "pt-br": "ObterNome" }
          },
          "literals": {}
        }
        """);

        OperationResult result = await mapper.LoadMapAsync(tempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, mapper.IdentifierCount);
    }

    [Fact]
    public void GetTranslation_KnownIdentifier_ReturnsTranslation()
    {
        CreateMapFile("""
        {
          "identifiers": {
            "GetName": { "pt-br": "ObterNome", "es-es": "ObtenerNombre" }
          },
          "literals": {}
        }
        """);
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> ptResult = mapper.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = mapper.GetTranslation("GetName", "es-es");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
    }

    [Fact]
    public void GetTranslation_UnknownIdentifier_ReturnsFailure()
    {
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> result = mapper.GetTranslation("Unknown", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetTranslation_UnknownLanguage_ReturnsFailure()
    {
        CreateMapFile("""
        {
          "identifiers": { "GetName": { "pt-br": "ObterNome" } },
          "literals": {}
        }
        """);
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> result = mapper.GetTranslation("GetName", "fr-fr");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetTranslation_EmptyOrNull_ReturnsFailure()
    {
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> emptyIdResult = mapper.GetTranslation("", "pt-br");
        OperationResultGeneric<string> nullIdResult = mapper.GetTranslation(null!, "pt-br");
        OperationResultGeneric<string> emptyLangResult = mapper.GetTranslation("GetName", "");
        OperationResultGeneric<string> nullLangResult = mapper.GetTranslation("GetName", null!);

        Assert.False(emptyIdResult.IsSuccess);
        Assert.False(nullIdResult.IsSuccess);
        Assert.False(emptyLangResult.IsSuccess);
        Assert.False(nullLangResult.IsSuccess);
    }

    [Fact]
    public void GetOriginal_KnownTranslation_ReturnsOriginal()
    {
        CreateMapFile("""
        {
          "identifiers": {
            "GetName": { "pt-br": "ObterNome" },
            "SetValue": { "pt-br": "DefinirValor" }
          },
          "literals": {}
        }
        """);
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> getNameResult = mapper.GetOriginal("ObterNome", "pt-br");
        OperationResultGeneric<string> setValueResult = mapper.GetOriginal("DefinirValor", "pt-br");

        Assert.True(getNameResult.IsSuccess);
        Assert.Equal("GetName", getNameResult.Value);
        Assert.True(setValueResult.IsSuccess);
        Assert.Equal("SetValue", setValueResult.Value);
    }

    [Fact]
    public void GetOriginal_UnknownTranslation_ReturnsFailure()
    {
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> result = mapper.GetOriginal("Desconhecido", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetOriginal_EmptyOrNull_ReturnsFailure()
    {
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> emptyResult = mapper.GetOriginal("", "pt-br");
        OperationResultGeneric<string> nullResult = mapper.GetOriginal(null!, "pt-br");
        OperationResultGeneric<string> emptyLangResult = mapper.GetOriginal("ObterNome", "");

        Assert.False(emptyResult.IsSuccess);
        Assert.False(nullResult.IsSuccess);
        Assert.False(emptyLangResult.IsSuccess);
    }

    [Fact]
    public void GetLiteralTranslation_KnownLiteral_ReturnsTranslation()
    {
        CreateMapFile("""
        {
          "identifiers": {},
          "literals": {
            "Calculator": { "pt-br": "Calculadora" },
            "Hello World": { "pt-br": "Ola Mundo", "es-es": "Hola Mundo" }
          }
        }
        """);
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> calcResult = mapper.GetLiteralTranslation("Calculator", "pt-br");
        OperationResultGeneric<string> helloResult = mapper.GetLiteralTranslation("Hello World", "es-es");

        Assert.True(calcResult.IsSuccess);
        Assert.Equal("Calculadora", calcResult.Value);
        Assert.True(helloResult.IsSuccess);
        Assert.Equal("Hola Mundo", helloResult.Value);
    }

    [Fact]
    public void GetLiteralTranslation_UnknownLiteral_ReturnsFailure()
    {
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> result = mapper.GetLiteralTranslation("Unknown", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetLiteralTranslation_EmptyOrNull_ReturnsFailure()
    {
        mapper.LoadMap(tempDir);

        OperationResultGeneric<string> emptyResult = mapper.GetLiteralTranslation("", "pt-br");
        OperationResultGeneric<string> nullResult = mapper.GetLiteralTranslation(null!, "pt-br");

        Assert.False(emptyResult.IsSuccess);
        Assert.False(nullResult.IsSuccess);
    }

    [Fact]
    public void SetTranslation_AddsNewMapping()
    {
        mapper.LoadMap(tempDir);

        mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        OperationResultGeneric<string> result = mapper.GetTranslation("GetName", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("ObterNome", result.Value);
        Assert.Equal(1, mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_MultipleLanguages()
    {
        mapper.LoadMap(tempDir);

        mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");

        OperationResultGeneric<string> ptResult = mapper.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = mapper.GetTranslation("GetName", "es-es");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
        Assert.Equal(1, mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_OverwritesExisting()
    {
        mapper.LoadMap(tempDir);

        mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        mapper.SetTranslation("GetName", "pt-br", "PegarNome");

        OperationResultGeneric<string> result = mapper.GetTranslation("GetName", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("PegarNome", result.Value);
    }

    [Fact]
    public void SetTranslation_DoesNotThrowOnNull()
    {
        mapper.LoadMap(tempDir);

        // SetTranslation no longer throws - it just silently handles nulls
        mapper.SetTranslation(null!, "pt-br", "x");
        mapper.SetTranslation("x", null!, "x");
        mapper.SetTranslation("x", "pt-br", null!);
    }

    [Fact]
    public void SetLiteralTranslation_AddsNewMapping()
    {
        mapper.LoadMap(tempDir);

        mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        OperationResultGeneric<string> result = mapper.GetLiteralTranslation("Calculator", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("Calculadora", result.Value);
        Assert.Equal(1, mapper.LiteralCount);
    }

    [Fact]
    public void RemoveIdentifier_ExistingMapping_ReturnsTrue()
    {
        mapper.LoadMap(tempDir);
        mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        Assert.True(mapper.RemoveIdentifier("GetName"));
        OperationResultGeneric<string> result = mapper.GetTranslation("GetName", "pt-br");
        Assert.False(result.IsSuccess);
        Assert.Equal(0, mapper.IdentifierCount);
    }

    [Fact]
    public void RemoveIdentifier_NonExistent_ReturnsFalse()
    {
        mapper.LoadMap(tempDir);

        Assert.False(mapper.RemoveIdentifier("NonExistent"));
    }

    [Fact]
    public void SaveMap_CreatesFileAndDirectory()
    {
        mapper.LoadMap(tempDir);
        mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        OperationResult saveResult = mapper.SaveMap();

        Assert.True(saveResult.IsSuccess);
        string filePath = Path.Combine(tempDir, ".multilingual", "identifier-map.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void SaveMap_RoundTrip()
    {
        mapper.LoadMap(tempDir);
        mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");
        mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");
        mapper.SaveMap();

        // Load in a new mapper
        IdentifierMapper mapper2 = new IdentifierMapper();
        mapper2.LoadMap(tempDir);

        OperationResultGeneric<string> ptResult = mapper2.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = mapper2.GetTranslation("GetName", "es-es");
        OperationResultGeneric<string> litResult = mapper2.GetLiteralTranslation("Calculator", "pt-br");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
        Assert.True(litResult.IsSuccess);
        Assert.Equal("Calculadora", litResult.Value);
    }

    [Fact]
    public void SaveMap_WithExplicitPath()
    {
        string otherDir = Path.Combine(tempDir, "other-project");
        Directory.CreateDirectory(otherDir);

        mapper.LoadMap(tempDir);
        mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        OperationResult saveResult = mapper.SaveMap(otherDir);

        Assert.True(saveResult.IsSuccess);
        string filePath = Path.Combine(otherDir, ".multilingual", "identifier-map.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void SaveMap_WithoutLoadMap_ReturnsFailure()
    {
        IdentifierMapper mapper = new IdentifierMapper();

        OperationResult result = mapper.SaveMap();

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetOriginal_IsCaseSensitive()
    {
        mapper.LoadMap(tempDir);
        mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        OperationResultGeneric<string> result = mapper.GetOriginal("obternome", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CalculatorExample_FullWorkflow()
    {
        mapper.LoadMap(tempDir);

        // Set up Calculator project mappings
        mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        mapper.SetTranslation("Add", "pt-br", "Somar");
        mapper.SetTranslation("Subtract", "pt-br", "Subtrair");
        mapper.SetTranslation("result", "pt-br", "resultado");
        mapper.SetLiteralTranslation("Enter a number:", "pt-br", "Digite um numero:");

        // Verify forward lookups
        OperationResultGeneric<string> calcResult = mapper.GetTranslation("Calculator", "pt-br");
        OperationResultGeneric<string> addResult = mapper.GetTranslation("Add", "pt-br");
        OperationResultGeneric<string> subResult = mapper.GetTranslation("Subtract", "pt-br");
        OperationResultGeneric<string> resResult = mapper.GetTranslation("result", "pt-br");
        OperationResultGeneric<string> litResult = mapper.GetLiteralTranslation("Enter a number:", "pt-br");

        Assert.True(calcResult.IsSuccess);
        Assert.Equal("Calculadora", calcResult.Value);
        Assert.True(addResult.IsSuccess);
        Assert.Equal("Somar", addResult.Value);
        Assert.True(subResult.IsSuccess);
        Assert.Equal("Subtrair", subResult.Value);
        Assert.True(resResult.IsSuccess);
        Assert.Equal("resultado", resResult.Value);
        Assert.True(litResult.IsSuccess);
        Assert.Equal("Digite um numero:", litResult.Value);

        // Verify reverse lookups
        OperationResultGeneric<string> revCalcResult = mapper.GetOriginal("Calculadora", "pt-br");
        OperationResultGeneric<string> revAddResult = mapper.GetOriginal("Somar", "pt-br");

        Assert.True(revCalcResult.IsSuccess);
        Assert.Equal("Calculator", revCalcResult.Value);
        Assert.True(revAddResult.IsSuccess);
        Assert.Equal("Add", revAddResult.Value);

        // Save and reload
        mapper.SaveMap();

        IdentifierMapper mapper2 = new IdentifierMapper();
        mapper2.LoadMap(tempDir);
        Assert.Equal(4, mapper2.IdentifierCount);
        Assert.Equal(1, mapper2.LiteralCount);
        OperationResultGeneric<string> reloadResult = mapper2.GetTranslation("Calculator", "pt-br");
        Assert.True(reloadResult.IsSuccess);
        Assert.Equal("Calculadora", reloadResult.Value);
    }
}
