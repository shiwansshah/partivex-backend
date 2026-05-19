using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface ICustomerPartPurchaseRepository
{
    Task<IReadOnlyList<CustomerPartPurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerPartPurchaseInvoice>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    Task<CustomerPartPurchaseInvoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    Task AddAsync(CustomerPartPurchaseInvoice invoice, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
