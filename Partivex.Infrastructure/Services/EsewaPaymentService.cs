using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Services;

public sealed class EsewaPaymentService : IEsewaPaymentService
{
    private const string ProviderName = "eSewa";
    private const string SignedFieldNames = "total_amount,transaction_uuid,product_code";
    private readonly HttpClient _httpClient;
    private readonly ICustomerPartPurchaseService _partPurchaseService;
    private readonly ICustomerPartPurchaseRepository _partInvoiceRepository;
    private readonly IAppointmentInvoiceRepository _appointmentInvoiceRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EsewaPaymentOptions _options;

    public EsewaPaymentService(
        HttpClient httpClient,
        ICustomerPartPurchaseService partPurchaseService,
        ICustomerPartPurchaseRepository partInvoiceRepository,
        IAppointmentInvoiceRepository appointmentInvoiceRepository,
        IHttpContextAccessor httpContextAccessor,
        IOptions<EsewaPaymentOptions> options)
    {
        _httpClient = httpClient;
        _partPurchaseService = partPurchaseService;
        _partInvoiceRepository = partInvoiceRepository;
        _appointmentInvoiceRepository = appointmentInvoiceRepository;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public async Task<CustomerPartPurchaseResult<EsewaPaymentInitiationDto>> CreatePartCheckoutPaymentAsync(
        string customerId,
        CustomerPartCheckoutRequest request,
        string source,
        CancellationToken cancellationToken = default)
    {
        var checkoutResult = await _partPurchaseService.CheckoutAsync(customerId, request, source, cancellationToken);
        if (!checkoutResult.Succeeded)
        {
            return checkoutResult.IsNotFound
                ? CustomerPartPurchaseResult<EsewaPaymentInitiationDto>.NotFound(checkoutResult.Message ?? "Customer part purchase invoice not found.")
                : CustomerPartPurchaseResult<EsewaPaymentInitiationDto>.Failed(checkoutResult.Errors, checkoutResult.Message);
        }

        return CustomerPartPurchaseResult<EsewaPaymentInitiationDto>.Success(BuildPartPayment(checkoutResult.Value!));
    }

    public async Task<CustomerPartPurchaseResult<EsewaPaymentInitiationDto>> CreatePartInvoicePaymentAsync(
        int invoiceId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _partInvoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null || invoice.CustomerId != customerId)
        {
            return CustomerPartPurchaseResult<EsewaPaymentInitiationDto>.NotFound("Customer part purchase invoice not found.");
        }

        if (string.Equals(invoice.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return CustomerPartPurchaseResult<EsewaPaymentInitiationDto>.Failed(
            [
                new CustomerPartPurchaseError(nameof(invoice.Status), "This invoice is already paid.")
            ],
            "This invoice is already paid.");
        }

        return CustomerPartPurchaseResult<EsewaPaymentInitiationDto>.Success(BuildPartPayment(invoice));
    }

    public async Task<CustomerPortalResult<EsewaPaymentInitiationDto>> CreateAppointmentInvoicePaymentAsync(
        int invoiceId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _appointmentInvoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null || invoice.CustomerId != customerId)
        {
            return CustomerPortalResult<EsewaPaymentInitiationDto>.NotFound("Appointment invoice not found.");
        }

        if (string.Equals(invoice.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return CustomerPortalResult<EsewaPaymentInitiationDto>.Failed(
            [
                new CustomerPortalError(nameof(invoice.PaymentStatus), "This invoice is already paid.")
            ],
            "This invoice is already paid.");
        }

        return CustomerPortalResult<EsewaPaymentInitiationDto>.Success(BuildAppointmentPayment(invoice));
    }

    public async Task<EsewaPaymentCallbackResult> CompletePaymentAsync(string encodedData, CancellationToken cancellationToken = default)
    {
        var payload = DecodePayload(encodedData);
        if (payload is null)
        {
            return CreateCallbackResult(false, "failed", "Unable to read the eSewa payment response.");
        }

        if (!VerifyResponseSignature(payload))
        {
            return CreateCallbackResult(false, "failed", "eSewa response signature could not be verified.", payload: payload);
        }

        var status = payload.Get("status");
        if (!string.Equals(status, "COMPLETE", StringComparison.OrdinalIgnoreCase))
        {
            return CreateCallbackResult(false, "failed", $"eSewa returned payment status {status ?? "UNKNOWN"}.", payload: payload);
        }

        var transactionUuid = payload.Get("transaction_uuid");
        if (!TryReadTarget(transactionUuid, out var targetType, out var targetId))
        {
            return CreateCallbackResult(false, "failed", "Payment target could not be matched to an invoice.", payload: payload);
        }

        var totalAmount = ParseAmount(payload.Get("total_amount"));
        if (!totalAmount.HasValue)
        {
            return CreateCallbackResult(false, "failed", "eSewa response did not include a valid amount.", payload: payload);
        }

        var productCode = payload.Get("product_code");
        if (!string.Equals(productCode, _options.ProductCode, StringComparison.Ordinal))
        {
            return CreateCallbackResult(false, "failed", "eSewa product code did not match this merchant configuration.", payload: payload);
        }

        var statusCheck = await CheckTransactionStatusAsync(transactionUuid!, totalAmount.Value, cancellationToken);
        if (!string.Equals(statusCheck.Status, "COMPLETE", StringComparison.OrdinalIgnoreCase))
        {
            return CreateCallbackResult(
                false,
                "failed",
                statusCheck.Message ?? $"eSewa verification returned {statusCheck.Status ?? "UNKNOWN"}.",
                payload: payload);
        }

        if (string.Equals(targetType, "parts", StringComparison.OrdinalIgnoreCase))
        {
            return await CompletePartInvoiceAsync(targetId, totalAmount.Value, payload, statusCheck, cancellationToken);
        }

        if (string.Equals(targetType, "appointment", StringComparison.OrdinalIgnoreCase))
        {
            return await CompleteAppointmentInvoiceAsync(targetId, totalAmount.Value, payload, statusCheck, cancellationToken);
        }

        return CreateCallbackResult(false, "failed", "Payment target type is not supported.", payload: payload);
    }

    public Task<EsewaPaymentCallbackResult> FailPaymentAsync(string? encodedData, CancellationToken cancellationToken = default)
    {
        var payload = string.IsNullOrWhiteSpace(encodedData) ? null : DecodePayload(encodedData);
        var message = payload?.Get("status") is { Length: > 0 } status
            ? $"eSewa returned payment status {status}."
            : "eSewa payment was cancelled or could not be completed.";

        return Task.FromResult(CreateCallbackResult(false, "failed", message, payload: payload));
    }

    private EsewaPaymentInitiationDto BuildPartPayment(CustomerPartInvoiceDto invoice)
    {
        return BuildPayment("parts", invoice.Id, invoice.InvoiceNumber, invoice.TotalAmount, invoice.Status);
    }

    private EsewaPaymentInitiationDto BuildPartPayment(CustomerPartPurchaseInvoice invoice)
    {
        return BuildPayment("parts", invoice.Id, invoice.InvoiceNumber, invoice.TotalAmount, invoice.Status);
    }

    private EsewaPaymentInitiationDto BuildAppointmentPayment(AppointmentInvoice invoice)
    {
        return BuildPayment("appointment", invoice.Id, invoice.InvoiceNumber, invoice.Amount, invoice.PaymentStatus);
    }

    private EsewaPaymentInitiationDto BuildPayment(string targetType, int invoiceId, string invoiceNumber, decimal amount, string status)
    {
        var amountText = FormatAmount(amount);
        var transactionUuid = $"{targetType}-{invoiceId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var fields = new Dictionary<string, string>
        {
            ["amount"] = amountText,
            ["tax_amount"] = "0",
            ["total_amount"] = amountText,
            ["transaction_uuid"] = transactionUuid,
            ["product_code"] = _options.ProductCode,
            ["product_service_charge"] = "0",
            ["product_delivery_charge"] = "0",
            ["success_url"] = BuildApiCallbackUrl("success"),
            ["failure_url"] = BuildApiCallbackUrl("failure"),
            ["signed_field_names"] = SignedFieldNames
        };
        fields["signature"] = Sign(BuildSignedMessage(fields, SignedFieldNames));

        return new EsewaPaymentInitiationDto(
            ProviderName,
            _options.Environment,
            "POST",
            _options.PaymentUrl,
            fields,
            new PaymentTargetDto(targetType, invoiceId, invoiceNumber, amount, status));
    }

    private async Task<EsewaPaymentCallbackResult> CompletePartInvoiceAsync(
        int invoiceId,
        decimal paidAmount,
        EsewaPayload payload,
        EsewaStatusCheckResult statusCheck,
        CancellationToken cancellationToken)
    {
        var invoice = await _partInvoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return CreateCallbackResult(false, "failed", "Customer part purchase invoice not found.", payload: payload);
        }

        if (!AmountsMatch(invoice.TotalAmount, paidAmount))
        {
            return CreateCallbackResult(false, "failed", "Paid amount does not match the customer part invoice total.", "parts", invoice.Id, invoice.InvoiceNumber, payload);
        }

        invoice.Status = "Paid";
        await _partInvoiceRepository.SaveChangesAsync(cancellationToken);

        return CreateCallbackResult(
            true,
            "success",
            "eSewa payment completed for your parts invoice.",
            "parts",
            invoice.Id,
            invoice.InvoiceNumber,
            payload,
            statusCheck.RefId);
    }

    private async Task<EsewaPaymentCallbackResult> CompleteAppointmentInvoiceAsync(
        int invoiceId,
        decimal paidAmount,
        EsewaPayload payload,
        EsewaStatusCheckResult statusCheck,
        CancellationToken cancellationToken)
    {
        var invoice = await _appointmentInvoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            return CreateCallbackResult(false, "failed", "Appointment invoice not found.", payload: payload);
        }

        if (!AmountsMatch(invoice.Amount, paidAmount))
        {
            return CreateCallbackResult(false, "failed", "Paid amount does not match the appointment invoice total.", "appointment", invoice.Id, invoice.InvoiceNumber, payload);
        }

        invoice.PaymentStatus = "Paid";
        invoice.PaidAt = DateTimeOffset.UtcNow;
        await _appointmentInvoiceRepository.SaveChangesAsync(cancellationToken);

        return CreateCallbackResult(
            true,
            "success",
            "eSewa payment completed for your appointment invoice.",
            "appointment",
            invoice.Id,
            invoice.InvoiceNumber,
            payload,
            statusCheck.RefId);
    }

    private async Task<EsewaStatusCheckResult> CheckTransactionStatusAsync(
        string transactionUuid,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        var url = BuildStatusCheckUrl(transactionUuid, totalAmount);

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new EsewaStatusCheckResult(null, null, $"eSewa status check failed with HTTP {(int)response.StatusCode}.");
            }

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            return new EsewaStatusCheckResult(
                ReadJsonValue(root, "status"),
                ReadJsonValue(root, "ref_id") ?? ReadJsonValue(root, "refId"),
                ReadJsonValue(root, "error_message"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new EsewaStatusCheckResult(null, null, "eSewa status check could not be completed.");
        }
    }

    private string BuildStatusCheckUrl(string transactionUuid, decimal totalAmount)
    {
        var separator = _options.StatusUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return string.Concat(
            _options.StatusUrl.TrimEnd('?'),
            separator,
            "product_code=", Uri.EscapeDataString(_options.ProductCode),
            "&total_amount=", Uri.EscapeDataString(FormatAmount(totalAmount)),
            "&transaction_uuid=", Uri.EscapeDataString(transactionUuid));
    }

    private string BuildApiCallbackUrl(string result)
    {
        var request = _httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("Payment callbacks require an active HTTP request.");

        return $"{request.Scheme}://{request.Host}{request.PathBase}/api/payments/esewa/{result}";
    }

    private EsewaPayload? DecodePayload(string encodedData)
    {
        try
        {
            var normalized = encodedData.Trim().Replace(' ', '+');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
            using var document = JsonDocument.Parse(json);
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                fields[property.Name] = ReadJsonValue(property.Value) ?? string.Empty;
            }

            return new EsewaPayload(fields);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }

    private bool VerifyResponseSignature(EsewaPayload payload)
    {
        var signature = payload.Get("signature");
        var signedFieldNames = payload.Get("signed_field_names");
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(signedFieldNames))
        {
            return false;
        }

        var message = BuildSignedMessage(payload.Fields, signedFieldNames);
        var expected = Sign(message);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    private string Sign(string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    private static string BuildSignedMessage(IReadOnlyDictionary<string, string> fields, string signedFieldNames)
    {
        return string.Join(
            ",",
            signedFieldNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(field => $"{field}={(fields.TryGetValue(field, out var value) ? value : string.Empty)}"));
    }

    private static bool TryReadTarget(string? transactionUuid, out string targetType, out int targetId)
    {
        targetType = string.Empty;
        targetId = 0;
        if (string.IsNullOrWhiteSpace(transactionUuid)) return false;

        var parts = transactionUuid.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out targetId))
        {
            return false;
        }

        targetType = parts[0];
        return true;
    }

    private EsewaPaymentCallbackResult CreateCallbackResult(
        bool succeeded,
        string status,
        string message,
        string? invoiceType = null,
        int? invoiceId = null,
        string? invoiceNumber = null,
        EsewaPayload? payload = null,
        string? referenceId = null)
    {
        if ((string.IsNullOrWhiteSpace(invoiceType) || !invoiceId.HasValue) &&
            TryReadTarget(payload?.Get("transaction_uuid"), out var parsedType, out var parsedId))
        {
            invoiceType = parsedType;
            invoiceId = parsedId;
        }

        var transactionCode = payload?.Get("transaction_code");
        var redirectUrl = BuildFrontendRedirectUrl(status, message, invoiceType, invoiceId, invoiceNumber, transactionCode, referenceId);

        return new EsewaPaymentCallbackResult(
            succeeded,
            status,
            message,
            redirectUrl,
            invoiceType,
            invoiceId,
            invoiceNumber,
            transactionCode,
            referenceId);
    }

    private string BuildFrontendRedirectUrl(
        string status,
        string message,
        string? invoiceType,
        int? invoiceId,
        string? invoiceNumber,
        string? transactionCode,
        string? referenceId)
    {
        var query = new Dictionary<string, string?>
        {
            ["status"] = status,
            ["message"] = message,
            ["type"] = invoiceType,
            ["invoiceId"] = invoiceId?.ToString(CultureInfo.InvariantCulture),
            ["invoiceNumber"] = invoiceNumber,
            ["transactionCode"] = transactionCode,
            ["referenceId"] = referenceId
        };

        var queryText = string.Join(
            "&",
            query.Where(item => !string.IsNullOrWhiteSpace(item.Value))
                .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value!)}"));

        return $"{_options.FrontendBaseUrl.TrimEnd('/')}/customer/payment-result?{queryText}";
    }

    private static bool AmountsMatch(decimal expected, decimal actual)
    {
        return decimal.Round(expected, 2) == decimal.Round(actual, 2);
    }

    private static decimal? ParseAmount(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : null;
    }

    private static string FormatAmount(decimal amount)
    {
        return amount.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string? ReadJsonValue(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) ? ReadJsonValue(value) : null;
    }

    private static string? ReadJsonValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => value.GetRawText()
        };
    }

    private sealed record EsewaPayload(IReadOnlyDictionary<string, string> Fields)
    {
        public string? Get(string key) => Fields.TryGetValue(key, out var value) ? value : null;
    }

    private sealed record EsewaStatusCheckResult(string? Status, string? RefId, string? Message);
}
