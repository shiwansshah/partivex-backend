namespace Partivex.Application.Constants;

public static class StaffFeatureKeys
{
    public const string CustomerManagement = "CustomerManagement";

    public const string Vehicles = "Vehicles";

    public const string CustomerReports = "CustomerReports";

    public const string Sales = "Sales";

    public const string PartRequestApprovals = "PartRequestApprovals";

    public const string CustomerPartInvoices = "CustomerPartInvoices";

    public const string AppointmentInvoices = "AppointmentInvoices";

    public static IReadOnlyCollection<(string Key, string DisplayName)> All { get; } =
    [
        (CustomerManagement, "Customer Management"),
        (Vehicles, "Vehicles"),
        (CustomerReports, "Customer Reports"),
        (Sales, "Sales"),
        (PartRequestApprovals, "Part Request Approvals"),
        (CustomerPartInvoices, "Customer Part Invoices"),
        (AppointmentInvoices, "Appointment Invoices")
    ];

    public static bool IsKnown(string featureKey)
    {
        return All.Any(feature => feature.Key == featureKey);
    }
}
