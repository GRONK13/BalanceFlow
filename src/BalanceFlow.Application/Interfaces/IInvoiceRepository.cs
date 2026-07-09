namespace BalanceFlow.Application.Interfaces;

/// <summary>
/// Data access contract for the <see cref="Invoice"/> entity.
/// </summary>
public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByInvoiceNumberAndVendorAsync(string invoiceNumber, string vendorName, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    void Update(Invoice invoice);
}
