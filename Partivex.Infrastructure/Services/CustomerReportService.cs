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

        var historyGroups = histories
            .GroupBy(history => history.CustomerId)
            .ToDictionary(group => group.Key, group => group.ToArray());

        return customers
            .Select(customer => BuildReport(customer, historyGroups.TryGetValue(customer.Id, out var customerHistories) ? customerHistories : []))
            .OrderByDescending(report => report.LatestActivityDate)
            .ThenBy(report => report.FullName)
            .ToArray();
    }

    private static CustomerReportDto BuildReport(ApplicationUser customer, IReadOnlyCollection<CustomerHistory> histories)
    {
        var totalAmount = histories.Sum(history => history.Amount);
        var pendingCreditAmount = histories
            .Where(history => history.PaymentStatus == PaymentStatus.Pending)
            .Sum(history => history.Amount);
        var overdueCreditAmount = histories
            .Where(history => history.PaymentStatus == PaymentStatus.Overdue)
            .Sum(history => history.Amount);
        var latestActivityDate = histories.Count == 0
            ? (DateTime?)null
            : histories.Max(history => history.HistoryDate);

        return new CustomerReportDto(
            customer.Id,
            NormalizeText(customer.FullName),
            NormalizeText(customer.Email),
            NormalizeOptionalText(customer.PhoneNumber),
            histories.Count,
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