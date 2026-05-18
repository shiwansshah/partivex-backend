using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class CustomerHistoryService : ICustomerHistoryService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerHistoryService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<CustomerHistoryDto>> GetHistoryAsync(string customerId)
    {
        var customer = await FindCustomerOrThrowAsync(customerId);

        var histories = await _dbContext.CustomerHistories
            .AsNoTracking()
            .Where(history => history.CustomerId == customer.Id)
            .OrderByDescending(history => history.HistoryDate)
            .ToListAsync();

        return histories.Select(MapToDto).ToArray();
    }

    public async Task<CustomerHistoryDto> CreateHistoryAsync(string customerId, CreateCustomerHistoryDto dto)
    {
        var customer = await FindCustomerOrThrowAsync(customerId);
        var normalizedDescription = NormalizeRequiredText(dto.Description, nameof(dto.Description));
        var historyType = ParseHistoryType(dto.HistoryType);
        var paymentStatus = ParsePaymentStatus(dto.PaymentStatus);
        var activityDate = dto.HistoryDate ?? DateTime.UtcNow;

        if (dto.CustomerId is not null && !string.Equals(dto.CustomerId.Trim(), customer.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("CustomerId does not match the selected customer.");
        }

        if (dto.Amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.");
        }

        if (dto.VehicleId.HasValue)
        {
            var vehicle = await _dbContext.Vehicles.AsNoTracking().FirstOrDefaultAsync(item => item.Id == dto.VehicleId.Value);

            if (vehicle is null)
            {
                throw new KeyNotFoundException("Vehicle was not found.");
            }

            if (!string.Equals(vehicle.CustomerId, customer.Id, StringComparison.Ordinal))
            {
                throw new ArgumentException("Vehicle does not belong to the selected customer.");
            }
        }

        var history = new CustomerHistory
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            VehicleId = dto.VehicleId,
            HistoryType = historyType,
            Description = normalizedDescription,
            Amount = dto.Amount,
            PaymentStatus = paymentStatus,
            HistoryDate = activityDate
        };

        _dbContext.CustomerHistories.Add(history);
        await _dbContext.SaveChangesAsync();

        return MapToDto(history);
    }

    private async Task<ApplicationUser> FindCustomerOrThrowAsync(string customerId)
    {
        var normalizedId = NormalizeRequiredText(customerId, nameof(customerId));
        var customer = await _userManager.FindByIdAsync(normalizedId);

        if (customer is null || !await _userManager.IsInRoleAsync(customer, Partivex.Application.Constants.ApplicationRoles.Customer))
        {
            throw new KeyNotFoundException("Customer was not found.");
        }

        return customer;
    }

    private static CustomerHistoryDto MapToDto(CustomerHistory history)
    {
        return new CustomerHistoryDto(
            history.Id,
            history.CustomerId,
            history.VehicleId,
            history.HistoryType.ToString(),
            history.Description,
            history.Amount,
            history.PaymentStatus.ToString(),
            history.HistoryDate);
    }

    private static HistoryType ParseHistoryType(string? value)
    {
        if (!Enum.TryParse(value, true, out HistoryType historyType))
        {
            throw new ArgumentException("History type is invalid.");
        }

        return historyType;
    }

    private static PaymentStatus ParsePaymentStatus(string? value)
    {
        if (!Enum.TryParse(value, true, out PaymentStatus paymentStatus))
        {
            throw new ArgumentException("Payment status is invalid.");
        }

        return paymentStatus;
    }

    private static string NormalizeRequiredText(string? value, string fieldName)
    {
        var normalizedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }

        return normalizedValue;
    }
}