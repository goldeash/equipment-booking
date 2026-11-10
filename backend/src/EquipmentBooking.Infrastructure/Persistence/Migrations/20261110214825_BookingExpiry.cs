using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EquipmentBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BookingExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "app",
                table: "notification_types",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "booking_not_collected", "Booking cancelled because the item was not collected" },
                    { "request_expired", "Request expired without a decision" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "app",
                table: "notification_types",
                keyColumn: "code",
                keyValue: "booking_not_collected");

            migrationBuilder.DeleteData(
                schema: "app",
                table: "notification_types",
                keyColumn: "code",
                keyValue: "request_expired");
        }
    }
}
