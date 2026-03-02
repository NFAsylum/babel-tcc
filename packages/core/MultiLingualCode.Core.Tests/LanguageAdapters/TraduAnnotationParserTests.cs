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
public class Calculator // tradu[pt-br]:Calculadora
{
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("Calculator", annotations[0].OriginalIdentifier);
        Assert.Equal("Calculadora", annotations[0].TranslatedIdentifier);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
        Assert.False(annotations[0].IsLiteralAnnotation);
    }

    [Fact]
    public void ExtractAnnotations_MethodWithParams_ExtractsAllMappings()
    {
        string sourceCode = @"
public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
{
    return a + b;
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        TraduAnnotation annotation = annotations[0];
        Assert.Equal("Add", annotation.OriginalIdentifier);
        Assert.Equal("Somar", annotation.TranslatedIdentifier);
        Assert.Equal("pt-br", annotation.TargetLanguage);
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
string message = ""Total operations: ""; // tradu[pt-br]:""Total de operacoes: ""
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
public class Calculator // tradu[pt-br]:Calculadora
{
    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
    {
        return a + b;
    }
    public int operationCount; // tradu[pt-br]:contagemOperacoes
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

public class Calculator // tradu[pt-br]:Calculadora
{
    public int operationCount; // tradu[pt-br]:contagemOperacoes

    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }

    public int Subtract(int a, int b) // tradu[pt-br]:Subtrair,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a - b;
    }

    public string GetSummary() // tradu[pt-br]:ObterResumo
    {
        string label = ""Total operations: ""; // tradu[pt-br]:""Total de operacoes: ""
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

    [Fact]
    public void ExtractAnnotations_WhitespaceOnly_ReturnsEmptyList()
    {
        List<TraduAnnotation> annotations = _parser.ExtractAnnotations("   \n\t  ");

        Assert.Empty(annotations);
    }

    [Fact]
    public void ExtractAnnotations_CommentWithoutTradu_IgnoresComment()
    {
        string sourceCode = @"
class Program // This is not a tradu annotation
{
    int x = 5; // just a comment
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Empty(annotations);
    }

    [Fact]
    public void ExtractAnnotations_TraduWithExtraSpaces_ExtractsCorrectly()
    {
        string sourceCode = @"
class Calculator // tradu[pt-br]:Calculadora
{
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("Calculadora", annotations[0].TranslatedIdentifier);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
    }

    [Fact]
    public void ExtractAnnotations_FieldAnnotation_ExtractsMapping()
    {
        string sourceCode = @"
class Program
{
    int counter = 0; // tradu[pt-br]:contador
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("counter", annotations[0].OriginalIdentifier);
        Assert.Equal("contador", annotations[0].TranslatedIdentifier);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
    }

    [Fact]
    public void ParseAnnotationText_SingleParam_ExtractsMethodAndOneParam()
    {
        TraduAnnotation annotation = _parser.ParseAnnotationText("Executar,x:valor");

        Assert.Equal("Executar", annotation.TranslatedIdentifier);
        Assert.Single(annotation.ParameterMappings);
        Assert.Equal("x", annotation.ParameterMappings[0].OriginalParameterName);
        Assert.Equal("valor", annotation.ParameterMappings[0].TranslatedParameterName);
    }

    [Fact]
    public void ExtractAnnotations_MethodNoParams_ExtractsOnlyMethodName()
    {
        string sourceCode = @"
class Program
{
    void Run() // tradu[pt-br]:Executar
    {
    }
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("Run", annotations[0].OriginalIdentifier);
        Assert.Equal("Executar", annotations[0].TranslatedIdentifier);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
        Assert.Empty(annotations[0].ParameterMappings);
    }

    [Fact]
    public void ExtractAnnotations_PropertyAnnotation_ExtractsMapping()
    {
        string sourceCode = @"
class Person
{
    public string Name { get; set; } // tradu[pt-br]:Nome
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("Name", annotations[0].OriginalIdentifier);
        Assert.Equal("Nome", annotations[0].TranslatedIdentifier);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
    }

    [Fact]
    public void ExtractAnnotations_MultiLanguage_ExtractsMultipleAnnotations()
    {
        string sourceCode = @"
public class Calculator // tradu[pt-br]:Calculadora|[es]:Calculadora
{
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Equal(2, annotations.Count);

        TraduAnnotation ptAnnotation = annotations.First(a => a.TargetLanguage == "pt-br");
        Assert.Equal("Calculator", ptAnnotation.OriginalIdentifier);
        Assert.Equal("Calculadora", ptAnnotation.TranslatedIdentifier);

        TraduAnnotation esAnnotation = annotations.First(a => a.TargetLanguage == "es");
        Assert.Equal("Calculator", esAnnotation.OriginalIdentifier);
        Assert.Equal("Calculadora", esAnnotation.TranslatedIdentifier);
    }

    [Fact]
    public void ExtractAnnotations_MultiLanguageMethodWithParams_ExtractsAllSegments()
    {
        string sourceCode = @"
public class Calc
{
    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo|[es]:Sumar,a:primero,b:segundo
    {
        return a + b;
    }
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Equal(2, annotations.Count);

        TraduAnnotation ptAnnotation = annotations.First(a => a.TargetLanguage == "pt-br");
        Assert.Equal("Add", ptAnnotation.OriginalIdentifier);
        Assert.Equal("Somar", ptAnnotation.TranslatedIdentifier);
        Assert.Equal(2, ptAnnotation.ParameterMappings.Count);
        Assert.Equal("primeiro", ptAnnotation.ParameterMappings[0].TranslatedParameterName);
        Assert.Equal("segundo", ptAnnotation.ParameterMappings[1].TranslatedParameterName);

        TraduAnnotation esAnnotation = annotations.First(a => a.TargetLanguage == "es");
        Assert.Equal("Add", esAnnotation.OriginalIdentifier);
        Assert.Equal("Sumar", esAnnotation.TranslatedIdentifier);
        Assert.Equal("primero", esAnnotation.ParameterMappings[0].TranslatedParameterName);
        Assert.Equal("segundo", esAnnotation.ParameterMappings[1].TranslatedParameterName);
    }

    [Fact]
    public void ExtractAnnotations_MethodWithParams_SetsMethodRange()
    {
        string sourceCode = @"
public class Calc
{
    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
    {
        return a + b;
    }
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        TraduAnnotation annotation = annotations[0];
        Assert.True(annotation.MethodStartLine >= 0);
        Assert.True(annotation.MethodEndLine >= annotation.MethodStartLine);
    }

    [Fact]
    public void ExtractAnnotations_MultiLanguageLiteral_ExtractsMultipleAnnotations()
    {
        string sourceCode = @"
string msg = ""Hello""; // tradu[pt-br]:""Ola""|[es]:""Hola""
";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Equal(2, annotations.Count);

        TraduAnnotation ptAnnotation = annotations.First(a => a.TargetLanguage == "pt-br");
        Assert.True(ptAnnotation.IsLiteralAnnotation);
        Assert.Equal("Hello", ptAnnotation.OriginalLiteral);
        Assert.Equal("Ola", ptAnnotation.TranslatedLiteral);

        TraduAnnotation esAnnotation = annotations.First(a => a.TargetLanguage == "es");
        Assert.True(esAnnotation.IsLiteralAnnotation);
        Assert.Equal("Hello", esAnnotation.OriginalLiteral);
        Assert.Equal("Hola", esAnnotation.TranslatedLiteral);
    }

    [Fact]
    public void ParseAnnotationText_ExplicitIdentifierMapping_ExtractsTranslation()
    {
        TraduAnnotation annotation = _parser.ParseAnnotationText("a:prime");

        Assert.Equal("prime", annotation.TranslatedIdentifier);
        Assert.False(annotation.IsLiteralAnnotation);
        Assert.Empty(annotation.ParameterMappings);
    }

    [Fact]
    public void ExtractAnnotations_FieldWithExplicitMapping_ExtractsCorrectly()
    {
        string sourceCode = @"
public class Test
{
    public int a = 10; //tradu[pt-br]:a:prime
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("a", annotations[0].OriginalIdentifier);
        Assert.Equal("prime", annotations[0].TranslatedIdentifier);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
    }

    [Fact]
    public void ExtractAnnotations_SingleLanguagePrefix_ExtractsWithTargetLanguage()
    {
        string sourceCode = @"
public class Calculator // tradu[pt-br]:Calculadora
{
}";

        List<TraduAnnotation> annotations = _parser.ExtractAnnotations(sourceCode);

        Assert.Single(annotations);
        Assert.Equal("pt-br", annotations[0].TargetLanguage);
        Assert.Equal("Calculator", annotations[0].OriginalIdentifier);
        Assert.Equal("Calculadora", annotations[0].TranslatedIdentifier);
    }
}
