-- Group roles. Roles are cluster wide, so they are created only when missing and never dropped by a down migration.
DO
$roles$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'app_read') THEN
        CREATE ROLE app_read NOLOGIN;
    END IF;
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'app_write') THEN
        CREATE ROLE app_write NOLOGIN;
    END IF;
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'app_admin') THEN
        CREATE ROLE app_admin NOLOGIN;
    END IF;
END
$roles$;

COMMENT ON ROLE app_read IS 'Read-only access to non-personal tables and masked views.';
COMMENT ON ROLE app_write IS 'Data access of the application: read and write application tables, no DDL.';
COMMENT ON ROLE app_admin IS 'Administration of the schema; the migrator belongs to this role.';

GRANT app_read TO app_write;
GRANT app_admin TO CURRENT_USER;

REVOKE ALL ON SCHEMA app FROM PUBLIC;
GRANT USAGE ON SCHEMA app TO app_read, app_write;
GRANT USAGE, CREATE ON SCHEMA app TO app_admin;

-- Application data access.
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA app TO app_write;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA app TO app_write;
REVOKE EXECUTE ON ALL FUNCTIONS IN SCHEMA app FROM PUBLIC;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA app TO app_write;

-- Reference data and the migration history are read only for the application.
REVOKE INSERT, UPDATE, DELETE ON
    app."__EFMigrationsHistory",
    app.roles,
    app.item_statuses,
    app.item_conditions,
    app.booking_statuses,
    app.decision_sources,
    app.routing_reasons,
    app.maintenance_types,
    app.notification_types
    FROM app_write;

-- History tables are append only; anonymisation may only clear the acting user of a status change.
REVOKE UPDATE, DELETE ON app.booking_status_history, app.equipment_condition_changes FROM app_write;
GRANT UPDATE (changed_by_user_id) ON app.booking_status_history TO app_write;

-- Objects created by later migrations get the same privileges automatically.
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT USAGE, SELECT ON SEQUENCES TO app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE EXECUTE ON FUNCTIONS FROM PUBLIC;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT EXECUTE ON FUNCTIONS TO app_write;

-- Read-only access: explicit grants, personal data only through the masked view.
GRANT SELECT ON
    app.roles,
    app.item_statuses,
    app.item_conditions,
    app.booking_statuses,
    app.decision_sources,
    app.routing_reasons,
    app.maintenance_types,
    app.notification_types,
    app.departments,
    app.locations,
    app.features,
    app.categories,
    app.equipment_items,
    app.equipment_item_features,
    app.equipment_condition_changes,
    app.bookings,
    app.booking_decisions,
    app.loans,
    app.booking_status_history,
    app.approval_assessments,
    app.approval_assessment_reasons,
    app.maintenance_records,
    app.organisation_settings,
    app.v_equipment_with_status,
    app.v_overdue_loans,
    app.v_requester_return_history,
    app.v_users_masked
    TO app_read;
GRANT EXECUTE ON FUNCTION
    app.fn_item_is_available(bigint, date, date),
    app.fn_item_effective_status(bigint, date)
    TO app_read;

-- Schema administration.
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA app TO app_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA app TO app_admin;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA app TO app_admin;
