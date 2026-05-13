namespace Partivex.Application.Constants;

public static class PermissionNames
{
    public const string ManageStaff = "ManageStaff";

    public const string ViewDashboard = "ViewDashboard";

    public const string ViewActivityLogs = "ViewActivityLogs";

    public const string ManageRoles = "ManageRoles";

    public const string ManagePermissions = "ManagePermissions";

    public static IReadOnlyCollection<(string Name, string Description)> SeedPermissions { get; } =
    [
        (ManageStaff, "Allows staff account management."),
        (ViewDashboard, "Allows viewing the admin dashboard."),
        (ViewActivityLogs, "Allows viewing activity logs."),
        (ManageRoles, "Allows role management actions."),
        (ManagePermissions, "Allows managing coursework permission assignments.")
    ];
}
