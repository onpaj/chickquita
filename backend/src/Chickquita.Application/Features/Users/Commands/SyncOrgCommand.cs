using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Users.Commands;

/// <summary>
/// Command to sync an organization from Clerk webhook and create or update the associated tenant.
/// This operation is idempotent - calling it multiple times with the same data will not create duplicates.
/// </summary>
public sealed record SyncOrgCommand : IRequest<Result<TenantDto>>, IAnonymousRequest
{
    /// <summary>Gets the Clerk Organization ID.</summary>
    public string ClerkOrgId { get; init; } = string.Empty;

    /// <summary>Gets the organization display name.</summary>
    public string Name { get; init; } = string.Empty;
}
