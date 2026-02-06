using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Query to get a single flock by ID.
/// </summary>
public sealed record GetFlockByIdQuery : IRequest<Result<FlockDto>>
{
    /// <summary>
    /// Gets or sets the ID of the flock to retrieve.
    /// </summary>
    public Guid FlockId { get; init; }
}
