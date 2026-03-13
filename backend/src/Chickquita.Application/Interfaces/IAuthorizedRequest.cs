namespace Chickquita.Application.Interfaces;

/// <summary>
/// Marker interface for MediatR requests that require an authenticated user with a resolved tenant.
/// The <see cref="Chickquita.Application.Behaviors.AuthorizationBehavior{TRequest,TResponse}"/>
/// pipeline behavior enforces this automatically for all implementing requests.
/// </summary>
public interface IAuthorizedRequest;
