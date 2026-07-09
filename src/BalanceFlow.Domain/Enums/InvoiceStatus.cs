namespace BalanceFlow.Domain.Enums;

/// <summary>
/// Represents the workflow status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>Invoice is being drafted and can be edited.</summary>
    Draft = 1,

    /// <summary>System audits have run; invoice is waiting approval.</summary>
    Audited = 2,

    /// <summary>Approved by auditor. Balanced journal entries are drafted and immutable.</summary>
    Approved = 3,

    /// <summary>Invoice failed compliance checks or was rejected manually.</summary>
    Rejected = 4
}
