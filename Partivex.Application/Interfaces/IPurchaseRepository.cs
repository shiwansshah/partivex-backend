using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IPurchaseRepository
{
    Task<IReadOnlyCollection<PurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PurchaseInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    Task AddAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default);

    void Remove(PurchaseInvoice invoice);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
