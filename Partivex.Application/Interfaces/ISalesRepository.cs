using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface ISalesRepository
{
    Task<IReadOnlyCollection<Sale>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default);

    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<SalesReport?> GetReportByDateAsync(DateOnly reportDate, CancellationToken cancellationToken = default);

    Task AddReportAsync(SalesReport report, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SalesReport>> GetReportsAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
