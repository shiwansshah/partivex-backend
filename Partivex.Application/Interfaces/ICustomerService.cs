using Partivex.Application.DTOs; // Imports customer DTOs.

namespace Partivex.Application.Interfaces; // Defines interface namespace.

public interface ICustomerService // Defines customer service contract.
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(); // Gets all customers.

    Task<CustomerDetailDto?> GetCustomerByIdAsync(string id); // Gets customer detail.

    Task<CustomerHistoryDto?> GetCustomerHistoryAsync(string id); // Gets customer history.
}
