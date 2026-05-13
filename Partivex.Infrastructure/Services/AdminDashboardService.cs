using Microsoft.EntityFrameworkCore;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public AdminDashboardService(
        AppDbContext dbContext,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalInventoryQuantity = await _dbContext.InventoryItems
            .AsNoTracking()
            .SumAsync(item => item.QuantityInStock, cancellationToken);

        var lowStockPartsCount = await _dbContext.InventoryItems
            .AsNoTracking()
            .CountAsync(item => item.QuantityInStock < item.ReorderLevel, cancellationToken);

        await _activityLogService.LogAsync(
            new CreateActivityLogCommand(
                "ViewAdminDashboard",
                "AdminDashboard",
                string.Empty,
                "Admin viewed the dashboard summary."),
            cancellationToken);

        return new DashboardSummaryDto(
            0m,
            totalInventoryQuantity,
            lowStockPartsCount);
    }
}
