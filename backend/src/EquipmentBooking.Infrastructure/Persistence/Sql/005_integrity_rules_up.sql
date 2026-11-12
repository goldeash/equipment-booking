-- Business rules enforced by the database in addition to the domain model. Every rule raises its own
-- SQLSTATE so that the API can return a meaningful error:
--   EB001 blocking booking and out of service maintenance overlap
--   EB002 booking status change that is not in booking_status_transitions
--   EB003 new booking of an item that has an overdue loan
--   EB004 action recorded by a user without the required role, or a manager deciding an own request
--   EB005 booking whose status does not match its loan and current decision
--   EB006 assessment whose outcome does not match its checks and purpose result

-- Availability and effective status: an item is retired when it has a retirement time; an overdue loan
-- blocks the item until it is returned.
CREATE OR REPLACE FUNCTION app.fn_item_is_available(p_item_id bigint, p_start date, p_end date) RETURNS boolean
    LANGUAGE sql STABLE AS
$$
SELECT EXISTS (SELECT 1 FROM app.equipment_items i WHERE i.id = p_item_id AND i.retired_at IS NULL)
   AND NOT EXISTS (
        SELECT 1 FROM app.bookings b
        WHERE b.equipment_item_id = p_item_id
          AND (b.status_code = 'overdue'
               OR (b.status_code IN ('requested', 'approved', 'issued')
                   AND daterange(b.start_date, b.end_date, '[]') && daterange(p_start, p_end, '[]'))))
   AND NOT EXISTS (
        SELECT 1 FROM app.maintenance_records m
        WHERE m.equipment_item_id = p_item_id
          AND m.takes_out_of_service
          AND m.start_date <= p_end
          AND (m.end_date IS NULL OR m.end_date >= p_start));
$$;

COMMENT ON FUNCTION app.fn_item_is_available(bigint, date, date) IS
    'Availability of an item for an inclusive date range; an overdue loan blocks the item until it is returned.';

CREATE OR REPLACE FUNCTION app.fn_item_effective_status(p_item_id bigint, p_on date) RETURNS text
    LANGUAGE sql STABLE AS
$$
SELECT CASE
           WHEN i.retired_at IS NOT NULL THEN 'retired'
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

COMMENT ON FUNCTION app.fn_item_effective_status(bigint, date) IS
    'Status of an item on a date: retired, out_of_service (active maintenance) or available.';

-- Overdue loans are computed from the due time, so the view is correct even before the overdue job has run.
DROP VIEW app.v_overdue_loans;

CREATE VIEW app.v_overdue_loans AS
SELECT b.id AS booking_id,
       b.equipment_item_id,
       i.inventory_number,
       i.name AS item_name,
       b.requester_id,
       b.status_code,
       l.issued_at,
       l.due_at,
       now() - l.due_at AS overdue_for
FROM app.loans l
         JOIN app.bookings b ON b.id = l.booking_id
         JOIN app.equipment_items i ON i.id = b.equipment_item_id
WHERE l.returned_at IS NULL
  AND l.due_at < now();

COMMENT ON VIEW app.v_overdue_loans IS
    'Loans that are not returned after their due time, whether or not the overdue job has already changed the status.';

-- Routing reasons are derived from the failed checks and the purpose result instead of being stored twice.
CREATE VIEW app.v_assessment_routing_reasons AS
SELECT c.assessment_id, t.routing_reason_code
FROM app.approval_checks c
         JOIN app.approval_check_types t ON t.code = c.check_type_code
WHERE NOT c.passed
UNION ALL
SELECT a.id,
       CASE a.purpose_result WHEN 'no_match' THEN 'purpose_not_confirmed' ELSE 'assistant_unavailable' END
FROM app.approval_assessments a
WHERE a.purpose_result IN ('no_match', 'unavailable');

COMMENT ON VIEW app.v_assessment_routing_reasons IS
    'Reasons why a request was routed: one row per failed policy check and one for a purpose that was not confirmed or not checked.';

-- Status changes must be listed in booking_status_transitions; a new booking always starts as requested.
CREATE FUNCTION app.fn_check_booking_transition() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF TG_OP = 'INSERT' THEN
        IF NEW.status_code <> 'requested' THEN
            RAISE EXCEPTION USING
                ERRCODE = 'EB002',
                MESSAGE = format('A new booking must start with the status requested, not %s.', NEW.status_code);
        END IF;
    ELSIF NEW.status_code IS DISTINCT FROM OLD.status_code
        AND NOT EXISTS (SELECT 1 FROM app.booking_status_transitions t
                        WHERE t.from_status_code = OLD.status_code AND t.to_status_code = NEW.status_code) THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB002',
            MESSAGE = format('A booking cannot change from %s to %s.', OLD.status_code, NEW.status_code);
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_booking_transition() IS
    'Trigger function that rejects booking status changes missing from booking_status_transitions (SQLSTATE EB002).';

CREATE TRIGGER trg_bookings_transition BEFORE INSERT OR UPDATE OF status_code ON app.bookings
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_booking_transition();

-- An overdue loan blocks its item until the item is returned: no new booking, no change of dates and no
-- issue of another booking. Bookings made before the loan became overdue stay, so their approval is still
-- possible. The item row is locked so that the overdue job and a new booking are serialised.
CREATE FUNCTION app.fn_check_item_overdue() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF TG_OP = 'UPDATE' AND NEW.status_code = 'overdue' AND OLD.status_code <> 'overdue' THEN
        PERFORM 1 FROM app.equipment_items WHERE id = NEW.equipment_item_id FOR UPDATE;
        RETURN NEW;
    END IF;

    IF NEW.status_code NOT IN ('requested', 'approved', 'issued') THEN
        RETURN NEW;
    END IF;

    IF TG_OP = 'UPDATE'
        AND NEW.start_date = OLD.start_date
        AND NEW.end_date = OLD.end_date
        AND NOT (NEW.status_code = 'issued' AND OLD.status_code <> 'issued') THEN
        RETURN NEW;
    END IF;

    PERFORM 1 FROM app.equipment_items WHERE id = NEW.equipment_item_id FOR UPDATE;

    IF EXISTS (SELECT 1 FROM app.bookings o
               WHERE o.equipment_item_id = NEW.equipment_item_id
                 AND o.status_code = 'overdue'
                 AND o.id <> NEW.id) THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB003',
            MESSAGE = 'The item has not been returned from an overdue loan yet.';
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_item_overdue() IS
    'Trigger function that keeps an item with an overdue loan blocked until it is returned (SQLSTATE EB003).';

CREATE TRIGGER trg_bookings_item_overdue BEFORE INSERT OR UPDATE OF start_date, end_date, status_code ON app.bookings
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_item_overdue();

-- The maintenance side of the rule that out of service periods and blocking bookings never overlap. Both
-- triggers lock the item row, so a booking and a maintenance record saved at the same time are serialised.
CREATE FUNCTION app.fn_check_maintenance_bookings() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF NOT NEW.takes_out_of_service THEN
        RETURN NEW;
    END IF;

    PERFORM 1 FROM app.equipment_items WHERE id = NEW.equipment_item_id FOR UPDATE;

    IF EXISTS (
        SELECT 1
        FROM app.bookings b
        WHERE b.equipment_item_id = NEW.equipment_item_id
          AND b.status_code IN ('requested', 'approved', 'issued', 'overdue')
          AND b.start_date <= coalesce(NEW.end_date, 'infinity'::date)
          AND b.end_date >= NEW.start_date) THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB001',
            MESSAGE = 'Blocking bookings exist in the maintenance period; cancel them first.';
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_maintenance_bookings() IS
    'Trigger function that rejects out of service maintenance overlapping blocking bookings (SQLSTATE EB001).';

CREATE TRIGGER trg_maintenance_records_bookings_check
    BEFORE INSERT OR UPDATE OF equipment_item_id, start_date, end_date, takes_out_of_service ON app.maintenance_records
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_maintenance_bookings();

-- Who may record an action: decisions only by an active manager of the requester's department who is not
-- the requester; issue, return and maintenance only by an active administrator; settings changes only by
-- an administrator.
CREATE FUNCTION app.fn_ensure_user_role(p_user_id bigint, p_role_code text, p_action text) RETURNS void
    LANGUAGE plpgsql STABLE AS
$$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM app.users u WHERE u.id = p_user_id AND u.role_code = p_role_code AND u.is_active) THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB004',
            MESSAGE = format('User %s is not an active %s and cannot %s.', p_user_id, p_role_code, p_action);
    END IF;
END;
$$;

COMMENT ON FUNCTION app.fn_ensure_user_role(bigint, text, text) IS
    'Raises SQLSTATE EB004 unless the user is active and has the role.';

CREATE FUNCTION app.fn_check_decision_maker() RETURNS trigger
    LANGUAGE plpgsql AS
$$
DECLARE
    v_requester_id  bigint;
    v_department_id bigint;
BEGIN
    IF NEW.decided_by_user_id IS NULL THEN
        RETURN NEW;
    END IF;

    PERFORM app.fn_ensure_user_role(NEW.decided_by_user_id, 'manager', 'decide on requests');

    SELECT b.requester_id, u.department_id
    INTO v_requester_id, v_department_id
    FROM app.bookings b
             JOIN app.users u ON u.id = b.requester_id
    WHERE b.id = NEW.booking_id;

    IF v_requester_id = NEW.decided_by_user_id THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB004',
            MESSAGE = 'A manager cannot decide on an own request.';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM app.users m WHERE m.id = NEW.decided_by_user_id AND m.department_id = v_department_id) THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB004',
            MESSAGE = 'A manager can only decide on requests of the own department.';
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_decision_maker() IS
    'Trigger function: a decision is made by an active manager of the requester''s department who is not the requester (SQLSTATE EB004).';

CREATE TRIGGER trg_booking_decisions_decision_maker BEFORE INSERT ON app.booking_decisions
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_decision_maker();

CREATE FUNCTION app.fn_check_loan_actors() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF TG_OP = 'INSERT' THEN
        PERFORM app.fn_ensure_user_role(NEW.issued_by_user_id, 'administrator', 'issue items');
    END IF;

    IF NEW.received_by_user_id IS NOT NULL
        AND (TG_OP = 'INSERT' OR NEW.received_by_user_id IS DISTINCT FROM OLD.received_by_user_id) THEN
        PERFORM app.fn_ensure_user_role(NEW.received_by_user_id, 'administrator', 'receive returned items');
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_loan_actors() IS
    'Trigger function: items are issued and received only by active administrators (SQLSTATE EB004).';

CREATE TRIGGER trg_loans_actors BEFORE INSERT OR UPDATE OF received_by_user_id ON app.loans
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_loan_actors();

CREATE FUNCTION app.fn_check_maintenance_actor() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    PERFORM app.fn_ensure_user_role(NEW.recorded_by_user_id, 'administrator', 'record maintenance');
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_maintenance_actor() IS
    'Trigger function: maintenance is recorded only by active administrators (SQLSTATE EB004).';

CREATE TRIGGER trg_maintenance_records_actor BEFORE INSERT ON app.maintenance_records
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_maintenance_actor();

CREATE FUNCTION app.fn_check_settings_actor() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF NEW.updated_by_user_id IS NOT NULL
        AND (TG_OP = 'INSERT' OR NEW.updated_by_user_id IS DISTINCT FROM OLD.updated_by_user_id) THEN
        PERFORM app.fn_ensure_user_role(NEW.updated_by_user_id, 'administrator', 'change the settings');
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_check_settings_actor() IS
    'Trigger function: the organisation settings are changed only by active administrators (SQLSTATE EB004).';

CREATE TRIGGER trg_organisation_settings_actor BEFORE INSERT OR UPDATE OF updated_by_user_id ON app.organisation_settings
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_settings_actor();

-- Every version of the settings is kept, so a past decision can be related to the settings in force.
CREATE FUNCTION app.fn_log_settings_change() RETURNS trigger
    LANGUAGE plpgsql AS
$$
BEGIN
    IF TG_OP = 'INSERT'
        OR (NEW.low_value_threshold, NEW.auto_approval_enabled, NEW.retention_days, NEW.time_zone, NEW.return_time)
           IS DISTINCT FROM
           (OLD.low_value_threshold, OLD.auto_approval_enabled, OLD.retention_days, OLD.time_zone, OLD.return_time) THEN
        INSERT INTO app.organisation_settings_history (changed_at, changed_by_user_id, low_value_threshold, auto_approval_enabled, retention_days, time_zone, return_time)
        VALUES (now(), NEW.updated_by_user_id, NEW.low_value_threshold, NEW.auto_approval_enabled, NEW.retention_days, NEW.time_zone, NEW.return_time);
    END IF;

    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION app.fn_log_settings_change() IS 'Trigger function that records every version of the organisation settings.';

CREATE TRIGGER trg_organisation_settings_history AFTER INSERT OR UPDATE ON app.organisation_settings
    FOR EACH ROW EXECUTE FUNCTION app.fn_log_settings_change();

-- Consistency of a booking with its loan and its current decision. The check runs at commit, after the
-- booking, its decision and its loan have all been written in the same transaction.
CREATE FUNCTION app.fn_booking_consistency_error(p_booking_id bigint) RETURNS text
    LANGUAGE plpgsql STABLE AS
$$
DECLARE
    v_status      text;
    v_has_loan    boolean;
    v_returned_at timestamptz;
    v_outcome     text;
BEGIN
    SELECT b.status_code, l.booking_id IS NOT NULL, l.returned_at, d.outcome
    INTO v_status, v_has_loan, v_returned_at, v_outcome
    FROM app.bookings b
             LEFT JOIN app.loans l ON l.booking_id = b.id
             LEFT JOIN app.booking_decisions d ON d.booking_id = b.id AND d.superseded_at IS NULL
    WHERE b.id = p_booking_id;

    IF NOT FOUND THEN
        RETURN NULL;
    END IF;

    IF v_has_loan <> (v_status IN ('issued', 'overdue', 'returned')) THEN
        RETURN format('status %s %s a loan', v_status, CASE WHEN v_has_loan THEN 'must not have' ELSE 'requires' END);
    END IF;

    IF v_status = 'returned' AND v_returned_at IS NULL THEN
        RETURN 'status returned requires a return time';
    END IF;

    IF v_status IN ('issued', 'overdue') AND v_returned_at IS NOT NULL THEN
        RETURN format('status %s must not have a return time', v_status);
    END IF;

    IF v_status IN ('approved', 'issued', 'overdue', 'returned') AND v_outcome IS DISTINCT FROM 'approved' THEN
        RETURN format('status %s requires a current approval', v_status);
    END IF;

    IF v_status = 'declined' AND v_outcome IS DISTINCT FROM 'declined' THEN
        RETURN 'status declined requires a current decline';
    END IF;

    IF v_status = 'requested' AND v_outcome IS NOT NULL THEN
        RETURN 'status requested must not have a current decision';
    END IF;

    IF v_status = 'cancelled' AND v_outcome = 'declined' THEN
        RETURN 'a declined request cannot be cancelled';
    END IF;

    RETURN NULL;
END;
$$;

COMMENT ON FUNCTION app.fn_booking_consistency_error(bigint) IS
    'Describes why a booking does not match its loan and current decision, or returns null when it is consistent.';

CREATE FUNCTION app.fn_check_booking_consistency() RETURNS trigger
    LANGUAGE plpgsql AS
$$
DECLARE
    v_booking_id bigint;
    v_error      text;
BEGIN
    IF TG_TABLE_NAME = 'bookings' THEN
        v_booking_id := NEW.id;
    ELSIF TG_OP = 'DELETE' THEN
        v_booking_id := OLD.booking_id;
    ELSE
        v_booking_id := NEW.booking_id;
    END IF;

    v_error := app.fn_booking_consistency_error(v_booking_id);
    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB005',
            MESSAGE = format('Booking %s is inconsistent: %s.', v_booking_id, v_error);
    END IF;

    RETURN NULL;
END;
$$;

COMMENT ON FUNCTION app.fn_check_booking_consistency() IS
    'Deferred trigger function that keeps the status of a booking consistent with its loan and current decision (SQLSTATE EB005).';

CREATE CONSTRAINT TRIGGER trg_bookings_consistency AFTER INSERT OR UPDATE ON app.bookings
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_booking_consistency();

CREATE CONSTRAINT TRIGGER trg_loans_consistency AFTER INSERT OR UPDATE OR DELETE ON app.loans
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_booking_consistency();

CREATE CONSTRAINT TRIGGER trg_booking_decisions_consistency AFTER INSERT OR UPDATE OR DELETE ON app.booking_decisions
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_booking_consistency();

-- Consistency of an assessment: one result for every check type, and an outcome that follows from the
-- checks and the purpose result. Approved only when every check passed and the purpose matched.
CREATE FUNCTION app.fn_assessment_consistency_error(p_assessment_id bigint) RETURNS text
    LANGUAGE plpgsql STABLE AS
$$
DECLARE
    v_outcome    text;
    v_purpose    text;
    v_checks     integer;
    v_all_passed boolean;
BEGIN
    SELECT a.outcome, a.purpose_result
    INTO v_outcome, v_purpose
    FROM app.approval_assessments a
    WHERE a.id = p_assessment_id;

    IF NOT FOUND THEN
        RETURN NULL;
    END IF;

    SELECT count(*), coalesce(bool_and(c.passed), true)
    INTO v_checks, v_all_passed
    FROM app.approval_checks c
    WHERE c.assessment_id = p_assessment_id;

    IF v_checks <> (SELECT count(*) FROM app.approval_check_types) THEN
        RETURN format('%s of %s policy checks recorded', v_checks, (SELECT count(*) FROM app.approval_check_types));
    END IF;

    IF v_outcome = 'pending' AND NOT (v_all_passed AND v_purpose = 'pending') THEN
        RETURN 'a pending assessment requires passed checks and a pending purpose check';
    END IF;

    IF v_outcome = 'approved' AND NOT (v_all_passed AND v_purpose = 'match') THEN
        RETURN 'an approval requires passed checks and a matching purpose';
    END IF;

    IF v_outcome = 'routed'
        AND NOT ((NOT v_all_passed AND v_purpose = 'not_run')
                 OR (v_all_passed AND v_purpose IN ('no_match', 'unavailable'))) THEN
        RETURN 'a routed assessment requires a failed check or an unconfirmed purpose';
    END IF;

    RETURN NULL;
END;
$$;

COMMENT ON FUNCTION app.fn_assessment_consistency_error(bigint) IS
    'Describes why an assessment outcome does not follow from its checks and purpose result, or returns null.';

CREATE FUNCTION app.fn_check_assessment_consistency() RETURNS trigger
    LANGUAGE plpgsql AS
$$
DECLARE
    v_assessment_id bigint;
    v_error         text;
BEGIN
    IF TG_TABLE_NAME = 'approval_assessments' THEN
        v_assessment_id := NEW.id;
    ELSIF TG_OP = 'DELETE' THEN
        v_assessment_id := OLD.assessment_id;
    ELSE
        v_assessment_id := NEW.assessment_id;
    END IF;

    v_error := app.fn_assessment_consistency_error(v_assessment_id);
    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION USING
            ERRCODE = 'EB006',
            MESSAGE = format('Assessment %s is inconsistent: %s.', v_assessment_id, v_error);
    END IF;

    RETURN NULL;
END;
$$;

COMMENT ON FUNCTION app.fn_check_assessment_consistency() IS
    'Deferred trigger function that keeps the outcome of an assessment consistent with its checks (SQLSTATE EB006).';

CREATE CONSTRAINT TRIGGER trg_approval_assessments_consistency AFTER INSERT OR UPDATE ON app.approval_assessments
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_assessment_consistency();

CREATE CONSTRAINT TRIGGER trg_approval_checks_consistency AFTER INSERT OR UPDATE OR DELETE ON app.approval_checks
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION app.fn_check_assessment_consistency();

-- Existing data must already satisfy the new rules.
DO
$validate$
DECLARE
    v_error text;
BEGIN
    SELECT format('booking %s: %s', b.id, app.fn_booking_consistency_error(b.id))
    INTO v_error
    FROM app.bookings b
    WHERE app.fn_booking_consistency_error(b.id) IS NOT NULL
    LIMIT 1;
    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION 'Existing data violate the booking rules: %', v_error;
    END IF;

    SELECT format('assessment %s: %s', a.id, app.fn_assessment_consistency_error(a.id))
    INTO v_error
    FROM app.approval_assessments a
    WHERE app.fn_assessment_consistency_error(a.id) IS NOT NULL
    LIMIT 1;
    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION 'Existing data violate the assessment rules: %', v_error;
    END IF;
END
$validate$;

-- Privileges of the new objects. Reference data are read only for the application, check results and
-- the settings history are append only.
REVOKE INSERT, UPDATE, DELETE ON
    app.approval_check_types,
    app.assessment_outcomes,
    app.purpose_results,
    app.decision_outcomes,
    app.email_statuses,
    app.cancellation_reasons,
    app.booking_status_transitions
    FROM app_write;

REVOKE UPDATE, DELETE ON app.approval_checks, app.organisation_settings_history FROM app_write;

GRANT SELECT ON
    app.approval_check_types,
    app.assessment_outcomes,
    app.purpose_results,
    app.decision_outcomes,
    app.email_statuses,
    app.cancellation_reasons,
    app.booking_status_transitions,
    app.approval_checks,
    app.organisation_settings_history,
    app.v_overdue_loans,
    app.v_assessment_routing_reasons
    TO app_read;

GRANT SELECT ON app.v_overdue_loans, app.v_assessment_routing_reasons TO app_write;

-- Least privilege for the application: it deletes only the notifications of anonymised loans and the
-- feature links of an item, never bookings, decisions, loans or history. Views are read only, and the
-- decision log is written once. Tables created by later migrations get no DELETE by default.
REVOKE DELETE ON ALL TABLES IN SCHEMA app FROM app_write;
GRANT DELETE ON app.notifications, app.equipment_item_features TO app_write;
REVOKE INSERT, UPDATE ON
    app.v_equipment_with_status,
    app.v_overdue_loans,
    app.v_requester_return_history,
    app.v_users_masked,
    app.v_assessment_routing_reasons
    FROM app_write;
REVOKE UPDATE ON app.approval_assessments FROM app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE DELETE ON TABLES FROM app_write;

-- Schema administration covers every object, also those created by this and later migrations.
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA app TO app_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA app TO app_admin;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA app TO app_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT ALL PRIVILEGES ON TABLES TO app_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT ALL PRIVILEGES ON SEQUENCES TO app_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT EXECUTE ON FUNCTIONS TO app_admin;
