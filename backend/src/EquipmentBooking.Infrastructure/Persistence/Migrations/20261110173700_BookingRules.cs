using EquipmentBooking.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBooking.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Business rules enforced by the database: overlap exclusion constraint, history and
    /// maintenance triggers, availability functions, views and search indexes.
    /// </summary>
    public partial class BookingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("002_booking_rules_up.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("002_booking_rules_down.sql"));
        }
    }
}
