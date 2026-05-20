using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ISalesService
{
    Task<IReadOnlyCollection<SaleDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SalesResult<SaleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<SalesResult<SaleDto>> CreateAsync(
        CreateSaleCommand command,
        CancellationToken cancellationToken = default);

    Task<SalesReportSummaryDto> GetReportSummaryAsync(
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default);
}
