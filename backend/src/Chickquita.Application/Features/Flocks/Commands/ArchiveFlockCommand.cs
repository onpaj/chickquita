using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;
using Chickquita.Application.Interfaces;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Command to archive a flock (soft delete).
/// Sets IsActive = false while preserving all data.
/// </summary>
public sealed record ArchiveFlockCommand : IRequest<Result<FlockDto>>, IAuthorizedRequest
{
    /// <summary>
    /// Gets or sets the ID of the flock to archive.
    /// </summary>
    public Guid FlockId { get; init; }
}
