namespace Partivex.Application.Constants; // Defines constants namespace.

public static class ApplicationRoles // Defines application role names.
{
    public const string Admin = "Admin"; // Defines admin role.

    public const string Staff = "Staff"; // Defines staff role.

    public const string Customer = "Customer"; // Defines customer role.

    public const string AdminAndStaff = Admin + "," + Staff; // Defines read roles.

    public const string AdminStaffAndCustomer = Admin + "," + Staff + "," + Customer; // Defines vehicle roles.

    public static IReadOnlyCollection<string> All { get; } = [Admin, Staff, Customer]; // Defines seeded roles.
}
