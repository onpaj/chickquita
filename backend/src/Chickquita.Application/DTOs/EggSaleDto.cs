namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for EggSale entity.
/// </summary>
public sealed class EggSaleDto
{
    /// <summary>
    /// Unique identifier for the egg sale.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The tenant that owns this egg sale.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The date when the sale occurred.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The number of eggs sold.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The price per egg.
    /// </summary>
    public decimal PricePerUnit { get; set; }

    /// <summary>
    /// The name of the buyer (optional).
    /// </summary>
    public string? BuyerName { get; set; }

    /// <summary>
    /// Optional notes about the sale.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when this record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
