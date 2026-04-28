using Partivex.Application.DTOs; // Imports vehicle DTOs.

namespace Partivex.Application.Interfaces; // Defines interface namespace.

public interface IVehicleService // Defines vehicle service contract.
{
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto); // Creates vehicle.

    Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerAsync(string customerId); // Gets customer vehicles.

    Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto); // Updates vehicle.

    Task DeleteVehicleAsync(Guid id); // Deletes vehicle.
}
