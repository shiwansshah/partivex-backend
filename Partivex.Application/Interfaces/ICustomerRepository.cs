using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();

    Task<Customer?> GetByIdAsync(int id);

    Task<bool> PhoneExistsAsync(string phone);

    Task<Customer> AddAsync(Customer customer);
}
