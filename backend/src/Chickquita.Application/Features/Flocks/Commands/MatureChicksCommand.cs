using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Command to mature chicks into adult hens and roosters.
/// Converts the specified number of chicks into hens and roosters,
/// creating an immutable FlockHistory entry with reason "Maturation".
/// </summary>
public sealed record MatureChicksCommand : IRequest<Result<FlockDto>>
{
    /// <summary>
    /// Gets or sets the ID of the flock whose chicks are maturing.
    /// </summary>
    public Guid FlockId { get; set; }

    /// <summary>
    /// Gets or sets the total number of chicks to mature.
    /// Must be between 1 and the flock's current chick count.
    /// </summary>
    public int ChicksToMature { get; init; }

    /// <summary>
    /// Gets or sets the number of maturing chicks that become hens.
    /// </summary>
    public int Hens { get; init; }

    /// <summary>
    /// Gets or sets the number of maturing chicks that become roosters.
    /// </summary>
    public int Roosters { get; init; }

    /// <summary>
    /// Gets or sets optional notes about this maturation event.
    /// </summary>
    public string? Notes { get; init; }
}
