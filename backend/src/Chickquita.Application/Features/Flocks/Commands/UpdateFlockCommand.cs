using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Command to update flock information (identifier, hatch date, and optionally composition).
/// When composition differs from current values, a history entry is created with reason "Manual update".
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

    /// <summary>
    /// Gets or sets the new number of hens. When null, current composition is preserved.
    /// </summary>
    public int? CurrentHens { get; init; }

    /// <summary>
    /// Gets or sets the new number of roosters. When null, current composition is preserved.
    /// </summary>
    public int? CurrentRoosters { get; init; }

    /// <summary>
    /// Gets or sets the new number of chicks. When null, current composition is preserved.
    /// </summary>
    public int? CurrentChicks { get; init; }
}
