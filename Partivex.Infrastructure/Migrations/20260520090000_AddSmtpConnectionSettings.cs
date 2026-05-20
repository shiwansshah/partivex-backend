using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Partivex.Infrastructure.Data;

#nullable disable

namespace Partivex.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260520090000_AddSmtpConnectionSettings")]
    public partial class AddSmtpConnectionSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableSsl",
                table: "SmtpSettings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "SmtpSettings",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "SmtpSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "SmtpSettings",
                type: "integer",
                nullable: false,
                defaultValue: 587);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "SmtpSettings",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EnableSsl", table: "SmtpSettings");
            migrationBuilder.DropColumn(name: "Host", table: "SmtpSettings");
            migrationBuilder.DropColumn(name: "Password", table: "SmtpSettings");
            migrationBuilder.DropColumn(name: "Port", table: "SmtpSettings");
            migrationBuilder.DropColumn(name: "Username", table: "SmtpSettings");
        }
    }
}
