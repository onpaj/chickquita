using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Command to update flock composition (hens, roosters, chicks).
/// Creates an immutable FlockHistory entry with the given reason.
/// Use UpdateFlockCommand to update metadata (identifier, hatch date).
/// </summary>
public sealed record UpdateFlockCompositionCommand : IRequest<Result<FlockDto>>
{
    /// <summary>
    /// Gets or sets the ID of the flock to update.
    /// </summary>
    public Guid FlockId { get; set; }

    /// <summary>
    /// Gets or sets the new number of hens.
    /// </summary>
    public int Hens { get; init; }

    /// <summary>
    /// Gets or sets the new number of roosters.
    /// </summary>
    public int Roosters { get; init; }

    /// <summary>
    /// Gets or sets the new number of chicks.
    /// </summary>
    public int Chicks { get; init; }

    /// <summary>
    /// Gets or sets the reason for this composition change.
    /// Defaults to "Manual update" when not specified.
    /// </summary>
    public string Reason { get; init; } = "Manual update";

    /// <summary>
    /// Gets or sets optional notes about this composition change.
    /// </summary>
    public string? Notes { get; init; }
}
