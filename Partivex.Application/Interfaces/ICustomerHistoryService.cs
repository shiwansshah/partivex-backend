using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface ICustomerHistoryService
{
    Task<IReadOnlyList<CustomerHistoryDto>> GetHistoryAsync(string customerId);

    Task<CustomerHistoryDto> CreateHistoryAsync(string customerId, CreateCustomerHistoryDto dto);
}