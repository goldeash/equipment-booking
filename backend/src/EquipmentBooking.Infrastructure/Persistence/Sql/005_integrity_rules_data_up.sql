-- Results of the policy checks move from one boolean column per check to one row per check. Earlier
-- assessments did not store the compared values, so their values stay empty.
INSERT INTO app.approval_checks (assessment_id, check_type_code, passed)
SELECT a.id, c.check_type_code, c.passed
FROM app.approval_assessments a
         CROSS JOIN LATERAL (VALUES ('organisation_auto_approval', a.organisation_auto_approval_enabled),
                                    ('category_auto_approval', a.category_auto_approval_enabled),
                                    ('value_threshold', a.value_check_passed),
                                    ('not_specialised', a.specialised_check_passed),
                                    ('max_duration', a.duration_check_passed)) AS c(check_type_code, passed);

-- Reason of every cancelled booking, taken from the notification that announced the cancellation.
UPDATE app.bookings b
SET cancellation_reason_code = CASE
                                   WHEN EXISTS (SELECT 1 FROM app.notifications n
                                                WHERE n.booking_id = b.id AND n.notification_type_code = 'booking_cancelled_by_maintenance')
                                       THEN 'maintenance'
                                   WHEN EXISTS (SELECT 1 FROM app.notifications n
                                                WHERE n.booking_id = b.id AND n.notification_type_code = 'request_expired')
                                       THEN 'request_expired'
                                   WHEN EXISTS (SELECT 1 FROM app.notifications n
                                                WHERE n.booking_id = b.id AND n.notification_type_code = 'booking_not_collected')
                                       THEN 'not_collected'
                                   ELSE 'by_requester'
    END
WHERE b.status_code = 'cancelled';

-- The current settings become the first version in the settings history.
INSERT INTO app.organisation_settings_history (changed_at, changed_by_user_id, low_value_threshold, auto_approval_enabled, retention_days, time_zone, return_time)
SELECT updated_at, updated_by_user_id, low_value_threshold, auto_approval_enabled, retention_days, time_zone, return_time
FROM app.organisation_settings;
