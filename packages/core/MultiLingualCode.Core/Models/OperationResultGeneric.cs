namespace MultiLingualCode.Core.Models;

public class OperationResultGeneric<T> : OperationResult
{
    public T Value { get; }

    public static OperationResultGeneric<T> Ok(T value)
    {
        return new OperationResultGeneric<T>(true, "", value);
    }

    public new static OperationResultGeneric<T> Fail(string errorMessage)
    {
        return new OperationResultGeneric<T>(false, errorMessage, default!);
    }

    protected OperationResultGeneric(bool isSuccess, string errorMessage, T value)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }
}
