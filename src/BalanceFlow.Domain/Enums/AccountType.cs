namespace BalanceFlow.Domain.Enums;

/// <summary>
/// Represents the five fundamental account classifications
/// in double-entry bookkeeping per the accounting equation:
/// Assets = Liabilities + Equity, extended by Revenue and Expense.
/// </summary>
public enum AccountType
{
    /// <summary>Resources owned by the entity (e.g., Cash, Accounts Receivable).</summary>
    Asset = 1,

    /// <summary>Obligations owed to external parties (e.g., Accounts Payable, Loans).</summary>
    Liability = 2,

    /// <summary>Residual interest in the entity's assets after deducting liabilities.</summary>
    Equity = 3,

    /// <summary>Income earned from primary business operations.</summary>
    Revenue = 4,

    /// <summary>Costs incurred in the process of earning revenue.</summary>
    Expense = 5
}
