using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partivex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCustomerPhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers");
        }
    }
}
