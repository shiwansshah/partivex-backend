using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Partivex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesAndInventoryItemStockChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStockChanges_Parts_PartId",
                table: "InventoryStockChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStockChanges_Vendors_VendorId",
                table: "InventoryStockChanges");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "InventoryStockChanges",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PartId",
                table: "InventoryStockChanges",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "InventoryItemId",
                table: "InventoryStockChanges",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    VehicleNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SoldBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SaleDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<int>(type: "integer", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalSalesCount = table.Column<int>(type: "integer", nullable: false),
                    TotalItemsSold = table.Column<int>(type: "integer", nullable: false),
                    GrossRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    InventoryItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    VehicleNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IssuedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<int>(type: "integer", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoices_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockChanges_InventoryItemId",
                table: "InventoryStockChanges",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_InventoryItemId",
                table: "SaleItems",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_InvoiceNumber",
                table: "SalesInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_SaleId",
                table: "SalesInvoices",
                column: "SaleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReports_ReportDate",
                table: "SalesReports",
                column: "ReportDate",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStockChanges_InventoryItems_InventoryItemId",
                table: "InventoryStockChanges",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStockChanges_Parts_PartId",
                table: "InventoryStockChanges",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStockChanges_Vendors_VendorId",
                table: "InventoryStockChanges",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStockChanges_InventoryItems_InventoryItemId",
                table: "InventoryStockChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStockChanges_Parts_PartId",
                table: "InventoryStockChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStockChanges_Vendors_VendorId",
                table: "InventoryStockChanges");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "SalesInvoices");

            migrationBuilder.DropTable(
                name: "SalesReports");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStockChanges_InventoryItemId",
                table: "InventoryStockChanges");

            migrationBuilder.DropColumn(
                name: "InventoryItemId",
                table: "InventoryStockChanges");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "InventoryStockChanges",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PartId",
                table: "InventoryStockChanges",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStockChanges_Parts_PartId",
                table: "InventoryStockChanges",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStockChanges_Vendors_VendorId",
                table: "InventoryStockChanges",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
