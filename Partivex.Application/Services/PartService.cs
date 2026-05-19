using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Partivex.Application.Services;

public sealed class PartService : IPartService
{
    private readonly IPartRepository _partRepository;
    private readonly IFileStorageService _fileStorageService;

    public PartService(IPartRepository partRepository, IFileStorageService fileStorageService)
    {
        _partRepository = partRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<PartResponseDto>> GetAllAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = "name",
        string? sortDirection = "asc")
    {
        var parts = await _partRepository.GetAllAsync(searchTerm, sortBy, sortDirection, pageNumber, pageSize);
        return parts.Select(MapToResponseDto).ToArray();
    }

    public async Task<PartResponseDto?> GetByIdAsync(int id)
    {
        var part = await _partRepository.GetByIdAsync(id);
        return part is null ? null : MapToResponseDto(part);
    }

    public async Task<PartResponseDto> CreateAsync(CreatePartDto partDto, IFormFile? image = null)
    {
        ValidatePart(partDto.Name, partDto.PartCode, partDto.Category, partDto.UnitPrice, partDto.MinimumStockLevel, partDto.CurrentStock);
        if (await _partRepository.PartCodeExistsAsync(partDto.PartCode))
        {
            throw new ArgumentException("A part with this code already exists.");
        }

        var imageUrl = image is null
            ? partDto.ImageUrl.Trim()
            : await _fileStorageService.SaveFileAsync(image, "parts");

        var part = new Part
        {
            Name = partDto.Name.Trim(),
            PartCode = partDto.PartCode.Trim(),
            Category = partDto.Category.Trim(),
            CompatibleVehicle = partDto.CompatibleVehicle.Trim(),
            UnitPrice = partDto.UnitPrice,
            MinimumStockLevel = partDto.MinimumStockLevel,
            CurrentStock = partDto.CurrentStock,
            ImageUrl = imageUrl,
            IsActive = partDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdPart = await _partRepository.AddAsync(part);
        return MapToResponseDto(createdPart);
    }

    public async Task<PartResponseDto?> UpdateAsync(int id, UpdatePartDto partDto, IFormFile? image = null)
    {
        ValidatePart(partDto.Name, partDto.PartCode, partDto.Category, partDto.UnitPrice, partDto.MinimumStockLevel, partDto.CurrentStock);
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
        part.Category = partDto.Category.Trim();
        part.CompatibleVehicle = partDto.CompatibleVehicle.Trim();
        part.UnitPrice = partDto.UnitPrice;
        part.MinimumStockLevel = partDto.MinimumStockLevel;
        part.CurrentStock = partDto.CurrentStock;
        if (image is not null)
        {
            _fileStorageService.DeleteFile(part.ImageUrl);
            part.ImageUrl = await _fileStorageService.SaveFileAsync(image, "parts");
        }
        else if (!string.IsNullOrWhiteSpace(partDto.ImageUrl))
        {
            part.ImageUrl = partDto.ImageUrl.Trim();
        }
        part.IsActive = partDto.IsActive;
        part.UpdatedAt = DateTime.UtcNow;

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

        if (part.CurrentStock > 0)
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
            Category = part.Category,
            CompatibleVehicle = part.CompatibleVehicle,
            UnitPrice = part.UnitPrice,
            MinimumStockLevel = part.MinimumStockLevel,
            CurrentStock = part.CurrentStock,
            StockStatus = GetStockStatus(part.CurrentStock, part.MinimumStockLevel),
            ImageUrl = part.ImageUrl,
            IsActive = part.IsActive,
            CreatedAt = part.CreatedAt
        };
    }

    private static void ValidatePart(string name, string partCode, string category, decimal unitPrice, int minimumStockLevel, int currentStock)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(partCode) || string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Part name, part code, and category are required.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        if (minimumStockLevel < 0 || currentStock < 0)
        {
            throw new ArgumentException("Stock values cannot be negative.");
        }
    }

    private static string GetStockStatus(int currentStock, int minimumStockLevel)
    {
        if (currentStock == 0) return "Out of Stock";
        return currentStock <= minimumStockLevel ? "Low Stock" : "In Stock";
    }
}
