using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using System.Text.RegularExpressions;

namespace Partivex.Application.Services;

public sealed class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;

    public VendorService(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<IReadOnlyList<VendorResponseDto>> GetAllAsync()
    {
        var vendors = await _vendorRepository.GetAllAsync();
        return vendors.Select(MapToResponseDto).ToArray();
    }

    public async Task<VendorResponseDto?> GetByIdAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        return vendor is null ? null : MapToResponseDto(vendor);
    }

    public async Task<VendorResponseDto> CreateAsync(CreateVendorDto vendorDto)
    {
        ValidateVendor(vendorDto.Name, vendorDto.ContactPerson, vendorDto.Email, vendorDto.Phone, vendorDto.Address);
        if (await _vendorRepository.EmailExistsAsync(vendorDto.Email))
        {
            throw new ArgumentException("A vendor with this email already exists.");
        }

        var vendor = new Vendor
        {
            Name = vendorDto.Name.Trim(),
            ContactPerson = vendorDto.ContactPerson.Trim(),
            Email = vendorDto.Email.Trim(),
            Phone = vendorDto.Phone.Trim(),
            Address = vendorDto.Address.Trim(),
            IsActive = vendorDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var createdVendor = await _vendorRepository.AddAsync(vendor);
        return MapToResponseDto(createdVendor);
    }

    public async Task<VendorResponseDto?> UpdateAsync(int id, UpdateVendorDto vendorDto)
    {
        ValidateVendor(vendorDto.Name, vendorDto.ContactPerson, vendorDto.Email, vendorDto.Phone, vendorDto.Address);
        if (await _vendorRepository.EmailExistsAsync(vendorDto.Email, id))
        {
            throw new ArgumentException("A vendor with this email already exists.");
        }

        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor is null || !vendor.IsActive)
        {
            return null;
        }

        vendor.Name = vendorDto.Name.Trim();
        vendor.ContactPerson = vendorDto.ContactPerson.Trim();
        vendor.Email = vendorDto.Email.Trim();
        vendor.Phone = vendorDto.Phone.Trim();
        vendor.Address = vendorDto.Address.Trim();
        vendor.IsActive = vendorDto.IsActive;

        await _vendorRepository.UpdateAsync(vendor);
        return MapToResponseDto(vendor);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor is null || !vendor.IsActive)
        {
            return false;
        }

        await _vendorRepository.DeleteAsync(vendor);
        return true;
    }

    private static VendorResponseDto MapToResponseDto(Vendor vendor)
    {
        return new VendorResponseDto
        {
            Id = vendor.Id,
            Name = vendor.Name,
            ContactPerson = vendor.ContactPerson,
            Email = vendor.Email,
            Phone = vendor.Phone,
            Address = vendor.Address,
            IsActive = vendor.IsActive,
            CreatedAt = vendor.CreatedAt
        };
    }

    private static void ValidateVendor(
        string name,
        string contactPerson,
        string email,
        string phone,
        string address)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(contactPerson) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Vendor name, contact person, email, phone, and address are required.");
        }

        if (!Regex.IsMatch(email.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            throw new ArgumentException("Enter a valid vendor email address.");
        }

        if (!Regex.IsMatch(phone.Trim(), @"^[0-9+\-\s()]{7,20}$"))
        {
            throw new ArgumentException("Enter a valid vendor phone number.");
        }
    }
}
