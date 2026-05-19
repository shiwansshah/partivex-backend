using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class StaffFeatureAccessService : IStaffFeatureAccessService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffFeatureAccessService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<StaffFeatureAccessSummaryDto> GetFeatureAccessAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureStaffUserAsync(userId);

        var accessRows = await _dbContext.StaffFeatureAccesses
            .AsNoTracking()
            .Where(access => access.UserId == userId)
            .ToArrayAsync(cancellationToken);

        return BuildSummary(userId, accessRows);
    }

    public async Task<StaffFeatureAccessSummaryDto> UpdateFeatureAccessAsync(
        string userId,
        UpdateStaffFeatureAccessDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureStaffUserAsync(userId);

        var enabledFeatureKeys = dto.EnabledFeatureKeys.Distinct().ToHashSet();
        var unknownFeatureKeys = enabledFeatureKeys
            .Where(featureKey => !StaffFeatureKeys.IsKnown(featureKey))
            .ToArray();

        if (unknownFeatureKeys.Length > 0)
        {
            throw new ArgumentException($"Unknown staff feature key: {unknownFeatureKeys[0]}.");
        }

        var existingRows = await _dbContext.StaffFeatureAccesses
            .Where(access => access.UserId == userId)
            .ToArrayAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var feature in StaffFeatureKeys.All)
        {
            var existing = existingRows.FirstOrDefault(access => access.FeatureKey == feature.Key);
            var enabled = enabledFeatureKeys.Contains(feature.Key);

            if (existing is null)
            {
                await _dbContext.StaffFeatureAccesses.AddAsync(
                    new StaffFeatureAccess
                    {
                        UserId = userId,
                        FeatureKey = feature.Key,
                        IsEnabled = enabled,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    cancellationToken);

                continue;
            }

            existing.IsEnabled = enabled;
            existing.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedRows = await _dbContext.StaffFeatureAccesses
            .AsNoTracking()
            .Where(access => access.UserId == userId)
            .ToArrayAsync(cancellationToken);

        return BuildSummary(userId, updatedRows);
    }

    private async Task EnsureStaffUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Staff user id is required.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !await _userManager.IsInRoleAsync(user, ApplicationRoles.Staff))
        {
            throw new KeyNotFoundException("Staff user was not found.");
        }
    }

    private static StaffFeatureAccessSummaryDto BuildSummary(
        string userId,
        IReadOnlyCollection<StaffFeatureAccess> accessRows)
    {
        var accessByFeature = accessRows.ToDictionary(access => access.FeatureKey);
        var features = StaffFeatureKeys.All
            .Select(feature => new StaffFeatureAccessDto(
                feature.Key,
                feature.DisplayName,
                accessByFeature.TryGetValue(feature.Key, out var access) && access.IsEnabled))
            .ToArray();

        return new StaffFeatureAccessSummaryDto(userId, features);
    }
}
