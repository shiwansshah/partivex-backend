using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class PartService : IPartService
{
    private readonly IPartRepository _partRepository;

    public PartService(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public Task<IReadOnlyList<PartResponseDto>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<PartResponseDto>>(Array.Empty<PartResponseDto>());
    }

    public Task<PartResponseDto?> GetByIdAsync(int id)
    {
        return Task.FromResult<PartResponseDto?>(null);
    }

    public async Task<PartResponseDto> CreateAsync(CreatePartDto partDto)
    {
        ValidatePart(partDto.Name, partDto.PartCode, partDto.Price, partDto.Stock);
        if (await _partRepository.PartCodeExistsAsync(partDto.PartCode))
        {
            throw new ArgumentException("A part with this code already exists.");
        }

        var part = new Part
        {
            Name = partDto.Name.Trim(),
            PartCode = partDto.PartCode.Trim(),
            Price = partDto.Price,
            Stock = partDto.Stock,
            IsActive = partDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var createdPart = await _partRepository.AddAsync(part);
        return MapToResponseDto(createdPart);
    }

    public async Task<PartResponseDto?> UpdateAsync(int id, UpdatePartDto partDto)
    {
        ValidatePart(partDto.Name, partDto.PartCode, partDto.Price, partDto.Stock);
        if (await _partRepository.PartCodeExistsAsync(partDto.PartCode, id))
        {
            throw new ArgumentException("A part with this code already exists.");
        }

        var part = await _partRepository.GetByIdAsync(id);
        if (part is null || !part.IsActive)
        {
            return null;
        }

        part.Name = partDto.Name.Trim();
        part.PartCode = partDto.PartCode.Trim();
        part.Price = partDto.Price;
        part.Stock = partDto.Stock;
        part.IsActive = partDto.IsActive;

        await _partRepository.UpdateAsync(part);
        return MapToResponseDto(part);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part is null || !part.IsActive)
        {
            return false;
        }

        if (part.Stock > 0)
        {
            throw new ArgumentException("Cannot delete a part that still has stock.");
        }

        await _partRepository.DeleteAsync(part);
        return true;
    }

    private static PartResponseDto MapToResponseDto(Part part)
    {
        return new PartResponseDto
        {
            Id = part.Id,
            Name = part.Name,
            PartCode = part.PartCode,
            Price = part.Price,
            Stock = part.Stock,
            IsActive = part.IsActive,
            CreatedAt = part.CreatedAt
        };
    }

    private static void ValidatePart(string name, string partCode, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(partCode))
        {
            throw new ArgumentException("Part name and part code are required.");
        }

        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        if (stock < 0)
        {
            throw new ArgumentException("Stock cannot be negative.");
        }
    }
}
