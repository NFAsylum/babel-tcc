using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Tests.Services;

public class IdentifierMapperTests : IDisposable
{
    public string TempDir;
    public IdentifierMapper Mapper = new();

    public IdentifierMapperTests()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"mapper_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
    }

    public void CreateMapFile(string json)
    {
        string dir = Path.Combine(TempDir, ".multilingual");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "identifier-map.json"), json);
    }

    [Fact]
    public void LoadMap_EmptyProject_StartsEmpty()
    {
        OperationResult result = Mapper.LoadMap(TempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, Mapper.IdentifierCount);
        Assert.Equal(0, Mapper.LiteralCount);
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

        OperationResult result = Mapper.LoadMap(TempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, Mapper.IdentifierCount);
        Assert.Equal(1, Mapper.LiteralCount);
    }

    [Fact]
    public void LoadMap_NullPath_ReturnsFailure()
    {
        OperationResult result = Mapper.LoadMap(null!);

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

        OperationResult result = await Mapper.LoadMapAsync(TempDir);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, Mapper.IdentifierCount);
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
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> ptResult = Mapper.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = Mapper.GetTranslation("GetName", "es-es");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
    }

    [Fact]
    public void GetTranslation_UnknownIdentifier_ReturnsFailure()
    {
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> result = Mapper.GetTranslation("Unknown", "pt-br");
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
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> result = Mapper.GetTranslation("GetName", "fr-fr");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetTranslation_EmptyOrNull_ReturnsFailure()
    {
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> emptyIdResult = Mapper.GetTranslation("", "pt-br");
        OperationResultGeneric<string> nullIdResult = Mapper.GetTranslation(null!, "pt-br");
        OperationResultGeneric<string> emptyLangResult = Mapper.GetTranslation("GetName", "");
        OperationResultGeneric<string> nullLangResult = Mapper.GetTranslation("GetName", null!);

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
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> getNameResult = Mapper.GetOriginal("ObterNome", "pt-br");
        OperationResultGeneric<string> setValueResult = Mapper.GetOriginal("DefinirValor", "pt-br");

        Assert.True(getNameResult.IsSuccess);
        Assert.Equal("GetName", getNameResult.Value);
        Assert.True(setValueResult.IsSuccess);
        Assert.Equal("SetValue", setValueResult.Value);
    }

    [Fact]
    public void GetOriginal_UnknownTranslation_ReturnsFailure()
    {
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> result = Mapper.GetOriginal("Desconhecido", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetOriginal_EmptyOrNull_ReturnsFailure()
    {
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> emptyResult = Mapper.GetOriginal("", "pt-br");
        OperationResultGeneric<string> nullResult = Mapper.GetOriginal(null!, "pt-br");
        OperationResultGeneric<string> emptyLangResult = Mapper.GetOriginal("ObterNome", "");

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
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> calcResult = Mapper.GetLiteralTranslation("Calculator", "pt-br");
        OperationResultGeneric<string> helloResult = Mapper.GetLiteralTranslation("Hello World", "es-es");

        Assert.True(calcResult.IsSuccess);
        Assert.Equal("Calculadora", calcResult.Value);
        Assert.True(helloResult.IsSuccess);
        Assert.Equal("Hola Mundo", helloResult.Value);
    }

    [Fact]
    public void GetLiteralTranslation_UnknownLiteral_ReturnsFailure()
    {
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> result = Mapper.GetLiteralTranslation("Unknown", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetLiteralTranslation_EmptyOrNull_ReturnsFailure()
    {
        Mapper.LoadMap(TempDir);

        OperationResultGeneric<string> emptyResult = Mapper.GetLiteralTranslation("", "pt-br");
        OperationResultGeneric<string> nullResult = Mapper.GetLiteralTranslation(null!, "pt-br");

        Assert.False(emptyResult.IsSuccess);
        Assert.False(nullResult.IsSuccess);
    }

    [Fact]
    public void SetTranslation_AddsNewMapping()
    {
        Mapper.LoadMap(TempDir);

        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        OperationResultGeneric<string> result = Mapper.GetTranslation("GetName", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("ObterNome", result.Value);
        Assert.Equal(1, Mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_MultipleLanguages()
    {
        Mapper.LoadMap(TempDir);

        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        Mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");

        OperationResultGeneric<string> ptResult = Mapper.GetTranslation("GetName", "pt-br");
        OperationResultGeneric<string> esResult = Mapper.GetTranslation("GetName", "es-es");

        Assert.True(ptResult.IsSuccess);
        Assert.Equal("ObterNome", ptResult.Value);
        Assert.True(esResult.IsSuccess);
        Assert.Equal("ObtenerNombre", esResult.Value);
        Assert.Equal(1, Mapper.IdentifierCount);
    }

    [Fact]
    public void SetTranslation_OverwritesExisting()
    {
        Mapper.LoadMap(TempDir);

        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        Mapper.SetTranslation("GetName", "pt-br", "PegarNome");

        OperationResultGeneric<string> result = Mapper.GetTranslation("GetName", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("PegarNome", result.Value);
    }

    [Fact]
    public void SetTranslation_DoesNotThrowOnNull()
    {
        Mapper.LoadMap(TempDir);

        // SetTranslation no longer throws - it just silently handles nulls
        Mapper.SetTranslation(null!, "pt-br", "x");
        Mapper.SetTranslation("x", null!, "x");
        Mapper.SetTranslation("x", "pt-br", null!);
    }

    [Fact]
    public void SetLiteralTranslation_AddsNewMapping()
    {
        Mapper.LoadMap(TempDir);

        Mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        OperationResultGeneric<string> result = Mapper.GetLiteralTranslation("Calculator", "pt-br");
        Assert.True(result.IsSuccess);
        Assert.Equal("Calculadora", result.Value);
        Assert.Equal(1, Mapper.LiteralCount);
    }

    [Fact]
    public void RemoveIdentifier_ExistingMapping_ReturnsTrue()
    {
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        Assert.True(Mapper.RemoveIdentifier("GetName"));
        OperationResultGeneric<string> result = Mapper.GetTranslation("GetName", "pt-br");
        Assert.False(result.IsSuccess);
        Assert.Equal(0, Mapper.IdentifierCount);
    }

    [Fact]
    public void RemoveIdentifier_NonExistent_ReturnsFalse()
    {
        Mapper.LoadMap(TempDir);

        Assert.False(Mapper.RemoveIdentifier("NonExistent"));
    }

    [Fact]
    public void SaveMap_CreatesFileAndDirectory()
    {
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        Mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");

        OperationResult saveResult = Mapper.SaveMap();

        Assert.True(saveResult.IsSuccess);
        string filePath = Path.Combine(TempDir, ".multilingual", "identifier-map.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void SaveMap_RoundTrip()
    {
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        Mapper.SetTranslation("GetName", "es-es", "ObtenerNombre");
        Mapper.SetLiteralTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SaveMap();

        // Load in a new mapper
        IdentifierMapper mapper2 = new IdentifierMapper();
        mapper2.LoadMap(TempDir);

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
        string otherDir = Path.Combine(TempDir, "other-project");
        Directory.CreateDirectory(otherDir);

        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");
        OperationResult saveResult = Mapper.SaveMap(otherDir);

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
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("GetName", "pt-br", "ObterNome");

        OperationResultGeneric<string> result = Mapper.GetOriginal("obternome", "pt-br");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CalculatorExample_WithFullWorkflow_TranslatesAndReversesAllMappings()
    {
        Mapper.LoadMap(TempDir);

        // Set up Calculator project mappings
        Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SetTranslation("Add", "pt-br", "Somar");
        Mapper.SetTranslation("Subtract", "pt-br", "Subtrair");
        Mapper.SetTranslation("result", "pt-br", "resultado");
        Mapper.SetLiteralTranslation("Enter a number:", "pt-br", "Digite um numero:");

        // Verify forward lookups
        OperationResultGeneric<string> calcResult = Mapper.GetTranslation("Calculator", "pt-br");
        OperationResultGeneric<string> addResult = Mapper.GetTranslation("Add", "pt-br");
        OperationResultGeneric<string> subResult = Mapper.GetTranslation("Subtract", "pt-br");
        OperationResultGeneric<string> resResult = Mapper.GetTranslation("result", "pt-br");
        OperationResultGeneric<string> litResult = Mapper.GetLiteralTranslation("Enter a number:", "pt-br");

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
        OperationResultGeneric<string> revCalcResult = Mapper.GetOriginal("Calculadora", "pt-br");
        OperationResultGeneric<string> revAddResult = Mapper.GetOriginal("Somar", "pt-br");

        Assert.True(revCalcResult.IsSuccess);
        Assert.Equal("Calculator", revCalcResult.Value);
        Assert.True(revAddResult.IsSuccess);
        Assert.Equal("Add", revAddResult.Value);

        // Save and reload
        Mapper.SaveMap();

        IdentifierMapper mapper2 = new IdentifierMapper();
        mapper2.LoadMap(TempDir);
        Assert.Equal(4, mapper2.IdentifierCount);
        Assert.Equal(1, mapper2.LiteralCount);
        OperationResultGeneric<string> reloadResult = mapper2.GetTranslation("Calculator", "pt-br");
        Assert.True(reloadResult.IsSuccess);
        Assert.Equal("Calculadora", reloadResult.Value);
    }

    [Fact]
    public void MultiLanguage_SetTranslationsFromDifferentFiles_AccumulatesInMemory()
    {
        Mapper.LoadMap(TempDir);

        Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SetTranslation("Add", "pt-br", "Somar");
        Mapper.SetTranslation("task_manager", "pt-br", "gerenciador_tarefas");
        Mapper.SetTranslation("add_task", "pt-br", "adicionar_tarefa");

        Assert.Equal(4, Mapper.IdentifierCount);
        Assert.True(Mapper.GetTranslation("Calculator", "pt-br").IsSuccess);
        Assert.True(Mapper.GetTranslation("task_manager", "pt-br").IsSuccess);
    }

    [Fact]
    public void MultiLanguage_SaveAndReload_PreservesAllIdentifiers()
    {
        Mapper.LoadMap(TempDir);

        Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SetTranslation("task_manager", "pt-br", "gerenciador_tarefas");
        Mapper.SaveMap();

        IdentifierMapper mapper2 = new();
        mapper2.LoadMap(TempDir);

        Assert.Equal(2, mapper2.IdentifierCount);
        Assert.Equal("Calculadora", mapper2.GetTranslation("Calculator", "pt-br").Value);
        Assert.Equal("gerenciador_tarefas", mapper2.GetTranslation("task_manager", "pt-br").Value);
    }

    [Fact]
    public void MultiLanguage_SequentialSaves_PreservePreviousData()
    {
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SetTranslation("Add", "pt-br", "Somar");
        Mapper.SaveMap();

        IdentifierMapper mapper2 = new();
        mapper2.LoadMap(TempDir);
        mapper2.SetTranslation("task_manager", "pt-br", "gerenciador_tarefas");
        mapper2.SaveMap();

        IdentifierMapper mapper3 = new();
        mapper3.LoadMap(TempDir);

        Assert.Equal(3, mapper3.IdentifierCount);
        Assert.Equal("Calculadora", mapper3.GetTranslation("Calculator", "pt-br").Value);
        Assert.Equal("Somar", mapper3.GetTranslation("Add", "pt-br").Value);
        Assert.Equal("gerenciador_tarefas", mapper3.GetTranslation("task_manager", "pt-br").Value);
    }

    [Fact]
    public void MultiLanguage_RoundTrip_TranslateAndReverse()
    {
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SetTranslation("task_manager", "pt-br", "gerenciador_tarefas");
        Mapper.SaveMap();

        IdentifierMapper mapper2 = new();
        mapper2.LoadMap(TempDir);

        Assert.Equal("Calculator", mapper2.GetOriginal("Calculadora", "pt-br").Value);
        Assert.Equal("task_manager", mapper2.GetOriginal("gerenciador_tarefas", "pt-br").Value);
    }

    [Fact]
    public void MultiLanguage_SameIdentifierDifferentLanguages_CoexistCorrectly()
    {
        Mapper.LoadMap(TempDir);
        Mapper.SetTranslation("Calculator", "pt-br", "Calculadora");
        Mapper.SetTranslation("Calculator", "es-es", "Calculadora_ES");
        Mapper.SetTranslation("task_manager", "pt-br", "gerenciador_tarefas");
        Mapper.SaveMap();

        IdentifierMapper mapper2 = new();
        mapper2.LoadMap(TempDir);

        Assert.Equal("Calculadora", mapper2.GetTranslation("Calculator", "pt-br").Value);
        Assert.Equal("Calculadora_ES", mapper2.GetTranslation("Calculator", "es-es").Value);
        Assert.Equal("gerenciador_tarefas", mapper2.GetTranslation("task_manager", "pt-br").Value);
    }
}
