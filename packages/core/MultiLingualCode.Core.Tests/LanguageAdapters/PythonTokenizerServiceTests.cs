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
    public void Tokenize_WithCodeContainingKeywords_IdentifiesKeywordTokens()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("def foo(): pass");
        Assert.True(result.IsSuccess);

        List<PythonToken> keywords = result.Value.FindAll(t => t.IsKeyword);
        Assert.Contains(keywords, t => t.String == "def");
        Assert.Contains(keywords, t => t.String == "pass");
    }

    [RequiresPythonFact]
    public void Tokenize_WithCodeContainingIdentifiers_IdentifiesNameTokens()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("def foo(x): return x");
        Assert.True(result.IsSuccess);

        List<PythonToken> identifiers = result.Value.FindAll(t => t.TypeName == "NAME" && !t.IsKeyword);
        Assert.Contains(identifiers, t => t.String == "foo");
        Assert.Contains(identifiers, t => t.String == "x");
    }

    [RequiresPythonFact]
    public void Tokenize_WithSimpleCode_ReturnsCorrectTokenPositions()
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
    public void Tokenize_WithStringLiteral_ReturnsStringToken()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = \"hello\"");
        Assert.True(result.IsSuccess);

        List<PythonToken> strings = result.Value.FindAll(t => t.TypeName == "STRING");
        Assert.NotEmpty(strings);
    }

    [RequiresPythonFact]
    public void Tokenize_WithComment_ReturnsCommentToken()
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
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
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

    [RequiresPythonFact]
    public void Dispose_WhenCalled_StopsPythonProcess()
    {
        PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1");
        Assert.True(result.IsSuccess);

        int pid = service.Process.Id;

        service.Dispose();

        try
        {
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(pid);
            Assert.True(proc.HasExited);
        }
        catch (ArgumentException)
        {
            // Process no longer exists — OK, means it was stopped
        }
    }

    [RequiresPythonFact]
    public void Dispose_AfterSequentialCreateCycles_DoesNotAccumulateProcesses()
    {
        List<int> processIds = new();

        for (int i = 0; i < 3; i++)
        {
            PythonTokenizerService service = new();
            OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1");
            Assert.True(result.IsSuccess);
            processIds.Add(service.Process.Id);
            service.Dispose();
        }

        foreach (int pid in processIds)
        {
            try
            {
                System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(pid);
                Assert.True(proc.HasExited);
            }
            catch (ArgumentException)
            {
                // Process no longer exists — OK
            }
        }
    }

    [RequiresPythonFact]
    public void ProcessCrash_NextTokenize_RestartsProcess()
    {
        using PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result1 = service.Tokenize("x = 1");
        Assert.True(result1.IsSuccess);

        int originalPid = service.Process.Id;

        service.Process.Kill();
        service.Process.WaitForExit(2000);

        OperationResultGeneric<List<PythonToken>> result2 = service.Tokenize("y = 2");
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(originalPid, service.Process.Id);
    }

    [RequiresPythonFact]
    public void Tokenize_AfterProcessCrash_DoesNotLeaveOrphanProcess()
    {
        PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1");
        Assert.True(result.IsSuccess);

        int originalPid = service.Process.Id;

        service.Process.Kill();
        service.Process.WaitForExit(2000);

        service.Tokenize("y = 2");

        try
        {
            System.Diagnostics.Process oldProc = System.Diagnostics.Process.GetProcessById(originalPid);
            Assert.True(oldProc.HasExited);
        }
        catch (ArgumentException)
        {
            // Process no longer exists — OK
        }

        service.Dispose();
    }

    [RequiresPythonFact]
    public void NoDispose_ProcessRemainsAlive_LeakDetectable()
    {
        PythonTokenizerService service = new();
        OperationResultGeneric<List<PythonToken>> result = service.Tokenize("x = 1");
        Assert.True(result.IsSuccess);

        int pid = service.Process.Id;

        // Intentionally NOT calling Dispose — process should still be alive
        try
        {
            System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(pid);
            Assert.False(proc.HasExited, "Process should still be alive without Dispose");
        }
        catch (ArgumentException)
        {
            Assert.Fail("Process should still be alive without Dispose, but it was already gone");
        }

        // Manual cleanup to not leak in test
        service.Dispose();
    }
}
