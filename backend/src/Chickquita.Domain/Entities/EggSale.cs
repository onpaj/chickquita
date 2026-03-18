using Chickquita.Domain.Common;

namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents a sale of eggs from the farm.
/// Tracks revenue by recording quantity sold, price per unit, and optional buyer/notes.
/// </summary>
public class EggSale
{
    /// <summary>
    /// Unique identifier for the egg sale.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this egg sale.
    /// Used for multi-tenancy and data isolation.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The date when the sale occurred.
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Number of eggs sold.
    /// Must be greater than zero.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Price per single egg.
    /// Must be greater than zero.
    /// </summary>
    public decimal PricePerUnit { get; private set; }

    /// <summary>
    /// Optional name of the buyer.
    /// Maximum 200 characters.
    /// </summary>
    public string? BuyerName { get; private set; }

    /// <summary>
    /// Optional notes about the sale.
    /// Maximum 1000 characters.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Timestamp when the egg sale record was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

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
    /// <param name="tenantId">The ID of the tenant that owns this sale</param>
    /// <param name="date">The date when the sale occurred</param>
    /// <param name="quantity">Number of eggs sold (must be > 0)</param>
    /// <param name="pricePerUnit">Price per single egg (must be > 0)</param>
    /// <param name="buyerName">Optional name of the buyer</param>
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

        if (date == default)
            return Error.Validation("Sale date cannot be a default value.");

        if (quantity <= 0)
            return Error.Validation("Quantity must be greater than zero.");

        if (pricePerUnit <= 0)
            return Error.Validation("Price per unit must be greater than zero.");

        if (buyerName != null && buyerName.Length > 200)
            return Error.Validation("Buyer name cannot exceed 200 characters.");

        if (notes != null && notes.Length > 1000)
            return Error.Validation("Notes cannot exceed 1000 characters.");

        // Normalize date to UTC date-only (midnight)
        var dateUtc = date.Kind switch
        {
            DateTimeKind.Utc => date.Date,
            DateTimeKind.Local => date.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
        };

        return new EggSale
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Date = dateUtc,
            Quantity = quantity,
            PricePerUnit = pricePerUnit,
            BuyerName = buyerName,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the egg sale details.
    /// </summary>
    /// <param name="date">The date when the sale occurred</param>
    /// <param name="quantity">Number of eggs sold (must be > 0)</param>
    /// <param name="pricePerUnit">Price per single egg (must be > 0)</param>
    /// <param name="buyerName">Optional name of the buyer</param>
    /// <param name="notes">Optional notes about the sale</param>
    /// <returns>A Result indicating success or a validation error</returns>
    public Result Update(
        DateTime date,
        int quantity,
        decimal pricePerUnit,
        string? buyerName,
        string? notes)
    {
        if (date == default)
            return Error.Validation("Sale date cannot be a default value.");

        if (quantity <= 0)
            return Error.Validation("Quantity must be greater than zero.");

        if (pricePerUnit <= 0)
            return Error.Validation("Price per unit must be greater than zero.");

        if (buyerName != null && buyerName.Length > 200)
            return Error.Validation("Buyer name cannot exceed 200 characters.");

        if (notes != null && notes.Length > 1000)
            return Error.Validation("Notes cannot exceed 1000 characters.");

        // Normalize date to UTC date-only (midnight)
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

        return Result.Success();
    }
}
