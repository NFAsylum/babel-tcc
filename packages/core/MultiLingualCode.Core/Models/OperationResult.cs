namespace MultiLingualCode.Core.Models;

/// <summary>
/// Represents the result of an operation that does not return a value, encapsulating success or failure with an error message.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message describing the failure reason. Empty string when the operation succeeded.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Creates a successful result with no error message.
    /// </summary>
    /// <returns>A successful <see cref="OperationResult"/>.</returns>
    public static OperationResult Ok()
    {
        return new OperationResult(true, "");
    }

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">A message describing why the operation failed.</param>
    /// <returns>A failed <see cref="OperationResult"/>.</returns>
    public static OperationResult Fail(string errorMessage)
    {
        return new OperationResult(false, errorMessage);
    }

    /// <summary>
    /// Creates a successful generic result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="value">The value to wrap in the result.</param>
    /// <returns>A successful <see cref="OperationResultGeneric{T}"/>.</returns>
    public static OperationResultGeneric<T> Ok<T>(T value)
    {
        return OperationResultGeneric<T>.Ok(value);
    }

    /// <summary>
    /// Creates a failed generic result with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="errorMessage">A message describing why the operation failed.</param>
    /// <returns>A failed <see cref="OperationResultGeneric{T}"/>.</returns>
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
