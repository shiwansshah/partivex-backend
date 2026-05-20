using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<PartRequest> PartRequests => Set<PartRequest>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<CustomerHistory> CustomerHistories { get; set; } = null!; // Stores history rows.

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InventoryStockChange> InventoryStockChanges => Set<InventoryStockChange>();

    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();

    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

    public DbSet<CustomerPartPurchaseInvoice> CustomerPartPurchaseInvoices => Set<CustomerPartPurchaseInvoice>();

    public DbSet<CustomerPartPurchaseInvoiceItem> CustomerPartPurchaseInvoiceItems => Set<CustomerPartPurchaseInvoiceItem>();

    public DbSet<AppointmentInvoice> AppointmentInvoices => Set<AppointmentInvoice>();

    public DbSet<SmtpSetting> SmtpSettings => Set<SmtpSetting>();

    public DbSet<Vendor> Vendors { get; set; }

    public DbSet<Part> Parts { get; set; }

    public DbSet<StaffFeatureAccess> StaffFeatureAccesses => Set<StaffFeatureAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Customers");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.PhoneNumber)
            .IsUnique();

        builder.Entity<ApplicationUser>()
            .Property(user => user.ImageUrl)
            .HasMaxLength(500);

        builder.Entity<StaffFeatureAccess>(entity =>
        {
            entity.ToTable("StaffFeatureAccesses");
            entity.HasKey(access => access.Id);
            entity.Property(access => access.UserId).IsRequired().HasMaxLength(450);
            entity.Property(access => access.FeatureKey).IsRequired().HasMaxLength(80);
            entity.Property(access => access.IsEnabled).IsRequired();
            entity.Property(access => access.CreatedAt).IsRequired();
            entity.Property(access => access.UpdatedAt).IsRequired();
            entity.HasIndex(access => new { access.UserId, access.FeatureKey }).IsUnique();
            entity.HasOne(access => access.Staff)
                .WithMany()
                .HasForeignKey(access => access.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems");
            entity.Property(item => item.PartNumber).HasMaxLength(40);
            entity.Property(item => item.Name).HasMaxLength(120);
            entity.Property(item => item.Category).HasMaxLength(80);
            entity.Property(item => item.VendorName).HasMaxLength(120);
            entity.Property(item => item.StorageLocation).HasMaxLength(80);
            entity.Property(item => item.UnitCost).HasPrecision(18, 2);
            entity.HasIndex(item => item.PartNumber).IsUnique();
        });

        builder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.Property(vendor => vendor.Name).IsRequired().HasMaxLength(120);
            entity.Property(vendor => vendor.ContactPerson).IsRequired().HasMaxLength(120);
            entity.Property(vendor => vendor.Phone).IsRequired().HasMaxLength(30);
            entity.Property(vendor => vendor.Email).IsRequired().HasMaxLength(160);
            entity.Property(vendor => vendor.Address).IsRequired().HasMaxLength(240);
            entity.HasIndex(vendor => vendor.Email).IsUnique();
        });

        builder.Entity<Part>(entity =>
        {
            entity.ToTable("Parts");
            entity.Property(part => part.Name).IsRequired().HasMaxLength(120);
            entity.Property(part => part.PartCode).IsRequired().HasMaxLength(40);
            entity.Property(part => part.Category).IsRequired().HasMaxLength(80);
            entity.Property(part => part.CompatibleVehicle).HasMaxLength(120);
            entity.Property(part => part.UnitPrice).HasPrecision(18, 2);
            entity.Property(part => part.ImageUrl).HasMaxLength(500);
            entity.HasIndex(part => part.PartCode).IsUnique();
        });

        builder.Entity<PurchaseInvoice>(entity =>
        {
            entity.ToTable("PurchaseInvoices");
            entity.Property(inv => inv.InvoiceNumber).HasMaxLength(40);
            entity.Property(inv => inv.VendorName).HasMaxLength(120);
            entity.Property(inv => inv.Status).HasMaxLength(20);
            entity.Property(inv => inv.CreatedBy).HasMaxLength(120);
            entity.Property(inv => inv.Notes).HasMaxLength(500);
            entity.HasIndex(inv => inv.InvoiceNumber).IsUnique();
            entity.HasOne(inv => inv.Vendor)
                .WithMany()
                .HasForeignKey(inv => inv.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.ToTable("PurchaseInvoiceItems");
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(item => item.PurchaseInvoice)
                .WithMany(inv => inv.Items)
                .HasForeignKey(item => item.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Part)
                .WithMany()
                .HasForeignKey(item => item.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InventoryStockChange>(entity =>
        {
            entity.ToTable("InventoryStockChanges");
            entity.Property(change => change.ChangeType).HasMaxLength(40);
            entity.Property(change => change.ReferenceCode).HasMaxLength(40);
            entity.Property(change => change.ChangedBy).HasMaxLength(120);
            entity.Property(change => change.Notes).HasMaxLength(240);
            entity.HasOne(change => change.Part)
                .WithMany(item => item.StockChanges)
                .HasForeignKey(change => change.PartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(change => change.Vendor)
                .WithMany()
                .HasForeignKey(change => change.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Name).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Number).IsRequired().HasMaxLength(50);
            entity.Property(v => v.ImageUrl).HasMaxLength(500);

            entity.HasOne(v => v.Customer)
                .WithMany()
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(appointment => appointment.Id);
            entity.Property(appointment => appointment.ServiceType).IsRequired().HasMaxLength(100);
            entity.Property(appointment => appointment.Notes).HasMaxLength(1000);
            entity.Property(appointment => appointment.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(appointment => appointment.CustomerId);
            entity.HasIndex(appointment => appointment.VehicleId);
            entity.HasIndex(appointment => appointment.PreferredAt);

            entity.HasOne(appointment => appointment.Customer)
                .WithMany()
                .HasForeignKey(appointment => appointment.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(appointment => appointment.Vehicle)
                .WithMany()
                .HasForeignKey(appointment => appointment.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PartRequest>(entity =>
        {
            entity.ToTable("PartRequests");
            entity.HasKey(request => request.Id);
            entity.Property(request => request.PartName).IsRequired().HasMaxLength(120);
            entity.Property(request => request.BrandModelSpecification).HasMaxLength(200);
            entity.Property(request => request.Reason).HasMaxLength(1000);
            entity.Property(request => request.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(request => request.CustomerId);
            entity.HasIndex(request => request.VehicleId);
            entity.HasIndex(request => request.PartId);
            entity.HasIndex(request => request.Status);

            entity.HasOne(request => request.Customer)
                .WithMany()
                .HasForeignKey(request => request.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(request => request.Vehicle)
                .WithMany()
                .HasForeignKey(request => request.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(request => request.Part)
                .WithMany()
                .HasForeignKey(request => request.PartId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CustomerPartPurchaseInvoice>(entity =>
        {
            entity.ToTable("CustomerPartPurchaseInvoices");
            entity.Property(invoice => invoice.InvoiceNumber).IsRequired().HasMaxLength(40);
            entity.Property(invoice => invoice.CustomerName).IsRequired().HasMaxLength(120);
            entity.Property(invoice => invoice.CustomerEmail).HasMaxLength(160);
            entity.Property(invoice => invoice.Source).IsRequired().HasMaxLength(40);
            entity.Property(invoice => invoice.Status).IsRequired().HasMaxLength(24);
            entity.Property(invoice => invoice.CreatedBy).HasMaxLength(120);
            entity.Property(invoice => invoice.SubTotal).HasPrecision(18, 2);
            entity.Property(invoice => invoice.DiscountAmount).HasPrecision(18, 2);
            entity.Property(invoice => invoice.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();
            entity.HasIndex(invoice => invoice.CustomerId);
            entity.HasIndex(invoice => invoice.PartRequestId);

            entity.HasOne(invoice => invoice.Customer)
                .WithMany()
                .HasForeignKey(invoice => invoice.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(invoice => invoice.PartRequest)
                .WithMany()
                .HasForeignKey(invoice => invoice.PartRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CustomerPartPurchaseInvoiceItem>(entity =>
        {
            entity.ToTable("CustomerPartPurchaseInvoiceItems");
            entity.Property(item => item.PartCode).IsRequired().HasMaxLength(40);
            entity.Property(item => item.PartName).IsRequired().HasMaxLength(120);
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.SubTotal).HasPrecision(18, 2);

            entity.HasOne(item => item.CustomerPartPurchaseInvoice)
                .WithMany(invoice => invoice.Items)
                .HasForeignKey(item => item.CustomerPartPurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Part)
                .WithMany()
                .HasForeignKey(item => item.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AppointmentInvoice>(entity =>
        {
            entity.ToTable("AppointmentInvoices");
            entity.Property(invoice => invoice.InvoiceNumber).IsRequired().HasMaxLength(40);
            entity.Property(invoice => invoice.CustomerName).IsRequired().HasMaxLength(120);
            entity.Property(invoice => invoice.CustomerEmail).HasMaxLength(160);
            entity.Property(invoice => invoice.ServiceType).IsRequired().HasMaxLength(100);
            entity.Property(invoice => invoice.VehicleName).HasMaxLength(100);
            entity.Property(invoice => invoice.VehicleNumber).HasMaxLength(50);
            entity.Property(invoice => invoice.PaymentStatus).IsRequired().HasMaxLength(24);
            entity.Property(invoice => invoice.Notes).HasMaxLength(500);
            entity.Property(invoice => invoice.CreatedBy).HasMaxLength(120);
            entity.Property(invoice => invoice.Amount).HasPrecision(18, 2);
            entity.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();
            entity.HasIndex(invoice => invoice.CustomerId);
            entity.HasIndex(invoice => invoice.AppointmentId);
            entity.HasIndex(invoice => invoice.PaymentStatus);

            entity.HasOne(invoice => invoice.Appointment)
                .WithMany()
                .HasForeignKey(invoice => invoice.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(invoice => invoice.Customer)
                .WithMany()
                .HasForeignKey(invoice => invoice.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SmtpSetting>(entity =>
        {
            entity.ToTable("SmtpSettings");
            entity.Property(setting => setting.SenderEmail).IsRequired().HasMaxLength(160);
            entity.Property(setting => setting.Host).IsRequired().HasMaxLength(160);
            entity.Property(setting => setting.Username).HasMaxLength(160);
            entity.Property(setting => setting.Password).HasMaxLength(500);
        });

        builder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(review => review.Id);
            entity.Property(review => review.Category)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(review => review.Comment).IsRequired().HasMaxLength(2000);

            entity.HasIndex(review => review.CustomerId);
            entity.HasIndex(review => new { review.CustomerId, review.AppointmentId })
                .IsUnique()
                .HasFilter("\"AppointmentId\" IS NOT NULL");

            entity.HasOne(review => review.Customer)
                .WithMany()
                .HasForeignKey(review => review.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(review => review.Appointment)
                .WithMany(appointment => appointment.Reviews)
                .HasForeignKey(review => review.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CustomerHistory>(entity => // Configures history entity.
        { // Begins history configuration.
            entity.ToTable("CustomerHistories"); // Maps history table.

            entity.HasKey(history => history.Id); // Configures primary key.

            entity.Property(history => history.CustomerId).IsRequired(); // Requires customer id.

            entity.Property(history => history.VehicleId); // Configures optional vehicle id.

            entity.Property(history => history.HistoryType)
                .HasConversion<string>()
                .HasMaxLength(20); // Stores history type as text.

            entity.Property(history => history.Description).IsRequired().HasMaxLength(1000); // Requires description.

            entity.Property(history => history.Amount).HasPrecision(18, 2); // Stores amount precisely.

            entity.Property(history => history.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(20); // Stores payment status as text.

            entity.Property(history => history.HistoryDate).IsRequired(); // Requires history timestamp.

            entity.HasIndex(history => history.CustomerId); // Adds customer index.

            entity.HasIndex(history => history.VehicleId); // Adds vehicle index.

            entity.HasIndex(history => history.HistoryType); // Adds type index.

            entity.HasIndex(history => history.PaymentStatus); // Adds payment index.

            entity.HasOne(history => history.Customer) // Configures customer relation.
                .WithMany(customer => customer.CustomerHistories) // Uses navigation collection.
                .HasForeignKey(history => history.CustomerId) // Uses customer id FK.
                .OnDelete(DeleteBehavior.Cascade); // Deletes history with customer.

            entity.HasOne(history => history.Vehicle) // Configures vehicle relation.
                .WithMany(vehicle => vehicle.CustomerHistories) // Uses vehicle history collection.
                .HasForeignKey(history => history.VehicleId) // Uses vehicle id FK.
                .OnDelete(DeleteBehavior.SetNull); // Keeps history if vehicle is deleted.
        }); // Ends history configuration.
    }
}
