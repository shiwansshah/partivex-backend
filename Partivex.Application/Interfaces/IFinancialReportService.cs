using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IFinancialReportService
{
    Task<FinancialReportDto> GetReportAsync(
        string period,
        DateOnly referenceDate,
        CancellationToken cancellationToken = default);
}
