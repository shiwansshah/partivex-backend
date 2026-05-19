using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class SmtpCustomerInvoiceEmailService : ICustomerInvoiceEmailService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;

    public SmtpCustomerInvoiceEmailService(IConfiguration configuration, AppDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<CustomerPartInvoiceEmailResult> SendInvoiceAsync(
        CustomerPartInvoiceDto invoice,
        string email,
        byte[] pdfBytes,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            return new CustomerPartInvoiceEmailResult(
                "Invoice PDF is ready, but SMTP is not configured for email delivery.",
                false);
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var from = await ResolveSenderEmailAsync(cancellationToken);
        var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;

        using var message = new MailMessage(from, email)
        {
            Subject = $"Partivex invoice {invoice.InvoiceNumber}",
            Body = $"Dear {invoice.CustomerName},\n\nYour Partivex customer parts invoice is attached as a PDF.\n\nTotal: NPR {invoice.TotalAmount:0.00}\n\nThank you,\nPartivex",
        };
        message.Attachments.Add(new Attachment(
            new MemoryStream(pdfBytes),
            $"{invoice.InvoiceNumber}.pdf",
            "application/pdf"));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message, cancellationToken);
        return new CustomerPartInvoiceEmailResult("Invoice email sent with the PDF attached.", true);
    }

    public async Task<AppointmentInvoiceEmailResult> SendAppointmentInvoiceAsync(
        AppointmentInvoiceDto invoice,
        string email,
        byte[] pdfBytes,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            return new AppointmentInvoiceEmailResult(
                "Appointment invoice PDF is ready, but SMTP is not configured for email delivery.",
                false);
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var from = await ResolveSenderEmailAsync(cancellationToken);
        var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;

        using var message = new MailMessage(from, email)
        {
            Subject = $"Partivex appointment invoice {invoice.InvoiceNumber}",
            Body = $"Dear {invoice.CustomerName},\n\nYour Partivex appointment invoice is attached as a PDF.\n\nService: {invoice.ServiceType}\nTotal: NPR {invoice.Amount:0.00}\nPayment status: {invoice.PaymentStatus}\n\nThank you,\nPartivex",
        };
        message.Attachments.Add(new Attachment(
            new MemoryStream(pdfBytes),
            $"{invoice.InvoiceNumber}.pdf",
            "application/pdf"));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message, cancellationToken);
        return new AppointmentInvoiceEmailResult("Appointment invoice email sent with the PDF attached.", true);
    }

    private async Task<string> ResolveSenderEmailAsync(CancellationToken cancellationToken)
    {
        var dynamicSender = await _dbContext.SmtpSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Id)
            .Select(setting => setting.SenderEmail)
            .FirstOrDefaultAsync(cancellationToken);

        var username = _configuration["Smtp:Username"];
        return dynamicSender ?? _configuration["Smtp:From"] ?? username ?? "no-reply@partivex.local";
    }
}
