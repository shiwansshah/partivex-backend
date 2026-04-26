using Partivex.Application.DTOs.Vehicles;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(ICustomerRepository customerRepository, IVehicleRepository vehicleRepository)
    {
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<List<VehicleDto>> GetByCustomerIdAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer is null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        var vehicles = await _vehicleRepository.GetByCustomerIdAsync(customerId);
        return vehicles.Select(ToDto).ToList();
    }

    public async Task<VehicleDto> AddVehicleAsync(int customerId, CreateVehicleDto dto)
    {
        var vehicleNumber = dto.VehicleNumber.Trim();

        if (string.IsNullOrWhiteSpace(vehicleNumber))
        {
            throw new ArgumentException("Vehicle number is required.");
        }

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer is null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        var vehicle = new Vehicle
        {
            CustomerId = customerId,
            VehicleNumber = vehicleNumber,
            Brand = string.IsNullOrWhiteSpace(dto.Brand) ? null : dto.Brand.Trim(),
            Model = string.IsNullOrWhiteSpace(dto.Model) ? null : dto.Model.Trim(),
            Year = dto.Year,
            VehicleType = string.IsNullOrWhiteSpace(dto.VehicleType) ? null : dto.VehicleType.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
        };

        var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
        return ToDto(createdVehicle);
    }

    private static VehicleDto ToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            VehicleNumber = vehicle.VehicleNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            VehicleType = vehicle.VehicleType,
            Notes = vehicle.Notes
        };
    }
}
