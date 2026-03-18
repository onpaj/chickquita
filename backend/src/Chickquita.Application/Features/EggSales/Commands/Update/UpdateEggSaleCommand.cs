using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.EggSales.Commands.Update;

/// <summary>
/// Command to update an existing egg sale record.
/// </summary>
public sealed record UpdateEggSaleCommand : IRequest<Result<EggSaleDto>>
{
    /// <summary>
    /// Gets the ID of the egg sale to update.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the date when the sale occurred.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Gets the number of eggs sold. Must be greater than zero.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets the price per single egg. Must be greater than zero.
    /// </summary>
    public decimal PricePerUnit { get; init; }

    /// <summary>
    /// Gets the optional name of the buyer.
    /// </summary>
    public string? BuyerName { get; init; }

    /// <summary>
    /// Gets the optional notes about the sale.
    /// </summary>
    public string? Notes { get; init; }
}
