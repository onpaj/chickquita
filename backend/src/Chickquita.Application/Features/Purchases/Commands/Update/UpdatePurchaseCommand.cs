using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Purchases.Commands.Update;

/// <summary>
/// Command to update an existing purchase.
/// </summary>
public sealed record UpdatePurchaseCommand : IRequest<Result<PurchaseDto>>
{
    /// <summary>
    /// Gets or sets the ID of the purchase to update.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the coop this purchase is associated with (optional).
    /// </summary>
    public Guid? CoopId { get; init; }

    /// <summary>
    /// Gets or sets the name or description of the purchased item.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the purchase.
    /// </summary>
    public PurchaseType Type { get; init; }

    /// <summary>
    /// Gets or sets the amount paid for the purchase.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets or sets the quantity purchased.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Gets or sets the unit of the quantity.
    /// </summary>
    public QuantityUnit Unit { get; init; }

    /// <summary>
    /// Gets or sets the date when the purchase was made.
    /// </summary>
    public DateTime PurchaseDate { get; init; }

    /// <summary>
    /// Gets or sets the date when the item was consumed or used (optional).
    /// </summary>
    public DateTime? ConsumedDate { get; init; }

    /// <summary>
    /// Gets or sets optional notes about the purchase.
    /// </summary>
    public string? Notes { get; init; }
}
