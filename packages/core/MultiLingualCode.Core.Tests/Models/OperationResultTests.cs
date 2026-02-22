using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Models;

public class OperationResultTests
{
    [Fact]
    public void Ok_ReturnsSuccess()
    {
        OperationResult result = OperationResult.Ok();

        Assert.True(result.IsSuccess);
        Assert.Equal("", result.ErrorMessage);
    }

    [Fact]
    public void Fail_ReturnsFailure()
    {
        OperationResult result = OperationResult.Fail("something went wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal("something went wrong", result.ErrorMessage);
    }

    [Fact]
    public void Fail_EmptyMessage_ReturnsFailure()
    {
        OperationResult result = OperationResult.Fail("");

        Assert.False(result.IsSuccess);
        Assert.Equal("", result.ErrorMessage);
    }

    [Fact]
    public void GenericOk_ReturnsValueAndSuccess()
    {
        OperationResult<string> result = OperationResult<string>.Ok("hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
        Assert.Equal("", result.ErrorMessage);
    }

    [Fact]
    public void GenericOk_IntValue_ReturnsCorrectValue()
    {
        OperationResult<int> result = OperationResult<int>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFail_ReturnsFailureWithDefaultValue()
    {
        OperationResult<string> result = OperationResult<string>.Fail("error");

        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public void GenericFail_IntType_ReturnsDefaultInt()
    {
        OperationResult<int> result = OperationResult<int>.Fail("error");

        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void StaticOkGeneric_CreatesTypedResult()
    {
        OperationResult<string> result = OperationResult.Ok<string>("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void StaticFailGeneric_CreatesTypedResult()
    {
        OperationResult<string> result = OperationResult.Fail<string>("error");

        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public void GenericOk_NullValue_StillSuccess()
    {
        OperationResult<string> result = OperationResult<string>.Ok(null!);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GenericResult_IsAlsoBaseResult()
    {
        OperationResult<string> typed = OperationResult<string>.Ok("value");
        OperationResult baseResult = typed;

        Assert.True(baseResult.IsSuccess);
    }
}
