using Partivex.Application.DTOs.Customers;
using Partivex.Application.DTOs.Vehicles;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Select(ToDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer is null ? null : ToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var fullName = dto.FullName.Trim();
        var phone = dto.Phone.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Phone number is required.");
        }

        if (await _customerRepository.PhoneExistsAsync(phone))
        {
            throw new InvalidOperationException("Phone number already exists.");
        }

        var customer = new Customer
        {
            FullName = fullName,
            Phone = phone,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var createdCustomer = await _customerRepository.AddAsync(customer);
        return ToDto(createdCustomer);
    }

    private static CustomerDto ToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Vehicles = customer.Vehicles.Select(vehicle => new VehicleDto
            {
                Id = vehicle.Id,
                CustomerId = vehicle.CustomerId,
                VehicleNumber = vehicle.VehicleNumber,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                VehicleType = vehicle.VehicleType,
                Notes = vehicle.Notes
            }).ToList()
        };
    }
}
