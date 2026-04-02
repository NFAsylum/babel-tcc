using MultiLingualCode.Core.LanguageAdapters.Python;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.LanguageAdapters;

public class PythonTokenizerServiceTests
{
    [RequiresPythonFact]
    public void Tokenize_SimpleCode_ReturnsTokens()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1");
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEmpty(result.Value);
    }

    [RequiresPythonFact]
    public void Tokenize_IdentifiesKeywords()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("def foo(): pass");
        Assert.True(result.IsSuccess);

        List<PythonToken> keywords = result.Value.FindAll(t => t.IsKeyword);
        Assert.Contains(keywords, t => t.String == "def");
        Assert.Contains(keywords, t => t.String == "pass");
    }

    [RequiresPythonFact]
    public void Tokenize_IdentifiesIdentifiers()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("def foo(x): return x");
        Assert.True(result.IsSuccess);

        List<PythonToken> identifiers = result.Value.FindAll(t => t.TypeName == "NAME" && !t.IsKeyword);
        Assert.Contains(identifiers, t => t.String == "foo");
        Assert.Contains(identifiers, t => t.String == "x");
    }

    [RequiresPythonFact]
    public void Tokenize_ReturnsCorrectPositions()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("def foo():");
        Assert.True(result.IsSuccess);

        PythonToken defToken = result.Value.First(t => t.String == "def");
        Assert.Equal(1, defToken.StartLine);
        Assert.Equal(0, defToken.StartCol);
        Assert.Equal(1, defToken.EndLine);
        Assert.Equal(3, defToken.EndCol);
    }

    [RequiresPythonFact]
    public void Tokenize_HandlesStringLiterals()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = \"hello\"");
        Assert.True(result.IsSuccess);

        List<PythonToken> strings = result.Value.FindAll(t => t.TypeName == "STRING");
        Assert.NotEmpty(strings);
    }

    [RequiresPythonFact]
    public void Tokenize_HandlesComments()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1 # comment");
        Assert.True(result.IsSuccess);

        List<PythonToken> comments = result.Value.FindAll(t => t.TypeName == "COMMENT");
        Assert.NotEmpty(comments);
    }

    [RequiresPythonFact]
    public void Tokenize_InvalidCode_ReturnsError()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = \"unterminated");
        Assert.False(result.IsSuccess);
        Assert.Contains("error", result.ErrorMessage.ToLower());
    }

    [RequiresPythonFact]
    public void Tokenize_MultipleRequests_ReuseProcess()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> r1 = service.Tokenize("x = 1");
        OperationResultGeneric<List<PythonToken>> r2 = service.Tokenize("y = 2");
        OperationResultGeneric<List<PythonToken>> r3 = service.Tokenize("z = 3");
        Assert.True(r1.IsSuccess);
        Assert.True(r2.IsSuccess);
        Assert.True(r3.IsSuccess);
    }

    [RequiresPythonFact]
    public void Tokenize_EmptyCode_ReturnsSuccess()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("");
        Assert.True(result.IsSuccess);
    }

    [RequiresPythonFact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        PythonTokenizerService service = new();
        service.Tokenize("x = 1");
        service.Dispose();
        service.Dispose();
    }

    [RequiresPythonFact]
    public void Tokenize_AfterDispose_ReturnsFail()
    {
        PythonTokenizerService service = new();
        service.Dispose();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1");
        Assert.False(result.IsSuccess);
    }
}
