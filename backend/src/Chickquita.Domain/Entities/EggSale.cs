using Chickquita.Domain.Common;

namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents a sale of eggs from the farm.
/// Tracks revenue by recording quantity sold, price per unit, and optional buyer info.
/// </summary>
public class EggSale
{
    /// <summary>
    /// Unique identifier for the egg sale.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this egg sale record.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The date when the sale occurred.
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// The number of eggs sold. Must be greater than zero.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// The price per egg. Must be greater than or equal to zero.
    /// </summary>
    public decimal PricePerUnit { get; private set; }

    /// <summary>
    /// The name of the buyer (optional). Maximum 100 characters.
    /// </summary>
    public string? BuyerName { get; private set; }

    /// <summary>
    /// Optional notes about the sale. Maximum 500 characters.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Timestamp when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when this record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the tenant.
    /// </summary>
    public Tenant Tenant { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private EggSale()
    {
    }

    /// <summary>
    /// Factory method to create a new EggSale.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant that owns this record</param>
    /// <param name="date">The date the sale occurred</param>
    /// <param name="quantity">Number of eggs sold</param>
    /// <param name="pricePerUnit">Price per egg</param>
    /// <param name="buyerName">Optional buyer name</param>
    /// <param name="notes">Optional notes about the sale</param>
    /// <returns>A Result containing the new EggSale instance, or a validation error</returns>
    public static Result<EggSale> Create(
        Guid tenantId,
        DateTime date,
        int quantity,
        decimal pricePerUnit,
        string? buyerName = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("Tenant ID cannot be empty.");

        if (quantity <= 0)
            return Error.Validation("Quantity must be greater than zero.");

        if (pricePerUnit < 0)
            return Error.Validation("Price per unit cannot be negative.");

        if (buyerName != null && buyerName.Length > 100)
            return Error.Validation("Buyer name cannot exceed 100 characters.");

        if (notes != null && notes.Length > 500)
            return Error.Validation("Notes cannot exceed 500 characters.");

        // Normalize date to UTC midnight
        var dateUtc = date.Kind switch
        {
            DateTimeKind.Utc => date.Date,
            DateTimeKind.Local => date.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
        };

        var now = DateTime.UtcNow;

        return new EggSale
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Date = dateUtc,
            Quantity = quantity,
            PricePerUnit = pricePerUnit,
            BuyerName = buyerName,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the egg sale details.
    /// </summary>
    /// <param name="date">The date the sale occurred</param>
    /// <param name="quantity">Number of eggs sold</param>
    /// <param name="pricePerUnit">Price per egg</param>
    /// <param name="buyerName">Optional buyer name</param>
    /// <param name="notes">Optional notes about the sale</param>
    /// <returns>A Result indicating success or a validation error</returns>
    public Result Update(
        DateTime date,
        int quantity,
        decimal pricePerUnit,
        string? buyerName = null,
        string? notes = null)
    {
        if (quantity <= 0)
            return Error.Validation("Quantity must be greater than zero.");

        if (pricePerUnit < 0)
            return Error.Validation("Price per unit cannot be negative.");

        if (buyerName != null && buyerName.Length > 100)
            return Error.Validation("Buyer name cannot exceed 100 characters.");

        if (notes != null && notes.Length > 500)
            return Error.Validation("Notes cannot exceed 500 characters.");

        // Normalize date to UTC midnight
        var dateUtc = date.Kind switch
        {
            DateTimeKind.Utc => date.Date,
            DateTimeKind.Local => date.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
        };

        Date = dateUtc;
        Quantity = quantity;
        PricePerUnit = pricePerUnit;
        BuyerName = buyerName;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
