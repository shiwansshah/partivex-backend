using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetByCustomerIdAsync(int customerId);

    Task<Vehicle> AddAsync(Vehicle vehicle);
}
