using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Command to update basic flock information (identifier and hatch date).
/// Does not modify flock composition - use composition-specific commands for that.
/// </summary>
public sealed record UpdateFlockCommand : IRequest<Result<FlockDto>>
{
    /// <summary>
    /// Gets or sets the ID of the flock to update.
    /// </summary>
    public Guid FlockId { get; init; }

    /// <summary>
    /// Gets or sets the new identifier for the flock.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the new hatch date for the flock.
    /// </summary>
    public DateTime HatchDate { get; init; }
}
