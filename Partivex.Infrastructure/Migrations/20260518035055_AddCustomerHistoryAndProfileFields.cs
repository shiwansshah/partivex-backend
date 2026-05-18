using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partivex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerHistoryAndProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "CustomerHistories",
                newName: "HistoryDate");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CustomerHistories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "CustomerHistories",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "HistoryType",
                table: "CustomerHistories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Purchase");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "CustomerHistories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Paid");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "CustomerHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerHistories_HistoryType",
                table: "CustomerHistories",
                column: "HistoryType");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerHistories_PaymentStatus",
                table: "CustomerHistories",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerHistories_VehicleId",
                table: "CustomerHistories",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerHistories_Vehicles_VehicleId",
                table: "CustomerHistories",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerHistories_Vehicles_VehicleId",
                table: "CustomerHistories");

            migrationBuilder.DropIndex(
                name: "IX_CustomerHistories_HistoryType",
                table: "CustomerHistories");

            migrationBuilder.DropIndex(
                name: "IX_CustomerHistories_PaymentStatus",
                table: "CustomerHistories");

            migrationBuilder.DropIndex(
                name: "IX_CustomerHistories_VehicleId",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "HistoryType",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CustomerHistories");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "CustomerHistories");

            migrationBuilder.RenameColumn(
                name: "HistoryDate",
                table: "CustomerHistories",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CustomerHistories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);
        }
    }
}
