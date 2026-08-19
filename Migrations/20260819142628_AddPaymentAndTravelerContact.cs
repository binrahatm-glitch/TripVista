using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripVista.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndTravelerContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "booking_travelers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "booking_travelers");
        }
    }
}
