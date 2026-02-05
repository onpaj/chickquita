namespace ChickenTrack.Domain.Common;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public class Result<T>
{
    private readonly T? _value;

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the result (only valid if IsFailure is true).
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Gets the value associated with the result (only valid if IsSuccess is true).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value of a failed result.</exception>
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            }

            return _value!;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class with a success state.
    /// </summary>
    /// <param name="value">The success value.</param>
    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        Error = Error.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class with a failure state.
    /// </summary>
    /// <param name="error">The error.</param>
    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        Error = error;
    }

    /// <summary>
    /// Creates a success result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A success result.</returns>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failure result.</returns>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Implicitly converts a value to a success result.
    /// </summary>
    /// <param name="value">The value.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a failure result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that can either succeed or fail with an error (without a value).
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the result (only valid if IsFailure is true).
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with a success state.
    /// </summary>
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <returns>A success result.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failure result.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Implicitly converts an error to a failure result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => Failure(error);
}
