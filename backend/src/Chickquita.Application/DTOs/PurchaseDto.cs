using Chickquita.Domain.Entities;

namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for Purchase entity.
/// </summary>
public sealed class PurchaseDto
{
    /// <summary>
    /// Unique identifier for the purchase.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The tenant that owns this purchase.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The coop this purchase is associated with (optional).
    /// </summary>
    public Guid? CoopId { get; set; }

    /// <summary>
    /// Name or description of the purchased item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of the purchase (Feed, Vitamins, Bedding, etc.).
    /// </summary>
    public PurchaseType Type { get; set; }

    /// <summary>
    /// The amount paid for the purchase.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The quantity purchased.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// The unit of the quantity (Kg, Pcs, L, Package, Other).
    /// </summary>
    public QuantityUnit Unit { get; set; }

    /// <summary>
    /// The date when the purchase was made.
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// The date when the item was consumed or used (optional).
    /// </summary>
    public DateTime? ConsumedDate { get; set; }

    /// <summary>
    /// Optional notes about the purchase.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when the purchase was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the purchase was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
