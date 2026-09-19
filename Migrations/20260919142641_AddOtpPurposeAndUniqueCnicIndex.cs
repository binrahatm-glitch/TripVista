using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripVista.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpPurposeAndUniqueCnicIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_booking_travelers_BookingId",
                table: "booking_travelers");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "otp_verifications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Registration");

            migrationBuilder.CreateIndex(
                name: "IX_booking_travelers_BookingId_CNICOrPassportNumber",
                table: "booking_travelers",
                columns: new[] { "BookingId", "CNICOrPassportNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_booking_travelers_BookingId_CNICOrPassportNumber",
                table: "booking_travelers");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "otp_verifications");

            migrationBuilder.CreateIndex(
                name: "IX_booking_travelers_BookingId",
                table: "booking_travelers",
                column: "BookingId");
        }
    }
}
