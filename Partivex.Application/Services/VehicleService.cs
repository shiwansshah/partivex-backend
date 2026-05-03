using Microsoft.AspNetCore.Http;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IFileStorageService _fileStorageService;

    public VehicleService(IVehicleRepository vehicleRepository, IFileStorageService fileStorageService)
    {
        _vehicleRepository = vehicleRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(string customerId)
    {
        var vehicles = await _vehicleRepository.GetByCustomerIdAsync(customerId);

        return vehicles.Select(v => new VehicleDto(v.Id, v.Name, v.Number, v.ImageUrl)).ToArray();
    }

    public async Task<VehicleDto> AddVehicleAsync(string customerId, CreateVehicleDto dto, IFormFile? image)
    {
        string? imageUrl = null;

        if (image is not null)
        {
            imageUrl = await _fileStorageService.SaveFileAsync(image, "vehicles");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Name = dto.Name,
            Number = dto.Number,
            ImageUrl = imageUrl
        };

        await _vehicleRepository.AddAsync(vehicle);

        return new VehicleDto(vehicle.Id, vehicle.Name, vehicle.Number, vehicle.ImageUrl);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid id, string customerId, UpdateVehicleDto dto, IFormFile? image)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null || vehicle.CustomerId != customerId)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        vehicle.Name = dto.Name;
        vehicle.Number = dto.Number;

        if (image is not null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(vehicle.ImageUrl))
            {
                _fileStorageService.DeleteFile(vehicle.ImageUrl);
            }

            vehicle.ImageUrl = await _fileStorageService.SaveFileAsync(image, "vehicles");
        }

        await _vehicleRepository.UpdateAsync(vehicle);

        return new VehicleDto(vehicle.Id, vehicle.Name, vehicle.Number, vehicle.ImageUrl);
    }
}
