namespace BalanceFlow.Domain.Enums;

/// <summary>
/// Represents the system audit check status of an invoice.
/// </summary>
public enum InvoiceAuditStatus
{
    /// <summary>No audit rules have run yet.</summary>
    NotAudited = 1,

    /// <summary>Audit rules ran and all checks passed.</summary>
    Passed = 2,

    /// <summary>Audit rules ran and flagged mathematical/logical issues.</summary>
    Failed = 3
}
