DROP INDEX IF EXISTS app.ix_equipment_items_name_trgm;
DROP INDEX IF EXISTS app.ix_equipment_items_search;

DROP VIEW IF EXISTS app.v_users_masked;
DROP VIEW IF EXISTS app.v_requester_return_history;
DROP VIEW IF EXISTS app.v_overdue_loans;
DROP VIEW IF EXISTS app.v_equipment_with_status;

DROP FUNCTION IF EXISTS app.fn_item_effective_status(bigint, date);
DROP FUNCTION IF EXISTS app.fn_item_is_available(bigint, date, date);

DROP TRIGGER IF EXISTS trg_bookings_maintenance_check ON app.bookings;
DROP FUNCTION IF EXISTS app.fn_check_booking_maintenance();

DROP TRIGGER IF EXISTS trg_equipment_items_condition_history ON app.equipment_items;
DROP FUNCTION IF EXISTS app.fn_log_condition_change();

DROP TRIGGER IF EXISTS trg_bookings_status_history ON app.bookings;
DROP FUNCTION IF EXISTS app.fn_log_booking_status();

DROP FUNCTION IF EXISTS app.fn_acting_user_id();

DROP TRIGGER IF EXISTS trg_organisation_settings_updated_at ON app.organisation_settings;
DROP TRIGGER IF EXISTS trg_bookings_updated_at ON app.bookings;
DROP TRIGGER IF EXISTS trg_equipment_items_updated_at ON app.equipment_items;
DROP TRIGGER IF EXISTS trg_users_updated_at ON app.users;
DROP FUNCTION IF EXISTS app.fn_set_updated_at();

ALTER TABLE app.bookings DROP CONSTRAINT IF EXISTS ex_bookings_no_overlap;
