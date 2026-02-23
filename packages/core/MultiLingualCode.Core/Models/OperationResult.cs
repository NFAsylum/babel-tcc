namespace MultiLingualCode.Core.Models;

public class OperationResult
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    public static OperationResult Ok()
    {
        return new OperationResult(true, "");
    }

    public static OperationResult Fail(string errorMessage)
    {
        return new OperationResult(false, errorMessage);
    }

    public static OperationResultGeneric<T> Ok<T>(T value)
    {
        return OperationResultGeneric<T>.Ok(value);
    }

    public static OperationResultGeneric<T> Fail<T>(string errorMessage)
    {
        return OperationResultGeneric<T>.Fail(errorMessage);
    }

    protected OperationResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}
