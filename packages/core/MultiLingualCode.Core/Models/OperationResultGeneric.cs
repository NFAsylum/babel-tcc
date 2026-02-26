namespace MultiLingualCode.Core.Models;

/// <summary>
/// Represents the result of an operation that returns a value of type <typeparamref name="T"/>, encapsulating success or failure.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class OperationResultGeneric<T> : OperationResult
{
    /// <summary>
    /// Gets the value produced by the operation. Only meaningful when <see cref="OperationResult.IsSuccess"/> is true.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The value to wrap in the result.</param>
    /// <returns>A successful <see cref="OperationResultGeneric{T}"/>.</returns>
    public static OperationResultGeneric<T> Ok(T value)
    {
        return new OperationResultGeneric<T>(true, "", value);
    }

    /// <summary>
    /// Creates a failed result with the specified error message and no value.
    /// </summary>
    /// <param name="errorMessage">A message describing why the operation failed.</param>
    /// <returns>A failed <see cref="OperationResultGeneric{T}"/>.</returns>
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
