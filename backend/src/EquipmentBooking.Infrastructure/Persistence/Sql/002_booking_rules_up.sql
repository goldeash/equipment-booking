-- No two blocking bookings of the same item may overlap (US-07, KPI-4).
-- The status list must match BookingStatusCodes.Blocking in the domain model.
ALTER TABLE app.bookings
    ADD CONSTRAINT ex_bookings_no_overlap
    EXCLUDE USING gist (equipment_item_id WITH =, daterange(start_date, end_date, '[]') WITH &&)
    WHERE (status_code IN ('requested', 'approved', 'issued', 'overdue'));

COMMENT ON CONSTRAINT ex_bookings_no_overlap ON app.bookings IS
    'Rejects a booking whose dates overlap another requested, approved, issued or overdue booking of the same item.';

-- Maintains updated_at on every update.
CREATE FUNCTION app.fn_set_updated_at() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    NEW.updated_at := now();
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_set_updated_at() IS 'Trigger function that sets updated_at to the current time.';

CREATE TRIGGER trg_users_updated_at BEFORE UPDATE ON app.users
    FOR EACH ROW EXECUTE FUNCTION app.fn_set_updated_at();
CREATE TRIGGER trg_equipment_items_updated_at BEFORE UPDATE ON app.equipment_items
    FOR EACH ROW EXECUTE FUNCTION app.fn_set_updated_at();
CREATE TRIGGER trg_bookings_updated_at BEFORE UPDATE ON app.bookings
    FOR EACH ROW EXECUTE FUNCTION app.fn_set_updated_at();
CREATE TRIGGER trg_organisation_settings_updated_at BEFORE UPDATE ON app.organisation_settings
    FOR EACH ROW EXECUTE FUNCTION app.fn_set_updated_at();

-- The API passes the acting user for the current transaction with set_config('app.user_id', ...).
CREATE FUNCTION app.fn_acting_user_id() RETURNS bigint
    LANGUAGE sql STABLE AS
$$
SELECT NULLIF(current_setting('app.user_id', true), '')::bigint;
$$;

COMMENT ON FUNCTION app.fn_acting_user_id() IS 'User id set by the API for the current transaction, or null for background jobs.';

-- Writes every status change of a booking to the history.
CREATE FUNCTION app.fn_log_booking_status() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF TG_OP = 'INSERT' OR NEW.status_code IS DISTINCT FROM OLD.status_code THEN
        INSERT INTO app.booking_status_history (booking_id, from_status_code, to_status_code, changed_at, changed_by_user_id)
        VALUES (
            NEW.id,
            CASE WHEN TG_OP = 'INSERT' THEN NULL ELSE OLD.status_code END,
            NEW.status_code,
            now(),
            app.fn_acting_user_id());
    END IF;
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_log_booking_status() IS 'Trigger function that records booking status changes in booking_status_history.';

CREATE TRIGGER trg_bookings_status_history AFTER INSERT OR UPDATE OF status_code ON app.bookings
    FOR EACH ROW EXECUTE FUNCTION app.fn_log_booking_status();

-- Writes every condition change of an item to the history (US-32).
CREATE FUNCTION app.fn_log_condition_change() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF TG_OP = 'INSERT' OR NEW.condition_code IS DISTINCT FROM OLD.condition_code THEN
        INSERT INTO app.equipment_condition_changes (equipment_item_id, from_condition_code, to_condition_code, changed_at, changed_by_user_id)
        VALUES (
            NEW.id,
            CASE WHEN TG_OP = 'INSERT' THEN NULL ELSE OLD.condition_code END,
            NEW.condition_code,
            now(),
            app.fn_acting_user_id());
    END IF;
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_log_condition_change() IS 'Trigger function that records condition changes in equipment_condition_changes.';

CREATE TRIGGER trg_equipment_items_condition_history AFTER INSERT OR UPDATE OF condition_code ON app.equipment_items
    FOR EACH ROW EXECUTE FUNCTION app.fn_log_condition_change();

-- A blocking booking must not overlap an out of service maintenance period (US-31).
-- The item row is locked so that a booking and a maintenance record saved at the same time are serialised.
CREATE FUNCTION app.fn_check_booking_maintenance() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF NEW.status_code NOT IN ('requested', 'approved', 'issued', 'overdue') THEN
        RETURN NEW;
    END IF;

    PERFORM 1 FROM app.equipment_items WHERE id = NEW.equipment_item_id FOR UPDATE;

    IF EXISTS (
        SELECT 1
        FROM app.maintenance_records m
        WHERE m.equipment_item_id = NEW.equipment_item_id
          AND m.takes_out_of_service
          AND m.start_date <= NEW.end_date
          AND (m.end_date IS NULL OR m.end_date >= NEW.start_date)) THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB001',
            MESSAGE = 'The item is out of service for maintenance in the requested period.';
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_booking_maintenance() IS
    'Trigger function that rejects bookings overlapping an out of service maintenance period (SQLSTATE EB001).';

CREATE TRIGGER trg_bookings_maintenance_check BEFORE INSERT OR UPDATE OF start_date, end_date ON app.bookings
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_booking_maintenance();

-- True when the item is not retired, has no blocking booking and no out of service maintenance in the range.
CREATE FUNCTION app.fn_item_is_available(p_item_id bigint, p_start date, p_end date) RETURNS boolean
    LANGUAGE sql STABLE AS
$$
SELECT EXISTS (SELECT 1 FROM app.equipment_items i WHERE i.id = p_item_id AND i.status_code <> 'retired')
   AND NOT EXISTS (
        SELECT 1 FROM app.bookings b
        WHERE b.equipment_item_id = p_item_id
          AND b.status_code IN ('requested', 'approved', 'issued', 'overdue')
          AND daterange(b.start_date, b.end_date, '[]') && daterange(p_start, p_end, '[]'))
   AND NOT EXISTS (
        SELECT 1 FROM app.maintenance_records m
        WHERE m.equipment_item_id = p_item_id
          AND m.takes_out_of_service
          AND m.start_date <= p_end
          AND (m.end_date IS NULL OR m.end_date >= p_start));
$$;

COMMENT ON FUNCTION app.fn_item_is_available(bigint, date, date) IS 'Availability of an item for an inclusive date range.';

-- Effective status of an item on a date: retired, out_of_service (active maintenance) or available.
CREATE FUNCTION app.fn_item_effective_status(p_item_id bigint, p_on date) RETURNS text
    LANGUAGE sql STABLE AS
$$
SELECT CASE
           WHEN i.status_code = 'retired' THEN 'retired'
           WHEN EXISTS (
               SELECT 1 FROM app.maintenance_records m
               WHERE m.equipment_item_id = i.id
                 AND m.takes_out_of_service
                 AND m.start_date <= p_on
                 AND (m.end_date IS NULL OR m.end_date >= p_on)) THEN 'out_of_service'
           ELSE 'available'
       END
FROM app.equipment_items i
WHERE i.id = p_item_id;
$$;

COMMENT ON FUNCTION app.fn_item_effective_status(bigint, date) IS 'Status of an item on a date, deriving out_of_service from maintenance.';

-- Items with their effective status today.
CREATE VIEW app.v_equipment_with_status AS
SELECT i.id,
       i.inventory_number,
       i.name,
       c.name AS category_name,
       l.name AS location_name,
       i.value,
       i.condition_code,
       app.fn_item_effective_status(i.id, current_date) AS effective_status
FROM app.equipment_items i
         JOIN app.categories c ON c.id = i.category_id
         JOIN app.locations l ON l.id = i.location_id;

COMMENT ON VIEW app.v_equipment_with_status IS 'Equipment items with category, location and effective status on the current date.';

-- Loans that are overdue now.
CREATE VIEW app.v_overdue_loans AS
SELECT b.id AS booking_id,
       b.equipment_item_id,
       i.inventory_number,
       i.name AS item_name,
       b.requester_id,
       l.issued_at,
       l.due_at,
       now() - l.due_at AS overdue_for
FROM app.bookings b
         JOIN app.loans l ON l.booking_id = b.id
         JOIN app.equipment_items i ON i.id = b.equipment_item_id
WHERE b.status_code = 'overdue';

COMMENT ON VIEW app.v_overdue_loans IS 'Loans with the status overdue and how long they are overdue.';

-- Return history of each requester for the manager (US-12). Anonymised loans are not counted.
CREATE VIEW app.v_requester_return_history AS
SELECT b.requester_id AS user_id,
       count(*)::integer AS loan_count,
       (count(*) FILTER (WHERE l.returned_at > l.due_at))::integer AS late_return_count,
       max(l.returned_at) FILTER (WHERE l.returned_at > l.due_at) AS last_late_return_at
FROM app.bookings b
         JOIN app.loans l ON l.booking_id = b.id
WHERE b.requester_id IS NOT NULL
GROUP BY b.requester_id;

COMMENT ON VIEW app.v_requester_return_history IS 'Number of loans, late returns and the last late return of every requester.';

-- Users with masked personal data for read-only access.
CREATE VIEW app.v_users_masked AS
SELECT u.id,
       left(u.email::text, 1) || '***@' || split_part(u.email::text, '@', 2) AS email_masked,
       left(u.full_name, 1) || '***' AS full_name_masked,
       u.role_code,
       u.department_id,
       u.is_active
FROM app.users u;

COMMENT ON VIEW app.v_users_masked IS 'Users with masked e-mail and name; granted to the read-only role instead of the users table.';

-- Keyword search over name and description (US-03); the expression matches the query of the API.
CREATE INDEX ix_equipment_items_search ON app.equipment_items
    USING gin (to_tsvector('english'::regconfig, (name::text || ' ' || coalesce(description, '')::text)));

-- Partial name search with ILIKE '%term%'.
CREATE INDEX ix_equipment_items_name_trgm ON app.equipment_items USING gin (name public.gin_trgm_ops);
