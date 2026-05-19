namespace Partivex.Application.DTOs;

public sealed record FinancialReportDto(
    string Period,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    DateTimeOffset GeneratedAt,
    FinancialReportSummaryDto Summary,
    IReadOnlyList<FinancialReportChannelDto> SalesChannels,
    IReadOnlyList<FinancialReportSeriesDto> Series,
    IReadOnlyList<FinancialReportTransactionDto> RecentTransactions);

public sealed record FinancialReportSummaryDto(
    decimal TotalPurchases,
    decimal TotalSales,
    decimal CustomerPartSales,
    decimal AppointmentSales,
    decimal ProfitLoss,
    decimal ProfitMarginPercent,
    decimal OutstandingAppointmentSales,
    int PurchaseInvoiceCount,
    int CustomerPartInvoiceCount,
    int AppointmentInvoiceCount,
    int PaidAppointmentInvoiceCount,
    int PendingAppointmentInvoiceCount);

public sealed record FinancialReportChannelDto(
    string Name,
    decimal Amount,
    int Count,
    decimal SharePercent);

public sealed record FinancialReportSeriesDto(
    string Label,
    DateTimeOffset Start,
    DateTimeOffset End,
    decimal Purchases,
    decimal Sales,
    decimal ProfitLoss);

public sealed record FinancialReportTransactionDto(
    string Type,
    string InvoiceNumber,
    DateTimeOffset Date,
    string Counterparty,
    string Source,
    decimal Amount,
    string Status);
