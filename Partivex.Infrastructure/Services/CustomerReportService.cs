using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class CustomerReportService : ICustomerReportService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerReportService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<CustomerReportDto>> GetRegularCustomersAsync()
    {
        var reports = await BuildReportsAsync();
        return reports.Where(report => report.TotalHistoryCount >= 2).ToArray();
    }

    public async Task<IReadOnlyList<CustomerReportDto>> GetHighSpendersAsync()
    {
        var reports = await BuildReportsAsync();
        return reports.Where(report => report.TotalAmount >= 5000m).ToArray();
    }

    public async Task<IReadOnlyList<CustomerReportDto>> GetCreditCustomersAsync()
    {
        var reports = await BuildReportsAsync();
        return reports.Where(report => report.PendingCreditAmount > 0m || report.OverdueCreditAmount > 0m).ToArray();
    }

    private async Task<IReadOnlyList<CustomerReportDto>> BuildReportsAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Customer);
        var histories = await _dbContext.CustomerHistories
            .AsNoTracking()
            .ToListAsync();
        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .ToListAsync();
        var appointmentInvoices = await _dbContext.AppointmentInvoices
            .AsNoTracking()
            .ToListAsync();
        var partPurchases = await _dbContext.CustomerPartPurchaseInvoices
            .AsNoTracking()
            .ToListAsync();
        var partRequests = await _dbContext.PartRequests
            .AsNoTracking()
            .ToListAsync();
        var reviews = await _dbContext.Reviews
            .AsNoTracking()
            .ToListAsync();

        var historyGroups = histories
            .GroupBy(history => history.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var appointmentGroups = appointments
            .GroupBy(appointment => appointment.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var appointmentInvoiceGroups = appointmentInvoices
            .GroupBy(invoice => invoice.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var partPurchaseGroups = partPurchases
            .GroupBy(invoice => invoice.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var partRequestGroups = partRequests
            .GroupBy(request => request.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var reviewGroups = reviews
            .GroupBy(review => review.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());

        return customers
            .Select(customer => BuildReport(
                customer,
                historyGroups.TryGetValue(customer.Id, out var customerHistories) ? customerHistories : [],
                appointmentGroups.TryGetValue(customer.Id, out var customerAppointments) ? customerAppointments : [],
                appointmentInvoiceGroups.TryGetValue(customer.Id, out var customerAppointmentInvoices) ? customerAppointmentInvoices : [],
                partPurchaseGroups.TryGetValue(customer.Id, out var customerPartPurchases) ? customerPartPurchases : [],
                partRequestGroups.TryGetValue(customer.Id, out var customerPartRequests) ? customerPartRequests : [],
                reviewGroups.TryGetValue(customer.Id, out var customerReviews) ? customerReviews : []))
            .OrderByDescending(report => report.LatestActivityDate)
            .ThenBy(report => report.FullName)
            .ToArray();
    }

    private static CustomerReportDto BuildReport(
        ApplicationUser customer,
        IReadOnlyCollection<CustomerHistory> histories,
        IReadOnlyCollection<Appointment> appointments,
        IReadOnlyCollection<AppointmentInvoice> appointmentInvoices,
        IReadOnlyCollection<CustomerPartPurchaseInvoice> partPurchases,
        IReadOnlyCollection<PartRequest> partRequests,
        IReadOnlyCollection<Review> reviews)
    {
        var activeAppointments = appointments
            .Where(appointment => appointment.Status is not (AppointmentStatus.Cancelled or AppointmentStatus.Rejected))
            .ToArray();
        var activePartRequests = partRequests
            .Where(request => request.Status is not (PartRequestStatus.Cancelled or PartRequestStatus.Rejected))
            .ToArray();
        var manualHistoryCount = histories.Count;
        var appointmentCount = activeAppointments.Length;
        var partPurchaseCount = partPurchases.Count;
        var partRequestCount = activePartRequests.Length;
        var reviewCount = reviews.Count;
        var totalActivityCount = manualHistoryCount + appointmentCount + partPurchaseCount + partRequestCount + reviewCount;
        var overdueCutoff = DateTimeOffset.UtcNow.AddMonths(-1);

        var totalAmount = histories.Sum(history => history.Amount)
            + appointmentInvoices.Sum(invoice => invoice.Amount)
            + partPurchases.Sum(invoice => invoice.TotalAmount);

        var pendingCreditAmount = histories
            .Where(history => history.PaymentStatus == PaymentStatus.Pending)
            .Sum(history => history.Amount)
            + appointmentInvoices
                .Where(invoice => string.Equals(invoice.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase)
                    && invoice.InvoiceDate > overdueCutoff)
                .Sum(invoice => invoice.Amount);

        var overdueCreditAmount = histories
            .Where(history => history.PaymentStatus == PaymentStatus.Overdue)
            .Sum(history => history.Amount)
            + appointmentInvoices
                .Where(invoice => string.Equals(invoice.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase)
                    && invoice.InvoiceDate <= overdueCutoff)
                .Sum(invoice => invoice.Amount);

        var activityDates = histories.Select(history => history.HistoryDate)
            .Concat(activeAppointments.Select(appointment => appointment.UpdatedAt.UtcDateTime))
            .Concat(appointmentInvoices.Select(invoice => invoice.CreatedAt.UtcDateTime))
            .Concat(partPurchases.Select(invoice => invoice.CreatedAt.UtcDateTime))
            .Concat(activePartRequests.Select(request => request.UpdatedAt.UtcDateTime))
            .Concat(reviews.Select(review => review.UpdatedAt.UtcDateTime))
            .ToArray();

        var latestActivityDate = activityDates.Length == 0
            ? (DateTime?)null
            : activityDates.Max();

        return new CustomerReportDto(
            customer.Id,
            NormalizeText(customer.FullName),
            NormalizeText(customer.Email),
            NormalizeOptionalText(customer.PhoneNumber),
            totalActivityCount,
            manualHistoryCount,
            appointmentCount,
            partPurchaseCount,
            partRequestCount,
            reviewCount,
            totalAmount,
            pendingCreditAmount,
            overdueCreditAmount,
            latestActivityDate);
    }

    private static string NormalizeText(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalizedValue = value?.Trim();
        return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue;
    }
}
