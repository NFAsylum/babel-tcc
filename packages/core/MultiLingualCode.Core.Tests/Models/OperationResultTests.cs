using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Tests.Models;

public class OperationResultTests
{
    [Fact]
    public void Ok_WhenCalled_ReturnsSuccessWithEmptyError()
    {
        OperationResult result = OperationResult.Ok();

        Assert.True(result.IsSuccess);
        Assert.Equal("", result.ErrorMessage);
    }

    [Fact]
    public void Fail_WithMessage_ReturnsFailureWithErrorMessage()
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
    public void GenericOk_WithStringValue_ReturnsValueAndSuccess()
    {
        OperationResultGeneric<string> result = OperationResultGeneric<string>.Ok("hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
        Assert.Equal("", result.ErrorMessage);
    }

    [Fact]
    public void GenericOk_IntValue_ReturnsCorrectValue()
    {
        OperationResultGeneric<int> result = OperationResultGeneric<int>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFail_WithErrorMessage_ReturnsFailureWithDefaultValue()
    {
        OperationResultGeneric<string> result = OperationResultGeneric<string>.Fail("error");

        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public void GenericFail_IntType_ReturnsDefaultInt()
    {
        OperationResultGeneric<int> result = OperationResultGeneric<int>.Fail("error");

        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void StaticOkGeneric_WithStringValue_CreatesTypedSuccessResult()
    {
        OperationResultGeneric<string> result = OperationResult.Ok<string>("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void StaticFailGeneric_WithErrorMessage_CreatesTypedFailureResult()
    {
        OperationResultGeneric<string> result = OperationResult.Fail<string>("error");

        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public void GenericOk_NullValue_StillSuccess()
    {
        OperationResultGeneric<string> result = OperationResultGeneric<string>.Ok(null!);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GenericResult_WhenCastToBase_PreservesSuccessStatus()
    {
        OperationResultGeneric<string> typed = OperationResultGeneric<string>.Ok("value");
        OperationResult baseResult = typed;

        Assert.True(baseResult.IsSuccess);
    }
}
