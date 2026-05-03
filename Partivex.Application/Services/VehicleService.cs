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

        return vehicles.Select(MapToDto).ToArray();
    }

    public async Task<VehicleDto> AddVehicleAsync(string customerId, CreateVehicleDto dto, IFormFile? image)
    {
        var name = ResolveName(dto.Name, dto.Model);
        var number = ResolveNumber(dto.Number, dto.VehicleNumber);
        string? imageUrl = null;

        if (image is not null)
        {
            imageUrl = await _fileStorageService.SaveFileAsync(image, "vehicles");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Name = name,
            Number = number,
            ImageUrl = imageUrl
        };

        await _vehicleRepository.AddAsync(vehicle);

        return MapToDto(vehicle);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid id, string customerId, UpdateVehicleDto dto, IFormFile? image)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null || vehicle.CustomerId != customerId)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        vehicle.Name = ResolveName(dto.Name, dto.Model);
        vehicle.Number = ResolveNumber(dto.Number, dto.VehicleNumber);

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

        return MapToDto(vehicle);
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
    {
        var customerId = NormalizeRequired(dto.CustomerId, nameof(dto.CustomerId));

        return await AddVehicleAsync(customerId, dto, null);
    }

    public async Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerAsync(string customerId)
    {
        return await GetCustomerVehiclesAsync(customerId);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        vehicle.Name = ResolveName(dto.Name, dto.Model);
        vehicle.Number = ResolveNumber(dto.Number, dto.VehicleNumber);

        await _vehicleRepository.UpdateAsync(vehicle);

        return MapToDto(vehicle);
    }

    public async Task DeleteVehicleAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (!string.IsNullOrEmpty(vehicle.ImageUrl))
        {
            _fileStorageService.DeleteFile(vehicle.ImageUrl);
        }

        await _vehicleRepository.DeleteAsync(vehicle);
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto(vehicle.Id, vehicle.Name, vehicle.Number, vehicle.ImageUrl, vehicle.CustomerId);
    }

    private static string ResolveName(string? name, string? model)
    {
        return NormalizeRequired(name ?? model, nameof(CreateVehicleDto.Name));
    }

    private static string ResolveNumber(string? number, string? vehicleNumber)
    {
        return NormalizeRequired(number ?? vehicleNumber, nameof(CreateVehicleDto.Number));
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
