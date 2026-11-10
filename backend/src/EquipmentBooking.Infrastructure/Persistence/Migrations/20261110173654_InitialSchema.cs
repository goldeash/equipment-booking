using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EquipmentBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "booking_statuses",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_statuses", x => x.code);
                },
                comment: "Statuses of the booking workflow.");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Unique category name."),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Optional description."),
                    max_loan_days = table.Column<int>(type: "integer", nullable: false, comment: "Longest loan in days that can be approved automatically."),
                    usage_guidelines = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "What the items may be used for; the assistant compares the stated purpose with this text."),
                    is_specialised = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Requests for items of a specialised category always go to a manager."),
                    auto_approval_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Switch for automatic approval of this category.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.CheckConstraint("ck_categories_max_loan_days", "max_loan_days > 0");
                },
                comment: "Groups of items with the same loan limit, usage guidelines and approval settings.");

            migrationBuilder.CreateTable(
                name: "decision_sources",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_decision_sources", x => x.code);
                },
                comment: "Who took a decision on a request.");

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Unique department name.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                },
                comment: "Organisational units; a manager decides on requests of the own department.");

            migrationBuilder.CreateTable(
                name: "features",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false, comment: "Unique feature name.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_features", x => x.id);
                },
                comment: "Searchable properties of items, for example wireless or 4K.");

            migrationBuilder.CreateTable(
                name: "item_conditions",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    sort_order = table.Column<int>(type: "integer", nullable: false, comment: "Display order, 1 is the best condition."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_conditions", x => x.code);
                },
                comment: "Physical condition of an item, ordered from best to worst.");

            migrationBuilder.CreateTable(
                name: "item_statuses",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_statuses", x => x.code);
                },
                comment: "Stored statuses of equipment items. Out of service is derived from active maintenance records.");

            migrationBuilder.CreateTable(
                name: "locations",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Unique location name, for example a room or a storage.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                },
                comment: "Places where items are kept when not on loan.");

            migrationBuilder.CreateTable(
                name: "maintenance_types",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_types", x => x.code);
                },
                comment: "Types of maintenance work.");

            migrationBuilder.CreateTable(
                name: "notification_types",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_types", x => x.code);
                },
                comment: "Types of in-app notifications.");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.code);
                },
                comment: "Fixed list of user roles.");

            migrationBuilder.CreateTable(
                name: "routing_reasons",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routing_reasons", x => x.code);
                },
                comment: "Reasons why a request is routed to the department manager instead of being approved automatically.");

            migrationBuilder.CreateTable(
                name: "equipment_items",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    inventory_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Unique inventory number printed on the item."),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Item name, for example the model."),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Optional description used by keyword search."),
                    category_id = table.Column<long>(type: "bigint", nullable: false, comment: "Category of the item."),
                    location_id = table.Column<long>(type: "bigint", nullable: false, comment: "Where the item is kept."),
                    value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, comment: "Replacement value in EUR; compared with the low value threshold."),
                    condition_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Current physical condition."),
                    status_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stored status: available or retired."),
                    is_specialised = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Item level override: specialised even if the category is not."),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "When the item was retired; set together with the status retired."),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Creation time."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Last change time, set by trigger.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_items", x => x.id);
                    table.CheckConstraint("ck_equipment_items_retired", "(status_code = 'retired') = (retired_at IS NOT NULL)");
                    table.CheckConstraint("ck_equipment_items_value", "value >= 0");
                    table.ForeignKey(
                        name: "fk_equipment_items_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "app",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_equipment_items_item_conditions_condition_code",
                        column: x => x.condition_code,
                        principalSchema: "app",
                        principalTable: "item_conditions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_equipment_items_item_statuses_status_code",
                        column: x => x.status_code,
                        principalSchema: "app",
                        principalTable: "item_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_equipment_items_locations_location_id",
                        column: x => x.location_id,
                        principalSchema: "app",
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Physical items of shared equipment.");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    email = table.Column<string>(type: "citext", maxLength: 254, nullable: false, comment: "Work e-mail address used to sign in, unique without regard to case."),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Full name shown to managers and administrators."),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "PBKDF2-HMAC-SHA512 hash with salt (ASP.NET Core Identity format v3); empty until a password is set."),
                    role_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Role of the user."),
                    department_id = table.Column<long>(type: "bigint", nullable: false, comment: "Department of the user."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Inactive users cannot sign in."),
                    failed_sign_in_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Consecutive failed sign ins since the last success or lockout."),
                    lockout_ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Sign in is refused until this time after too many failed attempts."),
                    privacy_notice_acknowledged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "When the user first saw the privacy notice (acknowledgement, not consent)."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Creation time."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Last change time, set by trigger.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_email_format", "email ~ '^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$'");
                    table.CheckConstraint("ck_users_failed_sign_in_count", "failed_sign_in_count >= 0");
                    table.ForeignKey(
                        name: "fk_users_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "app",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_users_roles_role_code",
                        column: x => x.role_code,
                        principalSchema: "app",
                        principalTable: "roles",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "People who sign in. Personal data: e-mail and full name.");

            migrationBuilder.CreateTable(
                name: "equipment_item_features",
                schema: "app",
                columns: table => new
                {
                    equipment_item_id = table.Column<long>(type: "bigint", nullable: false),
                    feature_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_item_features", x => new { x.equipment_item_id, x.feature_id });
                    table.ForeignKey(
                        name: "fk_equipment_item_features_equipment_items_equipment_item_id",
                        column: x => x.equipment_item_id,
                        principalSchema: "app",
                        principalTable: "equipment_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipment_item_features_features_feature_id",
                        column: x => x.feature_id,
                        principalSchema: "app",
                        principalTable: "features",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Many-to-many link between items and their features.");

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    equipment_item_id = table.Column<long>(type: "bigint", nullable: false, comment: "Requested item."),
                    requester_id = table.Column<long>(type: "bigint", nullable: true, comment: "Employee who requested the item; null after anonymisation."),
                    anonymous_ref = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true, comment: "Random reference replacing the requester after the retention period."),
                    anonymised_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "When the personal data of the loan was anonymised."),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "First day of the booking."),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Last day of the booking (inclusive)."),
                    purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Purpose stated by the employee; must not contain personal data."),
                    status_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Current status."),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Creation time."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Last change time, set by trigger.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.CheckConstraint("ck_bookings_anonymised", "(requester_id IS NULL) = (anonymised_at IS NOT NULL)");
                    table.CheckConstraint("ck_bookings_anonymous_ref", "anonymised_at IS NULL OR anonymous_ref IS NOT NULL");
                    table.CheckConstraint("ck_bookings_dates", "end_date >= start_date");
                    table.CheckConstraint("ck_bookings_purpose_length", "char_length(purpose) BETWEEN 3 AND 1000");
                    table.ForeignKey(
                        name: "fk_bookings_booking_statuses_status_code",
                        column: x => x.status_code,
                        principalSchema: "app",
                        principalTable: "booking_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_equipment_items_equipment_item_id",
                        column: x => x.equipment_item_id,
                        principalSchema: "app",
                        principalTable: "equipment_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_users_requester_id",
                        column: x => x.requester_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned.");

            migrationBuilder.CreateTable(
                name: "equipment_condition_changes",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    equipment_item_id = table.Column<long>(type: "bigint", nullable: false, comment: "Item whose condition changed."),
                    from_condition_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true, comment: "Previous condition; null for a new item."),
                    to_condition_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "New condition."),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Change time."),
                    changed_by_user_id = table.Column<long>(type: "bigint", nullable: true, comment: "User who caused the change, if known.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_condition_changes", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipment_condition_changes_equipment_items_equipment_item_",
                        column: x => x.equipment_item_id,
                        principalSchema: "app",
                        principalTable: "equipment_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipment_condition_changes_item_conditions_from_condition_",
                        column: x => x.from_condition_code,
                        principalSchema: "app",
                        principalTable: "item_conditions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_equipment_condition_changes_item_conditions_to_condition_co",
                        column: x => x.to_condition_code,
                        principalSchema: "app",
                        principalTable: "item_conditions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_equipment_condition_changes_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "History of condition changes of items, written by trigger.");

            migrationBuilder.CreateTable(
                name: "maintenance_records",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    equipment_item_id = table.Column<long>(type: "bigint", nullable: false, comment: "Maintained item."),
                    maintenance_type_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Type of work."),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "First day of the maintenance."),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true, comment: "Last day of the maintenance; null while open ended."),
                    takes_out_of_service = table.Column<bool>(type: "boolean", nullable: false, comment: "The item cannot be booked during the period."),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "What was done or found."),
                    recorded_by_user_id = table.Column<long>(type: "bigint", nullable: false, comment: "Administrator who recorded the entry."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Creation time.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_records", x => x.id);
                    table.CheckConstraint("ck_maintenance_records_dates", "end_date IS NULL OR end_date >= start_date");
                    table.ForeignKey(
                        name: "fk_maintenance_records_equipment_items_equipment_item_id",
                        column: x => x.equipment_item_id,
                        principalSchema: "app",
                        principalTable: "equipment_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_records_maintenance_types_maintenance_type_code",
                        column: x => x.maintenance_type_code,
                        principalSchema: "app",
                        principalTable: "maintenance_types",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_records_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Maintenance and repairs of items; an out of service record blocks bookings in its period.");

            migrationBuilder.CreateTable(
                name: "organisation_settings",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false, comment: "Always 1."),
                    low_value_threshold = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, comment: "Items with a value equal to or above it are high value (EUR)."),
                    auto_approval_enabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Organisation wide switch for automatic approval."),
                    retention_days = table.Column<int>(type: "integer", nullable: false, comment: "Days after the return after which personal data of a loan is anonymised."),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "IANA time zone of the organisation."),
                    return_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false, comment: "Local time on the end date by which items must be returned."),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Last change time, set by trigger."),
                    updated_by_user_id = table.Column<long>(type: "bigint", nullable: true, comment: "Administrator who changed the settings last.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_settings", x => x.id);
                    table.CheckConstraint("ck_organisation_settings_retention", "retention_days >= 30");
                    table.CheckConstraint("ck_organisation_settings_singleton", "id = 1");
                    table.CheckConstraint("ck_organisation_settings_threshold", "low_value_threshold > 0");
                    table.ForeignKey(
                        name: "fk_organisation_settings_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Organisation wide settings, exactly one row.");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "Owner of the token."),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "SHA-256 hash of the token in hexadecimal; the token itself is never stored."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Issue time."),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Expiry time."),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Revocation time after rotation, sign out, role change or deactivation."),
                    replaced_by_token_id = table.Column<long>(type: "bigint", nullable: true, comment: "Token issued when this one was rotated.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.CheckConstraint("ck_refresh_tokens_expiry", "expires_at > created_at");
                    table.ForeignKey(
                        name: "fk_refresh_tokens_refresh_tokens_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalSchema: "app",
                        principalTable: "refresh_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Hashes of refresh tokens; every use rotates the token.");

            migrationBuilder.CreateTable(
                name: "approval_assessments",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false, comment: "Assessed booking."),
                    assessed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Time of the assessment."),
                    value_check_passed = table.Column<bool>(type: "boolean", nullable: false, comment: "The item value is below the low value threshold."),
                    specialised_check_passed = table.Column<bool>(type: "boolean", nullable: false, comment: "Neither the item nor its category is specialised."),
                    duration_check_passed = table.Column<bool>(type: "boolean", nullable: false, comment: "The loan does not exceed the maximum duration of the category."),
                    organisation_auto_approval_enabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Automatic approval was switched on for the organisation."),
                    category_auto_approval_enabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Automatic approval was switched on for the category."),
                    purpose_result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Result of the purpose check by the language model."),
                    outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "pending, approved or routed to the manager."),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Short summary for the manager prepared by the assistant."),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Language model used for the purpose check."),
                    prompt_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Version of the prompt template used.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_assessments", x => x.id);
                    table.CheckConstraint("ck_approval_assessments_approved_needs_match", "outcome <> 'approved' OR purpose_result = 'match'");
                    table.CheckConstraint("ck_approval_assessments_outcome", "outcome IN ('pending', 'approved', 'routed')");
                    table.CheckConstraint("ck_approval_assessments_purpose_result", "purpose_result IN ('pending', 'match', 'no_match', 'unavailable', 'not_run')");
                    table.ForeignKey(
                        name: "fk_approval_assessments_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "app",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Decision log of the approval route: result of every policy check, the purpose check and the outcome.");

            migrationBuilder.CreateTable(
                name: "booking_decisions",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false, comment: "Decided booking."),
                    decision_source_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Manager or AI assistant."),
                    decided_by_user_id = table.Column<long>(type: "bigint", nullable: true, comment: "Manager who decided; null for the assistant."),
                    outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "approved or declined; the assistant can only approve."),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Comment of the manager; required for a decline."),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Decision time."),
                    superseded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Set when a date change sends the booking through the route again.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_decisions", x => x.id);
                    table.CheckConstraint("ck_booking_decisions_assistant_never_declines", "decision_source_code <> 'ai_assistant' OR (decided_by_user_id IS NULL AND outcome = 'approved')");
                    table.CheckConstraint("ck_booking_decisions_decline_comment", "outcome <> 'declined' OR comment IS NOT NULL");
                    table.CheckConstraint("ck_booking_decisions_manager_identified", "decision_source_code <> 'manager' OR decided_by_user_id IS NOT NULL");
                    table.CheckConstraint("ck_booking_decisions_outcome", "outcome IN ('approved', 'declined')");
                    table.ForeignKey(
                        name: "fk_booking_decisions_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "app",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_decisions_decision_sources_decision_source_code",
                        column: x => x.decision_source_code,
                        principalSchema: "app",
                        principalTable: "decision_sources",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_booking_decisions_users_decided_by_user_id",
                        column: x => x.decided_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Decisions on requests. A date change supersedes the current decision, so a booking can have several.");

            migrationBuilder.CreateTable(
                name: "booking_status_history",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false, comment: "Changed booking."),
                    from_status_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true, comment: "Previous status; null when the booking was created."),
                    to_status_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "New status."),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "Change time."),
                    changed_by_user_id = table.Column<long>(type: "bigint", nullable: true, comment: "User who made the change; null for background jobs and after anonymisation.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_booking_status_history_booking_statuses_from_status_code",
                        column: x => x.from_status_code,
                        principalSchema: "app",
                        principalTable: "booking_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_booking_status_history_booking_statuses_to_status_code",
                        column: x => x.to_status_code,
                        principalSchema: "app",
                        principalTable: "booking_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_booking_status_history_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "app",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_status_history_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Every status change of a booking, written by trigger.");

            migrationBuilder.CreateTable(
                name: "loans",
                schema: "app",
                columns: table => new
                {
                    booking_id = table.Column<long>(type: "bigint", nullable: false, comment: "Booking of the loan (primary key and foreign key)."),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "When the item was handed out."),
                    issued_by_user_id = table.Column<long>(type: "bigint", nullable: false, comment: "Administrator who issued the item."),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Due time fixed at issue: end date at the organisation return time."),
                    returned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "When the item came back."),
                    received_by_user_id = table.Column<long>(type: "bigint", nullable: true, comment: "Administrator who received the item."),
                    return_condition_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true, comment: "Condition recorded at return."),
                    return_comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Comment at return; required when the item is damaged.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loans", x => x.booking_id);
                    table.CheckConstraint("ck_loans_damage_comment", "return_condition_code IS DISTINCT FROM 'damaged' OR return_comment IS NOT NULL");
                    table.CheckConstraint("ck_loans_return_complete", "(returned_at IS NULL) = (received_by_user_id IS NULL) AND (returned_at IS NULL) = (return_condition_code IS NULL)");
                    table.CheckConstraint("ck_loans_returned_after_issue", "returned_at IS NULL OR returned_at >= issued_at");
                    table.ForeignKey(
                        name: "fk_loans_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "app",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_loans_item_conditions_return_condition_code",
                        column: x => x.return_condition_code,
                        principalSchema: "app",
                        principalTable: "item_conditions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_loans_users_issued_by_user_id",
                        column: x => x.issued_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_loans_users_received_by_user_id",
                        column: x => x.received_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Issue and return of the item of a booking; one loan per booking.");

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false, comment: "Recipient."),
                    notification_type_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Type of notification."),
                    booking_id = table.Column<long>(type: "bigint", nullable: true, comment: "Related booking, if any."),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Short title."),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Message text."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Creation time."),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "When the recipient marked it as read."),
                    email_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Delivery state of the e-mail copy; not_required while no provider is configured."),
                    email_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of e-mail delivery attempts."),
                    email_sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "When the e-mail copy was sent.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint("ck_notifications_email_attempts", "email_attempts >= 0");
                    table.CheckConstraint("ck_notifications_email_status", "email_status IN ('pending', 'sent', 'failed', 'not_required')");
                    table.ForeignKey(
                        name: "fk_notifications_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "app",
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_notification_types_notification_type_code",
                        column: x => x.notification_type_code,
                        principalSchema: "app",
                        principalTable: "notification_types",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "In-app notifications; the e-mail columns form an outbox for a future e-mail provider.");

            migrationBuilder.CreateTable(
                name: "approval_assessment_reasons",
                schema: "app",
                columns: table => new
                {
                    assessment_id = table.Column<long>(type: "bigint", nullable: false, comment: "Assessment."),
                    routing_reason_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Reason for routing.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_assessment_reasons", x => new { x.assessment_id, x.routing_reason_code });
                    table.ForeignKey(
                        name: "fk_approval_assessment_reasons_approval_assessments_assessment",
                        column: x => x.assessment_id,
                        principalSchema: "app",
                        principalTable: "approval_assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_approval_assessment_reasons_routing_reasons_routing_reason_",
                        column: x => x.routing_reason_code,
                        principalSchema: "app",
                        principalTable: "routing_reasons",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Many-to-many link between assessments and every reason why the request was routed.");

            migrationBuilder.InsertData(
                schema: "app",
                table: "booking_statuses",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "approved", "Approved" },
                    { "cancelled", "Cancelled" },
                    { "declined", "Declined" },
                    { "issued", "Issued" },
                    { "overdue", "Overdue" },
                    { "requested", "Requested" },
                    { "returned", "Returned" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "decision_sources",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "ai_assistant", "AI assistant" },
                    { "manager", "Department manager" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "item_conditions",
                columns: new[] { "code", "name", "sort_order" },
                values: new object[,]
                {
                    { "damaged", "Damaged", 4 },
                    { "fair", "Fair", 3 },
                    { "good", "Good", 2 },
                    { "new", "New", 1 }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "item_statuses",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "available", "Available" },
                    { "retired", "Retired" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "maintenance_types",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "calibration", "Calibration" },
                    { "cleaning", "Cleaning" },
                    { "inspection", "Inspection" },
                    { "repair", "Repair" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "notification_types",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "booking_cancelled_by_maintenance", "Booking cancelled because of maintenance" },
                    { "loan_overdue", "Loan overdue" },
                    { "request_approved", "Request approved" },
                    { "request_declined", "Request declined" },
                    { "return_reminder", "Return reminder" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "organisation_settings",
                columns: new[] { "id", "auto_approval_enabled", "low_value_threshold", "retention_days", "return_time", "time_zone", "updated_by_user_id" },
                values: new object[] { (short)1, true, 500.00m, 365, new TimeOnly(18, 0, 0), "Europe/Vilnius", null });

            migrationBuilder.InsertData(
                schema: "app",
                table: "roles",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "administrator", "Administrator" },
                    { "employee", "Employee" },
                    { "manager", "Department manager" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "routing_reasons",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "assistant_unavailable", "The AI assistant is not available" },
                    { "category_auto_approval_off", "Automatic approval is switched off for the category" },
                    { "duration_exceeded", "The loan exceeds the maximum duration of the category" },
                    { "high_value", "The item is high value" },
                    { "org_auto_approval_off", "Automatic approval is switched off for the organisation" },
                    { "purpose_not_confirmed", "The assistant could not confirm the purpose" },
                    { "specialised", "The item or its category is specialised" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_approval_assessment_reasons_routing_reason_code",
                schema: "app",
                table: "approval_assessment_reasons",
                column: "routing_reason_code");

            migrationBuilder.CreateIndex(
                name: "ix_approval_assessments_assessed_at",
                schema: "app",
                table: "approval_assessments",
                column: "assessed_at");

            migrationBuilder.CreateIndex(
                name: "ix_approval_assessments_booking_id",
                schema: "app",
                table: "approval_assessments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_decisions_decided_by_user_id",
                schema: "app",
                table: "booking_decisions",
                column: "decided_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_decisions_decision_source_code",
                schema: "app",
                table: "booking_decisions",
                column: "decision_source_code");

            migrationBuilder.CreateIndex(
                name: "ux_booking_decisions_current",
                schema: "app",
                table: "booking_decisions",
                column: "booking_id",
                unique: true,
                filter: "superseded_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_booking_changed_at",
                schema: "app",
                table: "booking_status_history",
                columns: new[] { "booking_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_changed_by_user_id",
                schema: "app",
                table: "booking_status_history",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_from_status_code",
                schema: "app",
                table: "booking_status_history",
                column: "from_status_code");

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_history_to_status_code",
                schema: "app",
                table: "booking_status_history",
                column: "to_status_code");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_item_start_date",
                schema: "app",
                table: "bookings",
                columns: new[] { "equipment_item_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_requester_id",
                schema: "app",
                table: "bookings",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status_start_date",
                schema: "app",
                table: "bookings",
                columns: new[] { "status_code", "start_date" });

            migrationBuilder.CreateIndex(
                name: "ux_categories_name",
                schema: "app",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_departments_name",
                schema: "app",
                table: "departments",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_equipment_condition_changes_changed_by_user_id",
                schema: "app",
                table: "equipment_condition_changes",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_condition_changes_from_condition_code",
                schema: "app",
                table: "equipment_condition_changes",
                column: "from_condition_code");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_condition_changes_item_changed_at",
                schema: "app",
                table: "equipment_condition_changes",
                columns: new[] { "equipment_item_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_equipment_condition_changes_to_condition_code",
                schema: "app",
                table: "equipment_condition_changes",
                column: "to_condition_code");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_item_features_feature_id",
                schema: "app",
                table: "equipment_item_features",
                column: "feature_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_items_category_id",
                schema: "app",
                table: "equipment_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_items_condition_code",
                schema: "app",
                table: "equipment_items",
                column: "condition_code");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_items_location_id",
                schema: "app",
                table: "equipment_items",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_items_status_code",
                schema: "app",
                table: "equipment_items",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "ux_equipment_items_inventory_number",
                schema: "app",
                table: "equipment_items",
                column: "inventory_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_features_name",
                schema: "app",
                table: "features",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loans_issued_by_user_id",
                schema: "app",
                table: "loans",
                column: "issued_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_loans_open_due_at",
                schema: "app",
                table: "loans",
                column: "due_at",
                filter: "returned_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_loans_received_by_user_id",
                schema: "app",
                table: "loans",
                column: "received_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_loans_return_condition_code",
                schema: "app",
                table: "loans",
                column: "return_condition_code");

            migrationBuilder.CreateIndex(
                name: "ux_locations_name",
                schema: "app",
                table: "locations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_item_start_date",
                schema: "app",
                table: "maintenance_records",
                columns: new[] { "equipment_item_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_maintenance_type_code",
                schema: "app",
                table: "maintenance_records",
                column: "maintenance_type_code");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_recorded_by_user_id",
                schema: "app",
                table: "maintenance_records",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_booking_id",
                schema: "app",
                table: "notifications",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_email_pending",
                schema: "app",
                table: "notifications",
                column: "created_at",
                filter: "email_status = 'pending'");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_notification_type_code",
                schema: "app",
                table: "notifications",
                column: "notification_type_code");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_created_at",
                schema: "app",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_organisation_settings_updated_by_user_id",
                schema: "app",
                table: "organisation_settings",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_replaced_by_token_id",
                schema: "app",
                table: "refresh_tokens",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                schema: "app",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_refresh_tokens_token_hash",
                schema: "app",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_department_id",
                schema: "app",
                table: "users",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_role_code",
                schema: "app",
                table: "users",
                column: "role_code");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                schema: "app",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_assessment_reasons",
                schema: "app");

            migrationBuilder.DropTable(
                name: "booking_decisions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "booking_status_history",
                schema: "app");

            migrationBuilder.DropTable(
                name: "equipment_condition_changes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "equipment_item_features",
                schema: "app");

            migrationBuilder.DropTable(
                name: "loans",
                schema: "app");

            migrationBuilder.DropTable(
                name: "maintenance_records",
                schema: "app");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "app");

            migrationBuilder.DropTable(
                name: "organisation_settings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "app");

            migrationBuilder.DropTable(
                name: "approval_assessments",
                schema: "app");

            migrationBuilder.DropTable(
                name: "routing_reasons",
                schema: "app");

            migrationBuilder.DropTable(
                name: "decision_sources",
                schema: "app");

            migrationBuilder.DropTable(
                name: "features",
                schema: "app");

            migrationBuilder.DropTable(
                name: "maintenance_types",
                schema: "app");

            migrationBuilder.DropTable(
                name: "notification_types",
                schema: "app");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "booking_statuses",
                schema: "app");

            migrationBuilder.DropTable(
                name: "equipment_items",
                schema: "app");

            migrationBuilder.DropTable(
                name: "users",
                schema: "app");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "app");

            migrationBuilder.DropTable(
                name: "item_conditions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "item_statuses",
                schema: "app");

            migrationBuilder.DropTable(
                name: "locations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "app");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "app");
        }
    }
}
