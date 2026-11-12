DROP TRIGGER IF EXISTS trg_approval_checks_consistency ON app.approval_checks;
DROP TRIGGER IF EXISTS trg_approval_assessments_consistency ON app.approval_assessments;
DROP FUNCTION IF EXISTS app.fn_check_assessment_consistency();
DROP FUNCTION IF EXISTS app.fn_assessment_consistency_error(bigint);

DROP TRIGGER IF EXISTS trg_booking_decisions_consistency ON app.booking_decisions;
DROP TRIGGER IF EXISTS trg_loans_consistency ON app.loans;
DROP TRIGGER IF EXISTS trg_bookings_consistency ON app.bookings;
DROP FUNCTION IF EXISTS app.fn_check_booking_consistency();
DROP FUNCTION IF EXISTS app.fn_booking_consistency_error(bigint);

DROP TRIGGER IF EXISTS trg_organisation_settings_history ON app.organisation_settings;
DROP FUNCTION IF EXISTS app.fn_log_settings_change();

DROP TRIGGER IF EXISTS trg_organisation_settings_actor ON app.organisation_settings;
DROP FUNCTION IF EXISTS app.fn_check_settings_actor();
DROP TRIGGER IF EXISTS trg_maintenance_records_actor ON app.maintenance_records;
DROP FUNCTION IF EXISTS app.fn_check_maintenance_actor();
DROP TRIGGER IF EXISTS trg_loans_actors ON app.loans;
DROP FUNCTION IF EXISTS app.fn_check_loan_actors();
DROP TRIGGER IF EXISTS trg_booking_decisions_decision_maker ON app.booking_decisions;
DROP FUNCTION IF EXISTS app.fn_check_decision_maker();
DROP FUNCTION IF EXISTS app.fn_ensure_user_role(bigint, text, text);

DROP TRIGGER IF EXISTS trg_maintenance_records_bookings_check ON app.maintenance_records;
DROP FUNCTION IF EXISTS app.fn_check_maintenance_bookings();

DROP TRIGGER IF EXISTS trg_bookings_item_overdue ON app.bookings;
DROP FUNCTION IF EXISTS app.fn_check_item_overdue();

DROP TRIGGER IF EXISTS trg_bookings_transition ON app.bookings;
DROP FUNCTION IF EXISTS app.fn_check_booking_transition();

DROP VIEW IF EXISTS app.v_assessment_routing_reasons;
DROP VIEW IF EXISTS app.v_overdue_loans;
