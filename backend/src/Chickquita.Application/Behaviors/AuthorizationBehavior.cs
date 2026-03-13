using System.Reflection;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces authentication and tenant resolution
/// for all requests implementing <see cref="IAuthorizedRequest"/>.
///
/// When <typeparamref name="TResponse"/> is <see cref="Result{T}"/>, failures are returned as
/// <c>Result&lt;T&gt;.Failure(Error.Unauthorized(...))</c> so callers receive a consistent result
/// object rather than a thrown exception.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="currentUserService">Service for resolving the current user and tenant context.</param>
    public AuthorizationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only enforce auth for requests that explicitly opt in via IAuthorizedRequest
        if (request is not IAuthorizedRequest)
        {
            return await next();
        }

        if (!_currentUserService.IsAuthenticated)
        {
            return WrapFailure(Error.Unauthorized("User is not authenticated"));
        }

        if (!_currentUserService.TenantId.HasValue)
        {
            return WrapFailure(Error.Unauthorized("Tenant not found"));
        }

        return await next();
    }

    private static TResponse WrapFailure(Error error)
    {
        // When TResponse is Result<T>, use the implicit Error → Result<T> conversion operator
        // so callers receive a consistent Result object rather than a thrown exception.
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
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

        throw new UnauthorizedAccessException(error.Message);
    }
}
