namespace Partivex.Application.DTOs;

public sealed record DashboardSummaryDto(
    decimal TotalSales,
    int TotalInventoryQuantity,
    int LowStockPartsCount);
