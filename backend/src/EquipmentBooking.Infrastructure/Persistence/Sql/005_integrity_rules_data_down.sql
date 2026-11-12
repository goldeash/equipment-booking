-- Restores the stored item status, the boolean check columns and the stored routing reasons from the
-- data of the newer structure before it is removed.
UPDATE app.equipment_items
SET status_code = CASE WHEN retired_at IS NULL THEN 'available' ELSE 'retired' END;

UPDATE app.approval_assessments a
SET organisation_auto_approval_enabled = coalesce((SELECT c.passed FROM app.approval_checks c
                                                   WHERE c.assessment_id = a.id AND c.check_type_code = 'organisation_auto_approval'), true),
    category_auto_approval_enabled = coalesce((SELECT c.passed FROM app.approval_checks c
                                               WHERE c.assessment_id = a.id AND c.check_type_code = 'category_auto_approval'), true),
    value_check_passed = coalesce((SELECT c.passed FROM app.approval_checks c
                                   WHERE c.assessment_id = a.id AND c.check_type_code = 'value_threshold'), true),
    specialised_check_passed = coalesce((SELECT c.passed FROM app.approval_checks c
                                         WHERE c.assessment_id = a.id AND c.check_type_code = 'not_specialised'), true),
    duration_check_passed = coalesce((SELECT c.passed FROM app.approval_checks c
                                      WHERE c.assessment_id = a.id AND c.check_type_code = 'max_duration'), true);

INSERT INTO app.approval_assessment_reasons (assessment_id, routing_reason_code)
SELECT c.assessment_id, t.routing_reason_code
FROM app.approval_checks c
         JOIN app.approval_check_types t ON t.code = c.check_type_code
WHERE NOT c.passed
UNION
SELECT a.id, CASE a.purpose_result WHEN 'no_match' THEN 'purpose_not_confirmed' ELSE 'assistant_unavailable' END
FROM app.approval_assessments a
WHERE a.purpose_result IN ('no_match', 'unavailable');

-- Previous versions of the functions and the view that depend on the stored item status.
CREATE OR REPLACE FUNCTION app.fn_item_is_available(p_item_id bigint, p_start date, p_end date) RETURNS boolean
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

CREATE OR REPLACE FUNCTION app.fn_item_effective_status(p_item_id bigint, p_on date) RETURNS text
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

REVOKE INSERT, UPDATE, DELETE ON app.item_statuses FROM app_write;
GRANT SELECT ON app.item_statuses, app.approval_assessment_reasons, app.v_overdue_loans TO app_read;

-- Privileges as they were before: the application could delete in every application table and update the decision log.
GRANT DELETE ON
    app.approval_assessments,
    app.booking_decisions,
    app.bookings,
    app.categories,
    app.departments,
    app.equipment_items,
    app.features,
    app.loans,
    app.locations,
    app.maintenance_records,
    app.organisation_settings,
    app.refresh_tokens,
    app.users
    TO app_write;
GRANT UPDATE ON app.approval_assessments TO app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT DELETE ON TABLES TO app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE ALL PRIVILEGES ON TABLES FROM app_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE ALL PRIVILEGES ON SEQUENCES FROM app_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE EXECUTE ON FUNCTIONS FROM app_admin;
