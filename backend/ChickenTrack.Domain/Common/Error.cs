namespace ChickenTrack.Domain.Common;

/// <summary>
/// Represents an error with a code and message.
/// Used in the Result pattern to represent failure states.
/// </summary>
public sealed class Error
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Represents a null error (no error).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    /// <summary>
    /// Creates a new validation error.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    /// <returns>A new validation error.</returns>
    public static Error Validation(string message) => new("Error.Validation", message);

    /// <summary>
    /// Creates a new not found error.
    /// </summary>
    /// <param name="message">The not found error message.</param>
    /// <returns>A new not found error.</returns>
    public static Error NotFound(string message) => new("Error.NotFound", message);

    /// <summary>
    /// Creates a new conflict error.
    /// </summary>
    /// <param name="message">The conflict error message.</param>
    /// <returns>A new conflict error.</returns>
    public static Error Conflict(string message) => new("Error.Conflict", message);

    /// <summary>
    /// Creates a new failure error.
    /// </summary>
    /// <param name="message">The failure error message.</param>
    /// <returns>A new failure error.</returns>
    public static Error Failure(string message) => new("Error.Failure", message);

    /// <summary>
    /// Creates a new unauthorized error.
    /// </summary>
    /// <param name="message">The unauthorized error message.</param>
    /// <returns>A new unauthorized error.</returns>
    public static Error Unauthorized(string message) => new("Error.Unauthorized", message);
}
