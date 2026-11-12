using System;
using EquipmentBooking.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EquipmentBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntegrityRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_approval_assessments_bookings_booking_id",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_booking_decisions_bookings_booking_id",
                schema: "app",
                table: "booking_decisions");

            migrationBuilder.DropForeignKey(
                name: "fk_booking_status_history_bookings_booking_id",
                schema: "app",
                table: "booking_status_history");

            migrationBuilder.DropForeignKey(
                name: "fk_equipment_condition_changes_equipment_items_equipment_item_",
                schema: "app",
                table: "equipment_condition_changes");

            migrationBuilder.DropForeignKey(
                name: "fk_equipment_items_item_statuses_status_code",
                schema: "app",
                table: "equipment_items");

            migrationBuilder.DropForeignKey(
                name: "fk_loans_bookings_booking_id",
                schema: "app",
                table: "loans");

            migrationBuilder.DropCheckConstraint(
                name: "ck_notifications_email_status",
                schema: "app",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_equipment_items_status_code",
                schema: "app",
                table: "equipment_items");

            migrationBuilder.DropCheckConstraint(
                name: "ck_equipment_items_retired",
                schema: "app",
                table: "equipment_items");

            migrationBuilder.DropCheckConstraint(
                name: "ck_booking_decisions_outcome",
                schema: "app",
                table: "booking_decisions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_approval_assessments_approved_needs_match",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropCheckConstraint(
                name: "ck_approval_assessments_outcome",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropCheckConstraint(
                name: "ck_approval_assessments_purpose_result",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.AlterTable(
                name: "bookings",
                schema: "app",
                comment: "Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned; an overdue loan blocks the item until it is returned.",
                oldComment: "Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned.");

            migrationBuilder.AlterTable(
                name: "approval_assessments",
                schema: "app",
                comment: "Decision log of the approval route: the purpose check and the outcome; the results of the policy checks are in approval_checks.",
                oldComment: "Decision log of the approval route: result of every policy check, the purpose check and the outcome.");

            migrationBuilder.AlterColumn<string>(
                name: "email_status",
                schema: "app",
                table: "notifications",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                comment: "Delivery state of the e-mail copy; not_required while no provider is configured.",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldComment: "Delivery state of the e-mail copy; not_required while no provider is configured.");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "retired_at",
                schema: "app",
                table: "equipment_items",
                type: "timestamp with time zone",
                nullable: true,
                comment: "When the item was retired; null while the item is in use. The effective status is derived from this column and active maintenance.",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "When the item was retired; set together with the status retired.");

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason_code",
                schema: "app",
                table: "bookings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true,
                comment: "Why the booking was cancelled; set exactly when the status is cancelled.");

            migrationBuilder.AlterColumn<string>(
                name: "outcome",
                schema: "app",
                table: "booking_decisions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                comment: "Approved or declined; the assistant can only approve.",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldComment: "approved or declined; the assistant can only approve.");

            migrationBuilder.AlterColumn<string>(
                name: "purpose_result",
                schema: "app",
                table: "approval_assessments",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                comment: "Result of the purpose check by the language model.",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldComment: "Result of the purpose check by the language model.");

            migrationBuilder.AlterColumn<string>(
                name: "outcome",
                schema: "app",
                table: "approval_assessments",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                comment: "Pending, approved automatically or routed to the manager.",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldComment: "pending, approved or routed to the manager.");

            migrationBuilder.CreateTable(
                name: "approval_check_types",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    routing_reason_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Routing reason when the check fails."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_check_types", x => x.code);
                    table.ForeignKey(
                        name: "fk_approval_check_types_routing_reasons_routing_reason_code",
                        column: x => x.routing_reason_code,
                        principalSchema: "app",
                        principalTable: "routing_reasons",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Fixed policy checks of the approval route and the routing reason recorded when a check fails.");

            migrationBuilder.CreateTable(
                name: "assessment_outcomes",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_outcomes", x => x.code);
                },
                comment: "Outcomes of the approval route for a request.");

            migrationBuilder.CreateTable(
                name: "booking_status_transitions",
                schema: "app",
                columns: table => new
                {
                    from_status_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Current status."),
                    to_status_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Status the booking may change to.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_status_transitions", x => new { x.from_status_code, x.to_status_code });
                    table.ForeignKey(
                        name: "fk_booking_status_transitions_booking_statuses_from_status_code",
                        column: x => x.from_status_code,
                        principalSchema: "app",
                        principalTable: "booking_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_booking_status_transitions_booking_statuses_to_status_code",
                        column: x => x.to_status_code,
                        principalSchema: "app",
                        principalTable: "booking_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Allowed status changes of a booking, enforced by the trigger fn_check_booking_transition.");

            migrationBuilder.CreateTable(
                name: "cancellation_reasons",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cancellation_reasons", x => x.code);
                },
                comment: "Reasons why a booking was cancelled; every cancelled booking has one.");

            migrationBuilder.CreateTable(
                name: "decision_outcomes",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_decision_outcomes", x => x.code);
                },
                comment: "Outcomes of a decision on a request.");

            migrationBuilder.CreateTable(
                name: "email_statuses",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_statuses", x => x.code);
                },
                comment: "Delivery states of the e-mail copy of a notification.");

            migrationBuilder.CreateTable(
                name: "organisation_settings_history",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Surrogate key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()", comment: "When this version came into force."),
                    changed_by_user_id = table.Column<long>(type: "bigint", nullable: true, comment: "Administrator who made the change; null for the initial settings and after a user was removed."),
                    low_value_threshold = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, comment: "Low value threshold in EUR."),
                    auto_approval_enabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Organisation wide switch for automatic approval."),
                    retention_days = table.Column<int>(type: "integer", nullable: false, comment: "Retention period of personal data of loans in days."),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "IANA time zone of the organisation."),
                    return_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false, comment: "Local return time on the end date.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_settings_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_settings_history_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Every version of the organisation settings, written by trigger.");

            migrationBuilder.CreateTable(
                name: "purpose_results",
                schema: "app",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Stable code used by the application and constraints."),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Display name in English.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purpose_results", x => x.code);
                },
                comment: "Results of the purpose check by the language model.");

            migrationBuilder.CreateTable(
                name: "approval_checks",
                schema: "app",
                columns: table => new
                {
                    assessment_id = table.Column<long>(type: "bigint", nullable: false, comment: "Assessment the check belongs to."),
                    check_type_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Policy check."),
                    passed = table.Column<bool>(type: "boolean", nullable: false, comment: "The check passed."),
                    observed_value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true, comment: "Value of the request that was compared, for example the item value in EUR or the loan length in days; null for switches and flags."),
                    limit_value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true, comment: "Limit in force at the time of the assessment, for example the low value threshold or the maximum loan days; null for switches and flags.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_checks", x => new { x.assessment_id, x.check_type_code });
                    table.CheckConstraint("ck_approval_checks_values", "(observed_value IS NULL) = (limit_value IS NULL)");
                    table.ForeignKey(
                        name: "fk_approval_checks_approval_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalSchema: "app",
                        principalTable: "approval_assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_approval_checks_approval_check_types_check_type_code",
                        column: x => x.check_type_code,
                        principalSchema: "app",
                        principalTable: "approval_check_types",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Result of every policy check of an assessment with the values it compared.");

            migrationBuilder.InsertData(
                schema: "app",
                table: "approval_check_types",
                columns: new[] { "code", "name", "routing_reason_code" },
                values: new object[,]
                {
                    { "category_auto_approval", "Automatic approval is switched on for the category", "category_auto_approval_off" },
                    { "max_duration", "The loan does not exceed the maximum duration of the category", "duration_exceeded" },
                    { "not_specialised", "Neither the item nor its category is specialised", "specialised" },
                    { "organisation_auto_approval", "Automatic approval is switched on for the organisation", "org_auto_approval_off" },
                    { "value_threshold", "The item value is below the low value threshold", "high_value" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "assessment_outcomes",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "approved", "Approved automatically" },
                    { "pending", "Waiting for the purpose check" },
                    { "routed", "Routed to the department manager" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "booking_status_transitions",
                columns: new[] { "from_status_code", "to_status_code" },
                values: new object[,]
                {
                    { "approved", "cancelled" },
                    { "approved", "issued" },
                    { "approved", "requested" },
                    { "issued", "overdue" },
                    { "issued", "returned" },
                    { "overdue", "returned" },
                    { "requested", "approved" },
                    { "requested", "cancelled" },
                    { "requested", "declined" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "cancellation_reasons",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "by_requester", "Cancelled by the requester" },
                    { "maintenance", "Cancelled because the item is out of service for maintenance" },
                    { "not_collected", "The item was not collected by the end date" },
                    { "request_expired", "No decision was made by the start date" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "decision_outcomes",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "approved", "Approved" },
                    { "declined", "Declined" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "email_statuses",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "failed", "Delivery failed" },
                    { "not_required", "No e-mail copy is sent" },
                    { "pending", "Waiting for delivery" },
                    { "sent", "Sent" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "purpose_results",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "match", "The purpose matches the usage guidelines" },
                    { "no_match", "The purpose does not match the usage guidelines" },
                    { "not_run", "Not run because a policy check failed" },
                    { "pending", "Not completed yet" },
                    { "unavailable", "The assistant was not available" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_email_status",
                schema: "app",
                table: "notifications",
                column: "email_status");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_cancellation_reason_code",
                schema: "app",
                table: "bookings",
                column: "cancellation_reason_code");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bookings_anonymised_returned",
                schema: "app",
                table: "bookings",
                sql: "anonymised_at IS NULL OR status_code = 'returned'");

            migrationBuilder.CreateIndex(
                name: "ix_booking_decisions_outcome",
                schema: "app",
                table: "booking_decisions",
                column: "outcome");

            migrationBuilder.CreateIndex(
                name: "ix_approval_assessments_outcome",
                schema: "app",
                table: "approval_assessments",
                column: "outcome");

            migrationBuilder.CreateIndex(
                name: "ix_approval_assessments_purpose_result",
                schema: "app",
                table: "approval_assessments",
                column: "purpose_result");

            migrationBuilder.CreateIndex(
                name: "ux_approval_check_types_routing_reason_code",
                schema: "app",
                table: "approval_check_types",
                column: "routing_reason_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_approval_checks_check_type_code",
                schema: "app",
                table: "approval_checks",
                column: "check_type_code");

            migrationBuilder.CreateIndex(
                name: "ix_booking_status_transitions_to_status_code",
                schema: "app",
                table: "booking_status_transitions",
                column: "to_status_code");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_settings_history_changed_at",
                schema: "app",
                table: "organisation_settings_history",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_settings_history_changed_by_user_id",
                schema: "app",
                table: "organisation_settings_history",
                column: "changed_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_approval_assessments_assessment_outcomes_outcome",
                schema: "app",
                table: "approval_assessments",
                column: "outcome",
                principalSchema: "app",
                principalTable: "assessment_outcomes",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_approval_assessments_bookings_booking_id",
                schema: "app",
                table: "approval_assessments",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_approval_assessments_purpose_results_purpose_result",
                schema: "app",
                table: "approval_assessments",
                column: "purpose_result",
                principalSchema: "app",
                principalTable: "purpose_results",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_booking_decisions_bookings_booking_id",
                schema: "app",
                table: "booking_decisions",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_booking_decisions_decision_outcomes_outcome",
                schema: "app",
                table: "booking_decisions",
                column: "outcome",
                principalSchema: "app",
                principalTable: "decision_outcomes",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_booking_status_history_bookings_booking_id",
                schema: "app",
                table: "booking_status_history",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_cancellation_reasons_cancellation_reason_code",
                schema: "app",
                table: "bookings",
                column: "cancellation_reason_code",
                principalSchema: "app",
                principalTable: "cancellation_reasons",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_equipment_condition_changes_equipment_items_equipment_item_",
                schema: "app",
                table: "equipment_condition_changes",
                column: "equipment_item_id",
                principalSchema: "app",
                principalTable: "equipment_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_loans_bookings_booking_id",
                schema: "app",
                table: "loans",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_email_statuses_email_status",
                schema: "app",
                table: "notifications",
                column: "email_status",
                principalSchema: "app",
                principalTable: "email_statuses",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(SqlResource.Read("005_integrity_rules_data_up.sql"));

            migrationBuilder.DropTable(
                name: "approval_assessment_reasons",
                schema: "app");

            migrationBuilder.DropTable(
                name: "item_statuses",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "status_code",
                schema: "app",
                table: "equipment_items");

            migrationBuilder.DropColumn(
                name: "category_auto_approval_enabled",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropColumn(
                name: "duration_check_passed",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropColumn(
                name: "organisation_auto_approval_enabled",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropColumn(
                name: "specialised_check_passed",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropColumn(
                name: "value_check_passed",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bookings_cancellation_reason",
                schema: "app",
                table: "bookings",
                sql: "(status_code = 'cancelled') = (cancellation_reason_code IS NOT NULL)");


            migrationBuilder.Sql(SqlResource.Read("005_integrity_rules_up.sql"));
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("005_integrity_rules_down.sql"));

            migrationBuilder.AddColumn<string>(
                name: "status_code",
                schema: "app",
                table: "equipment_items",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                comment: "Stored status: available or retired.");

            migrationBuilder.AddColumn<bool>(
                name: "category_auto_approval_enabled",
                schema: "app",
                table: "approval_assessments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Automatic approval was switched on for the category.");

            migrationBuilder.AddColumn<bool>(
                name: "duration_check_passed",
                schema: "app",
                table: "approval_assessments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "The loan does not exceed the maximum duration of the category.");

            migrationBuilder.AddColumn<bool>(
                name: "organisation_auto_approval_enabled",
                schema: "app",
                table: "approval_assessments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Automatic approval was switched on for the organisation.");

            migrationBuilder.AddColumn<bool>(
                name: "specialised_check_passed",
                schema: "app",
                table: "approval_assessments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Neither the item nor its category is specialised.");

            migrationBuilder.AddColumn<bool>(
                name: "value_check_passed",
                schema: "app",
                table: "approval_assessments",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "The item value is below the low value threshold.");

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

            migrationBuilder.InsertData(
                schema: "app",
                table: "item_statuses",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "available", "Available" },
                    { "retired", "Retired" }
                });


            migrationBuilder.Sql(SqlResource.Read("005_integrity_rules_data_down.sql"));

            migrationBuilder.DropForeignKey(
                name: "fk_approval_assessments_assessment_outcomes_outcome",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_approval_assessments_bookings_booking_id",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_approval_assessments_purpose_results_purpose_result",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropForeignKey(
                name: "fk_booking_decisions_bookings_booking_id",
                schema: "app",
                table: "booking_decisions");

            migrationBuilder.DropForeignKey(
                name: "fk_booking_decisions_decision_outcomes_outcome",
                schema: "app",
                table: "booking_decisions");

            migrationBuilder.DropForeignKey(
                name: "fk_booking_status_history_bookings_booking_id",
                schema: "app",
                table: "booking_status_history");

            migrationBuilder.DropForeignKey(
                name: "fk_bookings_cancellation_reasons_cancellation_reason_code",
                schema: "app",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "fk_equipment_condition_changes_equipment_items_equipment_item_",
                schema: "app",
                table: "equipment_condition_changes");

            migrationBuilder.DropForeignKey(
                name: "fk_loans_bookings_booking_id",
                schema: "app",
                table: "loans");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_email_statuses_email_status",
                schema: "app",
                table: "notifications");

            migrationBuilder.DropTable(
                name: "approval_checks",
                schema: "app");

            migrationBuilder.DropTable(
                name: "assessment_outcomes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "booking_status_transitions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "cancellation_reasons",
                schema: "app");

            migrationBuilder.DropTable(
                name: "decision_outcomes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "email_statuses",
                schema: "app");

            migrationBuilder.DropTable(
                name: "organisation_settings_history",
                schema: "app");

            migrationBuilder.DropTable(
                name: "purpose_results",
                schema: "app");

            migrationBuilder.DropTable(
                name: "approval_check_types",
                schema: "app");

            migrationBuilder.DropIndex(
                name: "ix_notifications_email_status",
                schema: "app",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_bookings_cancellation_reason_code",
                schema: "app",
                table: "bookings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_bookings_anonymised_returned",
                schema: "app",
                table: "bookings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_bookings_cancellation_reason",
                schema: "app",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "ix_booking_decisions_outcome",
                schema: "app",
                table: "booking_decisions");

            migrationBuilder.DropIndex(
                name: "ix_approval_assessments_outcome",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropIndex(
                name: "ix_approval_assessments_purpose_result",
                schema: "app",
                table: "approval_assessments");

            migrationBuilder.DropColumn(
                name: "cancellation_reason_code",
                schema: "app",
                table: "bookings");

            migrationBuilder.AlterTable(
                name: "bookings",
                schema: "app",
                comment: "Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned.",
                oldComment: "Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned; an overdue loan blocks the item until it is returned.");

            migrationBuilder.AlterTable(
                name: "approval_assessments",
                schema: "app",
                comment: "Decision log of the approval route: result of every policy check, the purpose check and the outcome.",
                oldComment: "Decision log of the approval route: the purpose check and the outcome; the results of the policy checks are in approval_checks.");

            migrationBuilder.AlterColumn<string>(
                name: "email_status",
                schema: "app",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                comment: "Delivery state of the e-mail copy; not_required while no provider is configured.",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldComment: "Delivery state of the e-mail copy; not_required while no provider is configured.");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "retired_at",
                schema: "app",
                table: "equipment_items",
                type: "timestamp with time zone",
                nullable: true,
                comment: "When the item was retired; set together with the status retired.",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "When the item was retired; null while the item is in use. The effective status is derived from this column and active maintenance.");

            migrationBuilder.AlterColumn<string>(
                name: "outcome",
                schema: "app",
                table: "booking_decisions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                comment: "approved or declined; the assistant can only approve.",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldComment: "Approved or declined; the assistant can only approve.");

            migrationBuilder.AlterColumn<string>(
                name: "purpose_result",
                schema: "app",
                table: "approval_assessments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                comment: "Result of the purpose check by the language model.",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldComment: "Result of the purpose check by the language model.");

            migrationBuilder.AlterColumn<string>(
                name: "outcome",
                schema: "app",
                table: "approval_assessments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                comment: "pending, approved or routed to the manager.",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldComment: "Pending, approved automatically or routed to the manager.");

            migrationBuilder.AddCheckConstraint(
                name: "ck_notifications_email_status",
                schema: "app",
                table: "notifications",
                sql: "email_status IN ('pending', 'sent', 'failed', 'not_required')");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_items_status_code",
                schema: "app",
                table: "equipment_items",
                column: "status_code");

            migrationBuilder.AddCheckConstraint(
                name: "ck_equipment_items_retired",
                schema: "app",
                table: "equipment_items",
                sql: "(status_code = 'retired') = (retired_at IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_booking_decisions_outcome",
                schema: "app",
                table: "booking_decisions",
                sql: "outcome IN ('approved', 'declined')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_approval_assessments_approved_needs_match",
                schema: "app",
                table: "approval_assessments",
                sql: "outcome <> 'approved' OR purpose_result = 'match'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_approval_assessments_outcome",
                schema: "app",
                table: "approval_assessments",
                sql: "outcome IN ('pending', 'approved', 'routed')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_approval_assessments_purpose_result",
                schema: "app",
                table: "approval_assessments",
                sql: "purpose_result IN ('pending', 'match', 'no_match', 'unavailable', 'not_run')");

            migrationBuilder.CreateIndex(
                name: "ix_approval_assessment_reasons_routing_reason_code",
                schema: "app",
                table: "approval_assessment_reasons",
                column: "routing_reason_code");

            migrationBuilder.AddForeignKey(
                name: "fk_approval_assessments_bookings_booking_id",
                schema: "app",
                table: "approval_assessments",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_booking_decisions_bookings_booking_id",
                schema: "app",
                table: "booking_decisions",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_booking_status_history_bookings_booking_id",
                schema: "app",
                table: "booking_status_history",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_equipment_condition_changes_equipment_items_equipment_item_",
                schema: "app",
                table: "equipment_condition_changes",
                column: "equipment_item_id",
                principalSchema: "app",
                principalTable: "equipment_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_equipment_items_item_statuses_status_code",
                schema: "app",
                table: "equipment_items",
                column: "status_code",
                principalSchema: "app",
                principalTable: "item_statuses",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_loans_bookings_booking_id",
                schema: "app",
                table: "loans",
                column: "booking_id",
                principalSchema: "app",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
