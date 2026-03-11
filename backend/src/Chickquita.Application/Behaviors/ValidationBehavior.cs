using System.Reflection;
using Chickquita.Domain.Common;
using FluentValidation;
using MediatR;

namespace Chickquita.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before the handler is invoked.
///
/// When <typeparamref name="TResponse"/> is <see cref="Result{T}"/>, validation failures are
/// returned as <c>Result&lt;T&gt;.Failure(Error.Validation(...))</c> so callers receive a
/// consistent result object rather than a thrown exception.
///
/// For other response types a <see cref="ValidationException"/> is thrown, which is caught by
/// the global exception handler middleware in the API layer.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">All registered validators for <typeparamref name="TRequest"/>.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
        var error = Error.Validation(errorMessage);

        // When TResponse is Result<T>, return a failure result so callers get a consistent
        // Result object rather than a thrown exception. This preserves the pattern used
        // throughout the application where handlers return Result<T>.
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Use the implicit Error → Result<T> conversion operator
            var implicitOp = responseType.GetMethod(
                "op_Implicit",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(Error) },
                modifiers: null);

            if (implicitOp is not null)
            {
                return (TResponse)implicitOp.Invoke(null, new object[] { error })!;
            }
        }

        // Fallback: throw for non-Result response types (caught by the API exception middleware)
        throw new ValidationException(failures);
    }
}
