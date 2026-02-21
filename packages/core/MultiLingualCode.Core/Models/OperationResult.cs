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

    public static OperationResult<T> Ok<T>(T value)
    {
        return OperationResult<T>.Ok(value);
    }

    public static OperationResult<T> Fail<T>(string errorMessage)
    {
        return OperationResult<T>.Fail(errorMessage);
    }

    protected OperationResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}

public class OperationResult<T> : OperationResult
{
    public T Value { get; }

    public static OperationResult<T> Ok(T value)
    {
        return new OperationResult<T>(true, "", value);
    }

    public new static OperationResult<T> Fail(string errorMessage)
    {
        return new OperationResult<T>(false, errorMessage, default!);
    }

    protected OperationResult(bool isSuccess, string errorMessage, T value)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }
}
