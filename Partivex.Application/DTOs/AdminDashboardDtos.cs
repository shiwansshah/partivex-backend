namespace Partivex.Application.DTOs;

public sealed record AdminDashboardSummaryDto(
    int StaffCount,
    int CustomerCount,
    int VehicleCount,
    decimal TotalSales,
    int TotalStockQuantity,
    int LowStockPartsCount,
    IReadOnlyCollection<DashboardChartPointDto> LineGraphData,
    IReadOnlyCollection<DashboardChartPointDto> HistogramData);

public sealed record DashboardChartPointDto(
    string Label,
    decimal Value);
