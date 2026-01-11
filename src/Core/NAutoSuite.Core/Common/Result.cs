namespace NAutoSuite.Core.Common;

/// <summary>
/// Represents the result of an operation
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Message { get; protected set; }
    public Exception? Exception { get; protected set; }

    protected Result(bool isSuccess, string message, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Exception = exception;
    }

    public static Result Success(string message = "Operation completed successfully")
        => new(true, message);

    public static Result Failure(string message, Exception? exception = null)
        => new(false, message, exception);

    public static Result<T> Success<T>(T value, string message = "Operation completed successfully")
        => new(true, value, message);

    public static Result<T> Failure<T>(string message, Exception? exception = null)
        => new(false, default!, message, exception);
}

/// <summary>
/// Generic result with value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private set; }

    internal Result(bool isSuccess, T? value, string message, Exception? exception = null)
        : base(isSuccess, message, exception)
    {
        Value = value;
    }
}
