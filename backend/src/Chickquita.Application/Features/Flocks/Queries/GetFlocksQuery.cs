using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Query to get all flocks for a specific coop.
/// </summary>
public sealed record GetFlocksQuery : IRequest<Result<List<FlockDto>>>
{
    /// <summary>
    /// Gets or sets the ID of the coop to get flocks for.
    /// </summary>
    public Guid CoopId { get; init; }

    /// <summary>
    /// Gets or sets whether to include inactive (archived) flocks.
    /// Defaults to false.
    /// </summary>
    public bool IncludeInactive { get; init; } = false;
}
