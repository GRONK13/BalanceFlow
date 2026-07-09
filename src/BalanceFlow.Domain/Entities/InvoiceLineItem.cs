namespace BalanceFlow.Domain.Entities;

/// <summary>
/// Represents an itemized line inside a vendor invoice.
/// Map target debits to specific accounts (e.g. Office Supplies, Travel Expenses).
/// </summary>
public sealed class InvoiceLineItem : BaseEntity
{
    /// <summary>Foreign key linking back to the parent invoice.</summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>Parent invoice navigation property.</summary>
    public Invoice Invoice { get; private set; } = null!;

    /// <summary>Target Account ID where this line item expense or asset should be posted.</summary>
    public Guid AccountId { get; private set; }

    /// <summary>Target Account navigation property.</summary>
    public Account Account { get; private set; } = null!;

    /// <summary>Item description.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Quantity of items purchased.</summary>
    public int Quantity { get; private set; }

    /// <summary>Price per unit. Uses decimal for monetary math precision.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Computed line total (Quantity * UnitPrice).</summary>
    public decimal LineTotal => Quantity * UnitPrice;

    // EF Core constructor
    private InvoiceLineItem() { }

    public InvoiceLineItem(Guid accountId, string description, int quantity, decimal unitPrice)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID must not be empty.", nameof(accountId));

        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero.", nameof(unitPrice));

        AccountId = accountId;
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
