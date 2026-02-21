using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Services;

public class IdentifierMapperTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IdentifierMapper _mapper = new();

    public IdentifierMapperTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"mapper_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private void CreateMapFile(string json)
    {
        var dir = Path.Combine(_tempDir, ".multilingual");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "identifier-map.json"), json);
    }

    [Fact]
    public void LoadMap_EmptyProject_StartsEmpty()
    {
        _mapper.LoadMap(_tempDir);

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

        _mapper.LoadMap(_tempDir);

        Assert.Equal(2, _mapper.IdentifierCount);
        Assert.Equal(1, _mapper.LiteralCount);
    }

    [Fact]
    public void LoadMap_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.LoadMap(null!));
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

        await _mapper.LoadMapAsync(_tempDir);

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

        Assert.Equal("ObterNome", _mapper.GetTranslation("GetName", "pt-br"));
        Assert.Equal("ObtenerNombre", _mapper.GetTranslation("GetName", "es-es"));
    }

    [Fact]
    public void GetTranslation_UnknownIdentifier_ReturnsNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetTranslation("Unknown", "pt-br"));
    }

    [Fact]
    public void GetTranslation_UnknownLanguage_ReturnsNull()
    {
        CreateMapFile("""
        {
          "identifiers": { "GetName": { "pt-br": "ObterNome" } },
          "literals": {}
        }
        """);
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetTranslation("GetName", "fr-fr"));
    }

    [Fact]
    public void GetTranslation_EmptyOrNull_ReturnsNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetTranslation("", "pt-br"));
        Assert.Null(_mapper.GetTranslation(null!, "pt-br"));
        Assert.Null(_mapper.GetTranslation("GetName", ""));
        Assert.Null(_mapper.GetTranslation("GetName", null!));
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

        Assert.Equal("GetName", _mapper.GetOriginal("ObterNome", "pt-br"));
        Assert.Equal("SetValue", _mapper.GetOriginal("DefinirValor", "pt-br"));
    }

    [Fact]
    public void GetOriginal_UnknownTranslation_ReturnsNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetOriginal("Desconhecido", "pt-br"));
    }

    [Fact]
    public void GetOriginal_EmptyOrNull_ReturnsNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetOriginal("", "pt-br"));
        Assert.Null(_mapper.GetOriginal(null!, "pt-br"));
        Assert.Null(_mapper.GetOriginal("ObterNome", ""));
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

        Assert.Equal("Calculadora", _mapper.GetLiteralTranslation("Calculator", "pt-br"));
        Assert.Equal("Hola Mundo", _mapper.GetLiteralTranslation("Hello World", "es-es"));
    }

    [Fact]
    public void GetLiteralTranslation_UnknownLiteral_ReturnsNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetLiteralTranslation("Unknown", "pt-br"));
    }

    [Fact]
    public void GetLiteralTranslation_EmptyOrNull_ReturnsNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Null(_mapper.GetLiteralTranslation("", "pt-br"));
        Assert.Null(_mapper.GetLiteralTranslation(null!, "pt-br"));
    }

    [Fact]
    public void SetTranslation_AddsNewMapping()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        Assert.Equal("ObterNome", _mapper.GetTranslation("GetName", "pt-br"));
        Assert.Equal(1, _mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_MultipleLanguages()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");

        Assert.Equal("ObterNome", _mapper.GetTranslation("GetName", "pt-br"));
        Assert.Equal("ObtenerNombre", _mapper.GetTranslation("GetName", "es-es"));
        Assert.Equal(1, _mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_OverwritesExisting()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SetTranslation("GetName", "pt-br", "PegarNome");

        Assert.Equal("PegarNome", _mapper.GetTranslation("GetName", "pt-br"));
    }

    [Fact]
    public void SetTranslation_ThrowsOnNull()
    {
        _mapper.LoadMap(_tempDir);

        Assert.Throws<ArgumentNullException>(() => _mapper.SetTranslation(null!, "pt-br", "x"));
        Assert.Throws<ArgumentNullException>(() => _mapper.SetTranslation("x", null!, "x"));
        Assert.Throws<ArgumentNullException>(() => _mapper.SetTranslation("x", "pt-br", null!));
    }

    [Fact]
    public void SetLiteralTranslation_AddsNewMapping()
    {
        _mapper.LoadMap(_tempDir);

        _mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        Assert.Equal("Calculadora", _mapper.GetLiteralTranslation("Calculator", "pt-br"));
        Assert.Equal(1, _mapper.LiteralCount);
    }

    [Fact]
    public void RemoveIdentifier_ExistingMapping_ReturnsTrue()
    {
        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        Assert.True(_mapper.RemoveIdentifier("GetName"));
        Assert.Null(_mapper.GetTranslation("GetName", "pt-br"));
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

        _mapper.SaveMap();

        var filePath = Path.Combine(_tempDir, ".multilingual", "identifier-map.json");
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
        var mapper2 = new IdentifierMapper();
        mapper2.LoadMap(_tempDir);

        Assert.Equal("ObterNome", mapper2.GetTranslation("GetName", "pt-br"));
        Assert.Equal("ObtenerNombre", mapper2.GetTranslation("GetName", "es-es"));
        Assert.Equal("Calculadora", mapper2.GetLiteralTranslation("Calculator", "pt-br"));
    }

    [Fact]
    public void SaveMap_WithExplicitPath()
    {
        var otherDir = Path.Combine(_tempDir, "other-project");
        Directory.CreateDirectory(otherDir);

        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        _mapper.SaveMap(otherDir);

        var filePath = Path.Combine(otherDir, ".multilingual", "identifier-map.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void SaveMap_WithoutLoadMap_Throws()
    {
        var mapper = new IdentifierMapper();

        Assert.Throws<InvalidOperationException>(() => mapper.SaveMap());
    }

    [Fact]
    public void GetOriginal_IsCaseSensitive()
    {
        _mapper.LoadMap(_tempDir);
        _mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        Assert.Null(_mapper.GetOriginal("obternome", "pt-br"));
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
        Assert.Equal("Calculadora", _mapper.GetTranslation("Calculator", "pt-br"));
        Assert.Equal("Somar", _mapper.GetTranslation("Add", "pt-br"));
        Assert.Equal("Subtrair", _mapper.GetTranslation("Subtract", "pt-br"));
        Assert.Equal("resultado", _mapper.GetTranslation("result", "pt-br"));
        Assert.Equal("Digite um numero:", _mapper.GetLiteralTranslation("Enter a number:", "pt-br"));

        // Verify reverse lookups
        Assert.Equal("Calculator", _mapper.GetOriginal("Calculadora", "pt-br"));
        Assert.Equal("Add", _mapper.GetOriginal("Somar", "pt-br"));

        // Save and reload
        _mapper.SaveMap();

        var mapper2 = new IdentifierMapper();
        mapper2.LoadMap(_tempDir);
        Assert.Equal(4, mapper2.IdentifierCount);
        Assert.Equal(1, mapper2.LiteralCount);
        Assert.Equal("Calculadora", mapper2.GetTranslation("Calculator", "pt-br"));
    }
}
