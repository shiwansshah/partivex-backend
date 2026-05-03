using Microsoft.AspNetCore.Http;
using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(string customerId);

    Task<VehicleDto> AddVehicleAsync(string customerId, CreateVehicleDto dto, IFormFile? image);

    Task<VehicleDto> UpdateVehicleAsync(Guid id, string customerId, UpdateVehicleDto dto, IFormFile? image);
}
