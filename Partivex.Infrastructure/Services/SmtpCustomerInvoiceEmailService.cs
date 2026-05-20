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

        var sender = await ResolveSenderEmailAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(sender))
        {
            return new CustomerPartInvoiceEmailResult(
                "Invoice PDF is ready, but no sender email is configured. Set it from the admin email settings.",
                false);
        }

        var result = await SendEmailAsync(
            sender,
            email,
            $"Partivex invoice {invoice.InvoiceNumber}",
            $"Dear {invoice.CustomerName},\n\nYour Partivex customer parts invoice is attached as a PDF.\n\nTotal: NPR {invoice.TotalAmount:0.00}\n\nThank you,\nPartivex",
            pdfBytes,
            $"{invoice.InvoiceNumber}.pdf",
            cancellationToken);

        return new CustomerPartInvoiceEmailResult(
            result.EmailSent ? $"Invoice email sent from {sender} with the PDF attached." : result.Message,
            result.EmailSent);
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

        var sender = await ResolveSenderEmailAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(sender))
        {
            return new AppointmentInvoiceEmailResult(
                "Appointment invoice PDF is ready, but no sender email is configured. Set it from the admin email settings.",
                false);
        }

        var result = await SendEmailAsync(
            sender,
            email,
            $"Partivex appointment invoice {invoice.InvoiceNumber}",
            $"Dear {invoice.CustomerName},\n\nYour Partivex appointment invoice is attached as a PDF.\n\nService: {invoice.ServiceType}\nTotal: NPR {invoice.Amount:0.00}\nPayment status: {invoice.PaymentStatus}\n\nThank you,\nPartivex",
            pdfBytes,
            $"{invoice.InvoiceNumber}.pdf",
            cancellationToken);

        return new AppointmentInvoiceEmailResult(
            result.EmailSent ? $"Appointment invoice email sent from {sender} with the PDF attached." : result.Message,
            result.EmailSent);
    }

    private async Task<string> ResolveSenderEmailAsync(CancellationToken cancellationToken)
    {
        var dynamicSender = await _dbContext.SmtpSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Id)
            .Select(setting => setting.SenderEmail)
            .FirstOrDefaultAsync(cancellationToken);

        var username = _configuration["Smtp:Username"];
        return NormalizeEmail(dynamicSender)
            ?? NormalizeEmail(_configuration["Smtp:From"])
            ?? NormalizeEmail(username)
            ?? string.Empty;
    }

    private async Task<(string Message, bool EmailSent)> SendEmailAsync(
        string sender,
        string recipient,
        string subject,
        string body,
        byte[] pdfBytes,
        string attachmentName,
        CancellationToken cancellationToken)
    {
        try
        {
            var host = _configuration["Smtp:Host"]!;
            var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;

            using var message = new MailMessage(sender, recipient)
            {
                Subject = subject,
                Body = body,
            };
            message.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), attachmentName, "application/pdf"));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            await client.SendMailAsync(message, cancellationToken);
            return ("Email sent.", true);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException)
        {
            return ($"Email could not be sent from {sender}: {ex.Message}", false);
        }
    }

    private static string? NormalizeEmail(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
