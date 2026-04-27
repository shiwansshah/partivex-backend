using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

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
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor is null)
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
        if (vendor is null)
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
}
