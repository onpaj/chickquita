using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Command to archive a coop (soft delete).
/// </summary>
public sealed record ArchiveCoopCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets the ID of the coop to archive.
    /// </summary>
    public Guid Id { get; init; }
}
