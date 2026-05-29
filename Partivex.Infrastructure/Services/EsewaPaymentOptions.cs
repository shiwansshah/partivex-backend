namespace Partivex.Infrastructure.Services;

public sealed class EsewaPaymentOptions
{
    public const string SectionName = "Esewa";

    public string Environment { get; set; } = "Sandbox";

    public string ProductCode { get; set; } = "EPAYTEST";

    public string SecretKey { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = "https://rc-epay.esewa.com.np/api/epay/main/v2/form";

    public string StatusUrl { get; set; } = "https://rc.esewa.com.np/api/epay/transaction/status/";

    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}
