using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Services;

public class IdentifierMapperTests : IDisposable
{
    public string _tempDir;
    public IdentifierMapper _mapper = new();

    public IdentifierMapperTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"mapper_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private void CreateMapFile(string json)
    {
        string dir = Path.Combine(_tempDir, ".multilingual");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "identifier-map.json"), json);
    }

    [Fact]
    public void LoadMap_EmptyProject_StartsEmpty()
    {
        OperationResult result = _mapper.LoadMap(_tempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _mapper.IdentifierCount);
        Assert.Equal(0, _mapper.LiteralCount);
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

        OperationResult result = _mapper.LoadMap(_tempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, _mapper.IdentifierCount);
        Assert.Equal(1, _mapper.LiteralCount);
    }

    [Fact]
    public void LoadMap_NullPath_ReturnsFailure()
    {
        OperationResult result = _mapper.LoadMap(null!);

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

        OperationResult result = await _mapper.LoadMapAsync(_tempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _mapper.IdentifierCount);
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
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> ptResult = _mapper.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = _mapper.GetTranslation("GetName", "es-es");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
    }

    [Fact]
    public void GetTranslation_UnknownIdentifier_ReturnsFailure()
    {
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> result = _mapper.GetTranslation("Unknown", "pt-br");
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
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> result = _mapper.GetTranslation("GetName", "fr-fr");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetTranslation_EmptyOrNull_ReturnsFailure()
    {
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> emptyIdResult = _mapper.GetTranslation("", "pt-br");
        OperationResultGeneric<string> nullIdResult = _mapper.GetTranslation(null!, "pt-br");
        OperationResultGeneric<string> emptyLangResult = _mapper.GetTranslation("GetName", "");
        OperationResultGeneric<string> nullLangResult = _mapper.GetTranslation("GetName", null!);

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
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> getNameResult = _mapper.GetOriginal("ObterNome", "pt-br");
        OperationResultGeneric<string> setValueResult = _mapper.GetOriginal("DefinirValor", "pt-br");

        Assert.True(getNameResult.IsSuccess);
        Assert.Equal("GetName", getNameResult.Value);
        Assert.True(setValueResult.IsSuccess);
        Assert.Equal("SetValue", setValueResult.Value);
    }

    [Fact]
    public void GetOriginal_UnknownTranslation_ReturnsFailure()
    {
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> result = _mapper.GetOriginal("Desconhecido", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetOriginal_EmptyOrNull_ReturnsFailure()
    {
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> emptyResult = _mapper.GetOriginal("", "pt-br");
        OperationResultGeneric<string> nullResult = _mapper.GetOriginal(null!, "pt-br");
        OperationResultGeneric<string> emptyLangResult = _mapper.GetOriginal("ObterNome", "");

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
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> calcResult = _mapper.GetLiteralTranslation("Calculator", "pt-br");
        OperationResultGeneric<string> helloResult = _mapper.GetLiteralTranslation("Hello World", "es-es");

        Assert.True(calcResult.IsSuccess);
        Assert.Equal("Calculadora", calcResult.Value);
        Assert.True(helloResult.IsSuccess);
        Assert.Equal("Hola Mundo", helloResult.Value);
    }

    [Fact]
    public void GetLiteralTranslation_UnknownLiteral_ReturnsFailure()
    {
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> result = _mapper.GetLiteralTranslation("Unknown", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetLiteralTranslation_EmptyOrNull_ReturnsFailure()
    {
        _mapper.LoadMap(_tempDir);

        OperationResultGeneric<string> emptyResult = _mapper.GetLiteralTranslation("", "pt-br");
        OperationResultGeneric<string> nullResult = _mapper.GetLiteralTranslation(null!, "pt-br");

        Assert.False(emptyResult.IsSuccess);
        Assert.False(nullResult.IsSuccess);
    }

    [Fact]
    public void SetTranslation_AddsNewMapping()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        OperationResultGeneric<string> result = _mapper.GetTranslation("GetName", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("ObterNome", result.Value);
        Assert.Equal(1, _mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_MultipleLanguages()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");

        OperationResultGeneric<string> ptResult = _mapper.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = _mapper.GetTranslation("GetName", "es-es");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
        Assert.Equal(1, _mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_OverwritesExisting()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SetTranslation("GetName", "pt-br", "PegarNome");

        OperationResultGeneric<string> result = _mapper.GetTranslation("GetName", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("PegarNome", result.Value);
    }

    [Fact]
    public void SetTranslation_DoesNotThrowOnNull()
    {
        _mapper.LoadMap(_tempDir);

        // SetTranslation no longer throws - it just silently handles nulls
        _mapper.SetTranslation(null!, "pt-br", "x");
        _mapper.SetTranslation("x", null!, "x");
        _mapper.SetTranslation("x", "pt-br", null!);
    }

    [Fact]
    public void SetLiteralTranslation_AddsNewMapping()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        OperationResultGeneric<string> result = _mapper.GetLiteralTranslation("Calculator", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("Calculadora", result.Value);
        Assert.Equal(1, _mapper.LiteralCount);
    }

    [Fact]
    public void RemoveIdentifier_ExistingMapping_ReturnsTrue()
    {
        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        Assert.True(_mapper.RemoveIdentifier("GetName"));
        OperationResultGeneric<string> result = _mapper.GetTranslation("GetName", "pt-br");
        Assert.False(result.IsSuccess);
        Assert.Equal(0, _mapper.IdentifierCount);
    }

    [Fact]
    public void RemoveIdentifier_NonExistent_ReturnsFalse()
    {
        _mapper.LoadMap(_tempDir);

        Assert.False(_mapper.RemoveIdentifier("NonExistent"));
    }

    [Fact]
    public void SaveMap_CreatesFileAndDirectory()
    {
        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        OperationResult saveResult = _mapper.SaveMap();

        Assert.True(saveResult.IsSuccess);
        string filePath = Path.Combine(_tempDir, ".multilingual", "identifier-map.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void SaveMap_RoundTrip()
    {
        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");
        _mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");
        _mapper.SaveMap();

        // Load in a new mapper
        IdentifierMapper mapper2 = new IdentifierMapper();
        mapper2.LoadMap(_tempDir);

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
        string otherDir = Path.Combine(_tempDir, "other-project");
        Directory.CreateDirectory(otherDir);

        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        OperationResult saveResult = _mapper.SaveMap(otherDir);

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
        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        OperationResultGeneric<string> result = _mapper.GetOriginal("obternome", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CalculatorExample_FullWorkflow()
    {
        _mapper.LoadMap(_tempDir);

        // Set up Calculator project mappings
        _mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        _mapper.SetTranslation("Add", "pt-br", "Somar");
        _mapper.SetTranslation("Subtract", "pt-br", "Subtrair");
        _mapper.SetTranslation("result", "pt-br", "resultado");
        _mapper.SetLiteralTranslation("Enter a number:", "pt-br", "Digite um numero:");

        // Verify forward lookups
        OperationResultGeneric<string> calcResult = _mapper.GetTranslation("Calculator", "pt-br");
        OperationResultGeneric<string> addResult = _mapper.GetTranslation("Add", "pt-br");
        OperationResultGeneric<string> subResult = _mapper.GetTranslation("Subtract", "pt-br");
        OperationResultGeneric<string> resResult = _mapper.GetTranslation("result", "pt-br");
        OperationResultGeneric<string> litResult = _mapper.GetLiteralTranslation("Enter a number:", "pt-br");

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
        OperationResultGeneric<string> revCalcResult = _mapper.GetOriginal("Calculadora", "pt-br");
        OperationResultGeneric<string> revAddResult = _mapper.GetOriginal("Somar", "pt-br");

        Assert.True(revCalcResult.IsSuccess);
        Assert.Equal("Calculator", revCalcResult.Value);
        Assert.True(revAddResult.IsSuccess);
        Assert.Equal("Add", revAddResult.Value);

        // Save and reload
        _mapper.SaveMap();

        IdentifierMapper mapper2 = new IdentifierMapper();
        mapper2.LoadMap(_tempDir);
        Assert.Equal(4, mapper2.IdentifierCount);
        Assert.Equal(1, mapper2.LiteralCount);
        OperationResultGeneric<string> reloadResult = mapper2.GetTranslation("Calculator", "pt-br");
        Assert.True(reloadResult.IsSuccess);
        Assert.Equal("Calculadora", reloadResult.Value);
    }
}
