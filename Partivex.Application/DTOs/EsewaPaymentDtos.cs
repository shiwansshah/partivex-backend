namespace Partivex.Application.DTOs;

public sealed record PaymentTargetDto(
    string Type,
    int Id,
    string InvoiceNumber,
    decimal Amount,
    string Status);

public sealed record EsewaPaymentInitiationDto(
    string Provider,
    string Environment,
    string Method,
    string ActionUrl,
    IReadOnlyDictionary<string, string> Fields,
    PaymentTargetDto Target);

public sealed record EsewaPaymentCallbackResult(
    bool Succeeded,
    string Status,
    string Message,
    string RedirectUrl,
    string? InvoiceType = null,
    int? InvoiceId = null,
    string? InvoiceNumber = null,
    string? TransactionCode = null,
    string? ReferenceId = null);
