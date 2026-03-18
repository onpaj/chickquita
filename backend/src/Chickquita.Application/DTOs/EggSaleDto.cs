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
    /// The date when the sale occurred.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Number of eggs sold.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price per single egg.
    /// </summary>
    public decimal PricePerUnit { get; set; }

    /// <summary>
    /// Total revenue from this sale (Quantity * PricePerUnit).
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Optional name of the buyer.
    /// </summary>
    public string? BuyerName { get; set; }

    /// <summary>
    /// Optional notes about the sale.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when the egg sale record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
