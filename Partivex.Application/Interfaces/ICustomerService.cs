using Partivex.Application.DTOs.Customers;

namespace Partivex.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();

    Task<CustomerDto?> GetByIdAsync(int id);

    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
}
