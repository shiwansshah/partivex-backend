using Partivex.Application.DTOs.Vehicles;

namespace Partivex.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleDto>> GetByCustomerIdAsync(int customerId);

    Task<VehicleDto> AddVehicleAsync(int customerId, CreateVehicleDto dto);
}
