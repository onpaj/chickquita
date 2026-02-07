namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents the type of purchase for chicken farming.
/// </summary>
public enum PurchaseType
{
    /// <summary>
    /// Chicken feed purchase.
    /// </summary>
    Feed = 0,

    /// <summary>
    /// Vitamins and supplements purchase.
    /// </summary>
    Vitamins = 1,

    /// <summary>
    /// Bedding material purchase.
    /// </summary>
    Bedding = 2,

    /// <summary>
    /// Toys and enrichment items purchase.
    /// </summary>
    Toys = 3,

    /// <summary>
    /// Veterinary care and medication purchase.
    /// </summary>
    Veterinary = 4,

    /// <summary>
    /// Other miscellaneous purchases.
    /// </summary>
    Other = 5
}

/// <summary>
/// Represents the unit of quantity for purchased items.
/// </summary>
public enum QuantityUnit
{
    /// <summary>
    /// Kilograms.
    /// </summary>
    Kg = 0,

    /// <summary>
    /// Pieces.
    /// </summary>
    Pcs = 1,

    /// <summary>
    /// Liters.
    /// </summary>
    L = 2,

    /// <summary>
    /// Package (unspecified unit).
    /// </summary>
    Package = 3,

    /// <summary>
    /// Other unit not listed.
    /// </summary>
    Other = 4
}

/// <summary>
/// Represents a purchase made for chicken farming.
/// Tracks expenses such as feed, bedding, veterinary care, and other costs.
/// </summary>
public class Purchase
{
    /// <summary>
    /// Unique identifier for the purchase.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this purchase.
    /// Used for multi-tenancy and data isolation.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The coop this purchase is associated with (optional).
    /// If null, the purchase is general and not tied to a specific coop.
    /// </summary>
    public Guid? CoopId { get; private set; }

    /// <summary>
    /// Name or description of the purchased item.
    /// Maximum 100 characters.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Type of the purchase (Feed, Vitamins, Bedding, etc.).
    /// </summary>
    public PurchaseType Type { get; private set; }

    /// <summary>
    /// The amount paid for the purchase.
    /// Must be greater than or equal to 0.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// The quantity purchased.
    /// Must be greater than 0.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// The unit of the quantity (Kg, Pcs, L, Package, Other).
    /// </summary>
    public QuantityUnit Unit { get; private set; }

    /// <summary>
    /// The date when the purchase was made.
    /// </summary>
    public DateTime PurchaseDate { get; private set; }

    /// <summary>
    /// The date when the item was consumed or used (optional).
    /// If set, must be greater than or equal to PurchaseDate.
    /// </summary>
    public DateTime? ConsumedDate { get; private set; }

    /// <summary>
    /// Optional notes about the purchase.
    /// Maximum 500 characters.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Timestamp when the purchase was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the purchase was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the tenant.
    /// </summary>
    public Tenant Tenant { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the coop (optional).
    /// </summary>
    public Coop? Coop { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Purchase()
    {
    }

    /// <summary>
    /// Factory method to create a new Purchase.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant that owns this purchase</param>
    /// <param name="name">Name or description of the purchased item</param>
    /// <param name="type">Type of the purchase</param>
    /// <param name="amount">Amount paid for the purchase</param>
    /// <param name="quantity">Quantity purchased</param>
    /// <param name="unit">Unit of the quantity</param>
    /// <param name="purchaseDate">Date when the purchase was made</param>
    /// <param name="coopId">Optional ID of the coop this purchase is associated with</param>
    /// <param name="consumedDate">Optional date when the item was consumed</param>
    /// <param name="notes">Optional notes about the purchase</param>
    /// <returns>A new Purchase instance</returns>
    public static Purchase Create(
        Guid tenantId,
        string name,
        PurchaseType type,
        decimal amount,
        decimal quantity,
        QuantityUnit unit,
        DateTime purchaseDate,
        Guid? coopId = null,
        DateTime? consumedDate = null,
        string? notes = null)
    {
        // Validate tenant ID
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Purchase name cannot be empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Purchase name cannot exceed 100 characters.", nameof(name));
        }

        // Validate amount
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        // Validate quantity
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        // Normalize purchase date to UTC and date only (midnight)
        var purchaseDateUtc = purchaseDate.Kind switch
        {
            DateTimeKind.Utc => purchaseDate.Date,
            DateTimeKind.Local => purchaseDate.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(purchaseDate.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(purchaseDate.Date, DateTimeKind.Utc)
        };

        // Validate and normalize consumed date if provided
        DateTime? consumedDateUtc = null;
        if (consumedDate.HasValue)
        {
            consumedDateUtc = consumedDate.Value.Kind switch
            {
                DateTimeKind.Utc => consumedDate.Value.Date,
                DateTimeKind.Local => consumedDate.Value.ToUniversalTime().Date,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(consumedDate.Value.Date, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(consumedDate.Value.Date, DateTimeKind.Utc)
            };

            // Validate consumed date is not before purchase date
            if (consumedDateUtc < purchaseDateUtc)
            {
                throw new ArgumentException("Consumed date cannot be before purchase date.", nameof(consumedDate));
            }
        }

        // Validate notes length if provided
        if (notes != null && notes.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        var now = DateTime.UtcNow;

        return new Purchase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CoopId = coopId,
            Name = name,
            Type = type,
            Amount = amount,
            Quantity = quantity,
            Unit = unit,
            PurchaseDate = purchaseDateUtc,
            ConsumedDate = consumedDateUtc,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the purchase details.
    /// </summary>
    /// <param name="name">Name or description of the purchased item</param>
    /// <param name="type">Type of the purchase</param>
    /// <param name="amount">Amount paid for the purchase</param>
    /// <param name="quantity">Quantity purchased</param>
    /// <param name="unit">Unit of the quantity</param>
    /// <param name="purchaseDate">Date when the purchase was made</param>
    /// <param name="coopId">Optional ID of the coop this purchase is associated with</param>
    /// <param name="consumedDate">Optional date when the item was consumed</param>
    /// <param name="notes">Optional notes about the purchase</param>
    public void Update(
        string name,
        PurchaseType type,
        decimal amount,
        decimal quantity,
        QuantityUnit unit,
        DateTime purchaseDate,
        Guid? coopId = null,
        DateTime? consumedDate = null,
        string? notes = null)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Purchase name cannot be empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Purchase name cannot exceed 100 characters.", nameof(name));
        }

        // Validate amount
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        // Validate quantity
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        // Normalize purchase date to UTC and date only (midnight)
        var purchaseDateUtc = purchaseDate.Kind switch
        {
            DateTimeKind.Utc => purchaseDate.Date,
            DateTimeKind.Local => purchaseDate.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(purchaseDate.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(purchaseDate.Date, DateTimeKind.Utc)
        };

        // Validate and normalize consumed date if provided
        DateTime? consumedDateUtc = null;
        if (consumedDate.HasValue)
        {
            consumedDateUtc = consumedDate.Value.Kind switch
            {
                DateTimeKind.Utc => consumedDate.Value.Date,
                DateTimeKind.Local => consumedDate.Value.ToUniversalTime().Date,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(consumedDate.Value.Date, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(consumedDate.Value.Date, DateTimeKind.Utc)
            };

            // Validate consumed date is not before purchase date
            if (consumedDateUtc < purchaseDateUtc)
            {
                throw new ArgumentException("Consumed date cannot be before purchase date.", nameof(consumedDate));
            }
        }

        // Validate notes length if provided
        if (notes != null && notes.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        Name = name;
        Type = type;
        Amount = amount;
        Quantity = quantity;
        Unit = unit;
        PurchaseDate = purchaseDateUtc;
        CoopId = coopId;
        ConsumedDate = consumedDateUtc;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
