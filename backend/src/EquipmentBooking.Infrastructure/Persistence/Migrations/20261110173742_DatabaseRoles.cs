using EquipmentBooking.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBooking.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Group roles app_read, app_write and app_admin with their privileges. Login roles with
    /// passwords are created by database/scripts/create_login_roles.sql, never by a migration.
    /// </summary>
    public partial class DatabaseRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("003_database_roles_up.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("003_database_roles_down.sql"));
        }
    }
}
