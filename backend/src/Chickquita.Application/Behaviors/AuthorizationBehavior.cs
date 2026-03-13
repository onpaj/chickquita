using System.Reflection;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces authentication and tenant checks for all requests
/// except those implementing <see cref="IAnonymousRequest"/>.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="currentUserService">The current user service.</param>
    public AuthorizationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Skip auth checks for anonymous requests (e.g., webhook handlers)
        if (request is IAnonymousRequest)
            return await next();

        if (!_currentUserService.IsAuthenticated)
            return CreateFailure(Error.Unauthorized("User is not authenticated"));

        if (!_currentUserService.TenantId.HasValue)
            return CreateFailure(Error.Unauthorized("Tenant not found"));

        return await next();
    }

    private static TResponse CreateFailure(Error error)
    {
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var implicitOp = responseType.GetMethod(
                "op_Implicit",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(Error) },
                modifiers: null);

            if (implicitOp is not null)
                return (TResponse)implicitOp.Invoke(null, new object[] { error })!;
        }

        throw new UnauthorizedAccessException(error.Message);
    }
}
