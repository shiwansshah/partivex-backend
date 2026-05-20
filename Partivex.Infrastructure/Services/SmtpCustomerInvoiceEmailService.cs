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
        var options = await ResolveSmtpOptionsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            return new CustomerPartInvoiceEmailResult(
                "Invoice PDF is ready, but SMTP host is not configured. Set SMTP details from the admin email settings.",
                false);
        }

        if (string.IsNullOrWhiteSpace(options.Sender))
        {
            return new CustomerPartInvoiceEmailResult(
                "Invoice PDF is ready, but no sender email is configured. Set it from the admin email settings.",
                false);
        }

        var result = await SendEmailAsync(
            options,
            email,
            $"Partivex invoice {invoice.InvoiceNumber}",
            $"Dear {invoice.CustomerName},\n\nYour Partivex customer parts invoice is attached as a PDF.\n\nTotal: NPR {invoice.TotalAmount:0.00}\n\nThank you,\nPartivex",
            pdfBytes,
            $"{invoice.InvoiceNumber}.pdf",
            cancellationToken);

        return new CustomerPartInvoiceEmailResult(
            result.EmailSent ? $"Invoice email sent from {options.Sender} with the PDF attached." : result.Message,
            result.EmailSent);
    }

    public async Task<AppointmentInvoiceEmailResult> SendAppointmentInvoiceAsync(
        AppointmentInvoiceDto invoice,
        string email,
        byte[] pdfBytes,
        CancellationToken cancellationToken = default)
    {
        var options = await ResolveSmtpOptionsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            return new AppointmentInvoiceEmailResult(
                "Appointment invoice PDF is ready, but SMTP host is not configured. Set SMTP details from the admin email settings.",
                false);
        }

        if (string.IsNullOrWhiteSpace(options.Sender))
        {
            return new AppointmentInvoiceEmailResult(
                "Appointment invoice PDF is ready, but no sender email is configured. Set it from the admin email settings.",
                false);
        }

        var result = await SendEmailAsync(
            options,
            email,
            $"Partivex appointment invoice {invoice.InvoiceNumber}",
            $"Dear {invoice.CustomerName},\n\nYour Partivex appointment invoice is attached as a PDF.\n\nService: {invoice.ServiceType}\nTotal: NPR {invoice.Amount:0.00}\nPayment status: {invoice.PaymentStatus}\n\nThank you,\nPartivex",
            pdfBytes,
            $"{invoice.InvoiceNumber}.pdf",
            cancellationToken);

        return new AppointmentInvoiceEmailResult(
            result.EmailSent ? $"Appointment invoice email sent from {options.Sender} with the PDF attached." : result.Message,
            result.EmailSent);
    }

    private async Task<SmtpOptions> ResolveSmtpOptionsAsync(CancellationToken cancellationToken)
    {
        var setting = await _dbContext.SmtpSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var username = _configuration["Smtp:Username"];
        return new SmtpOptions(
            NormalizeEmail(setting?.SenderEmail) ?? NormalizeEmail(_configuration["Smtp:From"]) ?? NormalizeEmail(username) ?? string.Empty,
            NormalizeText(setting?.Host) ?? NormalizeText(_configuration["Smtp:Host"]) ?? string.Empty,
            setting?.Port > 0 ? setting.Port : int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587,
            NormalizeText(setting?.Username) ?? NormalizeText(username),
            string.IsNullOrWhiteSpace(setting?.Password) ? _configuration["Smtp:Password"] : setting.Password,
            setting?.EnableSsl ?? (!bool.TryParse(_configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl));
    }

    private async Task<(string Message, bool EmailSent)> SendEmailAsync(
        SmtpOptions options,
        string recipient,
        string subject,
        string body,
        byte[] pdfBytes,
        string attachmentName,
        CancellationToken cancellationToken)
    {
        try
        {
            using var message = new MailMessage(options.Sender, recipient)
            {
                Subject = subject,
                Body = body,
            };
            message.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), attachmentName, "application/pdf"));

            using var client = new SmtpClient(options.Host, options.Port)
            {
                EnableSsl = options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                client.Credentials = new NetworkCredential(options.Username, options.Password);
            }

            await client.SendMailAsync(message, cancellationToken);
            return ("Email sent.", true);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException)
        {
            return ($"Email could not be sent from {options.Sender}: {ex.Message}", false);
        }
    }

    private static string? NormalizeEmail(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record SmtpOptions(
        string Sender,
        string Host,
        int Port,
        string? Username,
        string? Password,
        bool EnableSsl);
}
