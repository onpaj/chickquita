using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.EggSales.Commands.Create;

/// <summary>
/// Command to create a new egg sale record.
/// </summary>
public sealed record CreateEggSaleCommand : IRequest<Result<EggSaleDto>>
{
    /// <summary>
    /// Gets or sets the date when the sale occurred.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Gets or sets the number of eggs sold.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets or sets the price per egg.
    /// </summary>
    public decimal PricePerUnit { get; init; }

    /// <summary>
    /// Gets or sets the name of the buyer (optional).
    /// </summary>
    public string? BuyerName { get; init; }

    /// <summary>
    /// Gets or sets optional notes about the sale.
    /// </summary>
    public string? Notes { get; init; }
}
