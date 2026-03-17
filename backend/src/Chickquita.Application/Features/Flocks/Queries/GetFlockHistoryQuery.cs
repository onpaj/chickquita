using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Query to get the full history of a flock's composition changes.
/// </summary>
public sealed record GetFlockHistoryQuery : IRequest<Result<List<FlockHistoryDto>>>
{
    /// <summary>
    /// The ID of the flock to get history for.
    /// </summary>
    public required Guid FlockId { get; init; }
}
