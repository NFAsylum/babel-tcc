using MultiLingualCode.Core.LanguageAdapters;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class TraduAnnotationParserTests
{
    public TraduAnnotationParser _parser = new TraduAnnotationParser();

    [Fact]
    public void ExtractAnnotations_SimpleAnnotation_ExtractsIdentifierMapping()
    {
        string sourceCode = @"
public class Calculator // tradu:Calculadora
{
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("Calculator", annotations[0].OriginalIdentifier);
        Assert.Equal("Calculadora", annotations[0].TranslatedIdentifier);
        Assert.False(annotations[0].IsLiteralAnnotation);
    }

    [Fact]
    public void ExtractAnnotations_MethodWithParams_ExtractsAllMappings()
    {
        string sourceCode = @"
public int Add(int a, int b) // tradu:Somar,a:primeiroNumero,b:segundoNumero
{
    return a + b;
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        TraduAnnotation annotation = annotations[0];
        Assert.Equal("Add", annotation.OriginalIdentifier);
        Assert.Equal("Somar", annotation.TranslatedIdentifier);
        Assert.Equal(2, annotation.ParameterMappings.Count);
        Assert.Equal("a", annotation.ParameterMappings[0].OriginalParameterName);
        Assert.Equal("primeiroNumero", annotation.ParameterMappings[0].TranslatedParameterName);
        Assert.Equal("b", annotation.ParameterMappings[1].OriginalParameterName);
        Assert.Equal("segundoNumero", annotation.ParameterMappings[1].TranslatedParameterName);
    }

    [Fact]
    public void ExtractAnnotations_LiteralAnnotation_ExtractsLiteralMapping()
    {
        string sourceCode = @"
string message = ""Total operations: ""; // tradu:""Total de operacoes: ""
";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        TraduAnnotation annotation = annotations[0];
        Assert.True(annotation.IsLiteralAnnotation);
        Assert.Equal("Total operations: ", annotation.OriginalLiteral);
        Assert.Equal("Total de operacoes: ", annotation.TranslatedLiteral);
    }

    [Fact]
    public void ExtractAnnotations_NoAnnotations_ReturnsEmptyList()
    {
        string sourceCode = @"
public class Program
{
    // This is a regular comment
    public void Main() { }
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Empty(annotations);
    }

    [Fact]
    public void ExtractAnnotations_MultipleAnnotations_ExtractsAll()
    {
        string sourceCode = @"
public class Calculator // tradu:Calculadora
{
    public int Add(int a, int b) // tradu:Somar,a:primeiro,b:segundo
    {
        return a + b;
    }
    public int operationCount; // tradu:contagemOperacoes
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Equal(3, annotations.Count);

        TraduAnnotation classAnnotation = annotations.First(a => a.TranslatedIdentifier == "Calculadora");
        Assert.Equal("Calculator", classAnnotation.OriginalIdentifier);

        TraduAnnotation methodAnnotation = annotations.First(a => a.TranslatedIdentifier == "Somar");
        Assert.Equal("Add", methodAnnotation.OriginalIdentifier);
        Assert.Equal(2, methodAnnotation.ParameterMappings.Count);

        TraduAnnotation fieldAnnotation = annotations.First(a => a.TranslatedIdentifier == "contagemOperacoes");
        Assert.Equal("operationCount", fieldAnnotation.OriginalIdentifier);
    }

    [Fact]
    public void ExtractAnnotations_CalculatorExample_ExtractsAllMappings()
    {
        string sourceCode = @"
using System;

public class Calculator // tradu:Calculadora
{
    public int operationCount; // tradu:contagemOperacoes

    public int Add(int a, int b) // tradu:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }

    public int Subtract(int a, int b) // tradu:Subtrair,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a - b;
    }

    public string GetSummary() // tradu:ObterResumo
    {
        string label = ""Total operations: ""; // tradu:""Total de operacoes: ""
        return label + operationCount;
    }
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Equal(6, annotations.Count);

        TraduAnnotation classAnnotation = annotations.First(a => a.TranslatedIdentifier == "Calculadora");
        Assert.Equal("Calculator", classAnnotation.OriginalIdentifier);

        TraduAnnotation fieldAnnotation = annotations.First(a => a.TranslatedIdentifier == "contagemOperacoes");
        Assert.Equal("operationCount", fieldAnnotation.OriginalIdentifier);

        TraduAnnotation addAnnotation = annotations.First(a => a.TranslatedIdentifier == "Somar");
        Assert.Equal("Add", addAnnotation.OriginalIdentifier);
        Assert.Equal(2, addAnnotation.ParameterMappings.Count);

        TraduAnnotation subtractAnnotation = annotations.First(a => a.TranslatedIdentifier == "Subtrair");
        Assert.Equal("Subtract", subtractAnnotation.OriginalIdentifier);
        Assert.Equal(2, subtractAnnotation.ParameterMappings.Count);

        TraduAnnotation summaryAnnotation = annotations.First(a => a.TranslatedIdentifier == "ObterResumo");
        Assert.Equal("GetSummary", summaryAnnotation.OriginalIdentifier);

        TraduAnnotation literalAnnotation = annotations.First(a => a.IsLiteralAnnotation);
        Assert.Equal("Total operations: ", literalAnnotation.OriginalLiteral);
        Assert.Equal("Total de operacoes: ", literalAnnotation.TranslatedLiteral);
    }

    [Fact]
    public void ExtractAnnotations_EmptySource_ReturnsEmptyList()
    {
        List<TraduAnnotation> annotations = _parser.ExtractAnnotations("");

        Assert.Empty(annotations);
    }

    [Fact]
    public void ParseAnnotationText_SimpleFormat_ReturnsTranslation()
    {
        TraduAnnotation annotation = _parser.ParseAnnotationText("Calculadora");

        Assert.Equal("Calculadora", annotation.TranslatedIdentifier);
        Assert.False(annotation.IsLiteralAnnotation);
        Assert.Empty(annotation.ParameterMappings);
    }

    [Fact]
    public void ParseAnnotationText_MethodFormat_ReturnsMethodAndParams()
    {
        TraduAnnotation annotation = _parser.ParseAnnotationText("Somar,a:primeiroNumero,b:segundoNumero");

        Assert.Equal("Somar", annotation.TranslatedIdentifier);
        Assert.Equal(2, annotation.ParameterMappings.Count);
        Assert.Equal("a", annotation.ParameterMappings[0].OriginalParameterName);
        Assert.Equal("primeiroNumero", annotation.ParameterMappings[0].TranslatedParameterName);
        Assert.Equal("b", annotation.ParameterMappings[1].OriginalParameterName);
        Assert.Equal("segundoNumero", annotation.ParameterMappings[1].TranslatedParameterName);
    }

    [Fact]
    public void ParseAnnotationText_LiteralFormat_ReturnsLiteral()
    {
        TraduAnnotation annotation = _parser.ParseAnnotationText("\"Total de operacoes: \"");

        Assert.True(annotation.IsLiteralAnnotation);
        Assert.Equal("Total de operacoes: ", annotation.TranslatedLiteral);
    }

    [Fact]
    public void ParseAnnotationText_EmptyText_ReturnsEmptyAnnotation()
    {
        TraduAnnotation annotation = _parser.ParseAnnotationText("");

        Assert.Equal(string.Empty, annotation.TranslatedIdentifier);
        Assert.False(annotation.IsLiteralAnnotation);
        Assert.Empty(annotation.ParameterMappings);
    }
}
