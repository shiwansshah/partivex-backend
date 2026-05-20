using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ICustomerReportService
{
    Task<IReadOnlyList<CustomerReportDto>> GetRegularCustomersAsync();

    Task<IReadOnlyList<CustomerReportDto>> GetHighSpendersAsync();

    Task<IReadOnlyList<CustomerReportDto>> GetCreditCustomersAsync();
}