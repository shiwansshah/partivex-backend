using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class FinancialReportService : IFinancialReportService
{
    private const string DailyPeriod = "daily";
    private const string MonthlyPeriod = "monthly";
    private const string YearlyPeriod = "yearly";
    private const int RecentTransactionLimit = 12;

    private readonly IFinancialReportRepository _reportRepository;

    public FinancialReportService(IFinancialReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<FinancialReportDto> GetReportAsync(
        string period,
        DateOnly referenceDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var (periodStart, periodEnd) = GetRange(normalizedPeriod, referenceDate);

        var purchases = await _reportRepository.GetPurchaseInvoicesAsync(periodStart, periodEnd, cancellationToken);
        var partSales = await _reportRepository.GetCustomerPartPurchaseInvoicesAsync(periodStart, periodEnd, cancellationToken);
        var appointmentSales = await _reportRepository.GetAppointmentInvoicesAsync(periodStart, periodEnd, cancellationToken);
        var recognizedPartSales = partSales
            .Where(invoice => SalesRecognitionRules.IsRecognized(invoice.Status))
            .ToArray();
        var recognizedAppointmentSales = appointmentSales
            .Where(invoice => SalesRecognitionRules.IsRecognized(invoice.PaymentStatus))
            .ToArray();

        var totalPurchases = purchases.Sum(GetPurchaseTotal);
        var customerPartSales = recognizedPartSales.Sum(invoice => invoice.TotalAmount);
        var appointmentSalesTotal = recognizedAppointmentSales.Sum(invoice => invoice.Amount);
        var totalSales = customerPartSales + appointmentSalesTotal;
        var profitLoss = totalSales - totalPurchases;
        var paidAppointmentCount = recognizedAppointmentSales.Length;
        var pendingAppointmentCount = appointmentSales.Count(invoice => !IsRecognizedSale(invoice.PaymentStatus));
        var outstandingAppointmentSales = appointmentSales
            .Where(invoice => !IsRecognizedSale(invoice.PaymentStatus))
            .Sum(invoice => invoice.Amount);

        var summary = new FinancialReportSummaryDto(
            totalPurchases,
            totalSales,
            customerPartSales,
            appointmentSalesTotal,
            profitLoss,
            totalSales == 0 ? 0 : Math.Round(profitLoss / totalSales * 100, 2),
            outstandingAppointmentSales,
            purchases.Count,
            recognizedPartSales.Length,
            appointmentSales.Count,
            paidAppointmentCount,
            pendingAppointmentCount);

        return new FinancialReportDto(
            normalizedPeriod,
            periodStart,
            periodEnd,
            DateTimeOffset.UtcNow,
            summary,
            BuildSalesChannels(customerPartSales, recognizedPartSales.Length, appointmentSalesTotal, recognizedAppointmentSales.Length, totalSales),
            BuildSeries(normalizedPeriod, periodStart, periodEnd, purchases, recognizedPartSales, recognizedAppointmentSales),
            BuildRecentTransactions(purchases, recognizedPartSales, recognizedAppointmentSales));
    }

    private static string NormalizePeriod(string period)
    {
        var normalized = string.IsNullOrWhiteSpace(period)
            ? MonthlyPeriod
            : period.Trim().ToLowerInvariant();

        return normalized switch
        {
            DailyPeriod or MonthlyPeriod or YearlyPeriod => normalized,
            _ => MonthlyPeriod
        };
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetRange(string period, DateOnly referenceDate)
    {
        var startDate = period switch
        {
            DailyPeriod => referenceDate,
            MonthlyPeriod => new DateOnly(referenceDate.Year, referenceDate.Month, 1),
            YearlyPeriod => new DateOnly(referenceDate.Year, 1, 1),
            _ => new DateOnly(referenceDate.Year, referenceDate.Month, 1)
        };

        var endDate = period switch
        {
            DailyPeriod => startDate.AddDays(1),
            MonthlyPeriod => startDate.AddMonths(1),
            YearlyPeriod => startDate.AddYears(1),
            _ => startDate.AddMonths(1)
        };

        return (
            new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(endDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
    }

    private static IReadOnlyList<FinancialReportChannelDto> BuildSalesChannels(
        decimal customerPartSales,
        int customerPartCount,
        decimal appointmentSales,
        int appointmentCount,
        decimal totalSales)
    {
        return
        [
            new FinancialReportChannelDto(
                "Customer part sales",
                customerPartSales,
                customerPartCount,
                GetShare(customerPartSales, totalSales)),
            new FinancialReportChannelDto(
                "Appointment service sales",
                appointmentSales,
                appointmentCount,
                GetShare(appointmentSales, totalSales))
        ];
    }

    private static IReadOnlyList<FinancialReportSeriesDto> BuildSeries(
        string period,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        IReadOnlyList<PurchaseInvoice> purchases,
        IReadOnlyList<CustomerPartPurchaseInvoice> partSales,
        IReadOnlyList<AppointmentInvoice> appointmentSales)
    {
        var buckets = BuildBuckets(period, periodStart, periodEnd);

        return buckets
            .Select(bucket =>
            {
                var bucketPurchases = purchases
                    .Where(invoice => IsWithin(invoice.InvoiceDate, bucket.Start, bucket.End))
                    .Sum(GetPurchaseTotal);
                var bucketPartSales = partSales
                    .Where(invoice => IsWithin(invoice.InvoiceDate, bucket.Start, bucket.End))
                    .Sum(invoice => invoice.TotalAmount);
                var bucketAppointmentSales = appointmentSales
                    .Where(invoice => IsWithin(invoice.InvoiceDate, bucket.Start, bucket.End))
                    .Sum(invoice => invoice.Amount);
                var bucketSales = bucketPartSales + bucketAppointmentSales;

                return new FinancialReportSeriesDto(
                    bucket.Label,
                    bucket.Start,
                    bucket.End,
                    bucketPurchases,
                    bucketSales,
                    bucketSales - bucketPurchases);
            })
            .ToArray();
    }

    private static IReadOnlyList<(string Label, DateTimeOffset Start, DateTimeOffset End)> BuildBuckets(
        string period,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd)
    {
        if (period == DailyPeriod)
        {
            return Enumerable.Range(0, 24)
                .Select(hour =>
                {
                    var start = periodStart.AddHours(hour);
                    return ($"{hour:00}:00", start, start.AddHours(1));
                })
                .ToArray();
        }

        if (period == MonthlyPeriod)
        {
            var days = (periodEnd - periodStart).Days;
            return Enumerable.Range(0, days)
                .Select(day =>
                {
                    var start = periodStart.AddDays(day);
                    return (start.ToString("MMM d"), start, start.AddDays(1));
                })
                .ToArray();
        }

        return Enumerable.Range(0, 12)
            .Select(month =>
            {
                var start = periodStart.AddMonths(month);
                return (start.ToString("MMM"), start, start.AddMonths(1));
            })
            .ToArray();
    }

    private static IReadOnlyList<FinancialReportTransactionDto> BuildRecentTransactions(
        IReadOnlyList<PurchaseInvoice> purchases,
        IReadOnlyList<CustomerPartPurchaseInvoice> partSales,
        IReadOnlyList<AppointmentInvoice> appointmentSales)
    {
        var purchaseTransactions = purchases.Select(invoice => new FinancialReportTransactionDto(
            "Purchase",
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.VendorName,
            "Inventory stock update",
            GetPurchaseTotal(invoice),
            invoice.Status));

        var partSaleTransactions = partSales.Select(invoice => new FinancialReportTransactionDto(
            "Sale",
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.CustomerName,
            invoice.Source,
            invoice.TotalAmount,
            invoice.Status));

        var appointmentTransactions = appointmentSales.Select(invoice => new FinancialReportTransactionDto(
            "Sale",
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.CustomerName,
            $"Appointment - {invoice.ServiceType}",
            invoice.Amount,
            invoice.PaymentStatus));

        return purchaseTransactions
            .Concat(partSaleTransactions)
            .Concat(appointmentTransactions)
            .OrderByDescending(transaction => transaction.Date)
            .Take(RecentTransactionLimit)
            .ToArray();
    }

    private static decimal GetPurchaseTotal(PurchaseInvoice invoice)
    {
        return invoice.Items.Sum(item => item.Quantity * item.UnitPrice);
    }

    private static bool IsWithin(DateTimeOffset value, DateTimeOffset start, DateTimeOffset end)
    {
        return value >= start && value < end;
    }

    private static bool IsRecognizedSale(string? status)
    {
        return SalesRecognitionRules.IsRecognized(status);
    }

    private static decimal GetShare(decimal amount, decimal total)
    {
        return total == 0 ? 0 : Math.Round(amount / total * 100, 2);
    }
}
