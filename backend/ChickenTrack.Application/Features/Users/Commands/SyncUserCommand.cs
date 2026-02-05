using ChickenTrack.Application.DTOs;
using ChickenTrack.Domain.Common;
using MediatR;

namespace ChickenTrack.Application.Features.Users.Commands;

/// <summary>
/// Command to sync a user from Clerk webhook and create or retrieve the associated tenant.
/// This operation is idempotent - calling it multiple times with the same data will not create duplicates.
/// </summary>
public sealed record SyncUserCommand : IRequest<Result<TenantDto>>
{
    /// <summary>
    /// Gets or sets the Clerk user ID.
    /// </summary>
    public string ClerkUserId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
