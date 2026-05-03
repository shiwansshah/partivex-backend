using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetByCustomerIdAsync(string customerId);

    Task<Vehicle?> GetByIdAsync(Guid id);

    Task AddAsync(Vehicle vehicle);

    Task UpdateAsync(Vehicle vehicle);
}
