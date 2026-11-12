# Data dictionary

Database: PostgreSQL 17 or later, schema `app`, naming in snake_case. The sections Reference values, Relationships and Tables are generated from the database catalog and the `COMMENT ON` descriptions written by the migrations (`database/scripts/generate_data_dictionary.sql`), so they always match the deployed structure. Database roles and privileges are described in [roles-and-access.md](roles-and-access.md), constraints, triggers and transactions in [integrity-and-transactions.md](integrity-and-transactions.md).

## Conventions

| Item | Rule |
|---|---|
| Primary keys | `bigint GENERATED ALWAYS AS IDENTITY` named `id`; reference tables use a readable text `code`; one-to-one tables (`loans`) reuse the parent key; link and result tables (`equipment_item_features`, `approval_checks`, `booking_status_transitions`) use a composite key |
| Foreign keys | `<entity>_id` or `<reference>_code`; every foreign key has a B-tree index |
| Enumerations | Every status, outcome, result and type is a foreign key to a reference table with a readable code and an English display name |
| Money | `numeric(12,2)` in EUR |
| Time | `timestamptz` stored in UTC; calendar days of bookings and maintenance as `date` (inclusive ranges) |
| Text | `varchar(n)` with explicit limits; e-mail addresses as `citext` (unique without regard to case) |
| Audit | `created_at` and `updated_at`; `updated_at` is set by the trigger `fn_set_updated_at`; status, condition and settings changes are kept in history tables written by triggers |
| Deletion | Business data are never deleted: items are retired, users are deactivated, personal data of loans are anonymised. Foreign keys to bookings, items and users use `RESTRICT` |

## Data quality expectations

| Data | Expectation | Enforced by |
|---|---|---|
| Users | Unique work e-mail with a valid format; every user has exactly one role and one department | `ux_users_email`, `ck_users_email_format`, foreign keys |
| Passwords | Only salted PBKDF2-HMAC-SHA512 hashes, never plain text; empty until set | Application (ASP.NET Core Identity hasher) |
| Items | Unique inventory number; value not negative; retired exactly when a retirement time is set | `ux_equipment_items_inventory_number`, `ck_equipment_items_value`, column `retired_at` |
| Categories | Maximum loan duration of at least one day and non-empty usage guidelines | `ck_categories_max_loan_days`, `NOT NULL` |
| Bookings | End date not before start date; purpose of 3 to 1000 characters; no two blocking bookings of an item overlap; an item with an overdue loan cannot be booked | `ck_bookings_dates`, `ck_bookings_purpose_length`, `ex_bookings_no_overlap`, `fn_check_item_overdue` |
| Booking status | Only allowed transitions; the status matches the loan and the current decision; a cancelled booking has exactly one cancellation reason | `booking_status_transitions` with `fn_check_booking_transition`, `fn_check_booking_consistency`, `ck_bookings_cancellation_reason` |
| Maintenance | Out of service periods never overlap blocking bookings | `fn_check_booking_maintenance`, `fn_check_maintenance_bookings` (SQLSTATE `EB001`) |
| Decisions | A decline always has a comment; the assistant never declines and is never a person; one current decision per booking; a manager decides only requests of the own department and never an own request | `ck_booking_decisions_*`, `ux_booking_decisions_current`, `fn_check_decision_maker` |
| Assessments | One result for every policy check with the compared values; the outcome follows from the checks and the purpose result | Composite key of `approval_checks`, `ck_approval_checks_values`, `fn_check_assessment_consistency` |
| Loans | A return is complete (time, administrator, condition); a damaged return has a comment; issue and return only by administrators | `ck_loans_return_complete`, `ck_loans_damage_comment`, `fn_check_loan_actors` |
| Anonymisation | Only returned loans are anonymised; an anonymised booking has no requester and has a random reference | `ck_bookings_anonymised_returned`, `ck_bookings_anonymised`, `ck_bookings_anonymous_ref` |
| Settings | Exactly one row; threshold above zero; retention at least 30 days; changed only by administrators; every version kept | `ck_organisation_settings_*`, `fn_check_settings_actor`, `organisation_settings_history` |
| History | Status, condition and settings history and check results are append only for the application | Triggers and revoked `UPDATE` and `DELETE` privileges ([roles-and-access.md](roles-and-access.md)) |

## Normalisation

The schema is in third normal form: every non-key column depends on the key, the whole key and nothing else. Each fact is stored once; derived facts are computed by views and functions. The following columns may look redundant but store separate facts, each for a stated reason:

| Columns | Why it is not a duplicate |
|---|---|
| `equipment_items.is_specialised` and `categories.is_specialised` | The item flag overrides a category that is not specialised; the effective value is computed by the approval check |
| `equipment_items.condition_code` and `equipment_condition_changes` | The column is the current condition, needed by every catalogue query; the table is the history of changes written by a trigger from that column, never edited separately |
| `bookings.status_code` and `booking_status_history` | The column is the current status that the exclusion constraint and all queries use; the history is written by a trigger from that column. The consistency of the status with the loan and the decision is checked by the database |
| `bookings.status_code = 'overdue'` | Overdue is stored and not only computed from the due time, because it is an event: at that moment the employee and the administrators are notified, the history records the time, and KPI-2 (overdue loans flagged within one hour) is measured on it. The job runs five minutes after the organisation return time, and `v_overdue_loans` computes overdue loans from the due time, so reports are correct even before the job has run |
| `loans.due_at` | Derived from the end date and the organisation return time, but frozen at issue time, so a later change of the return time does not move existing due dates |
| `approval_checks.observed_value` and `limit_value` | Copies of the item value, the loan length and the limits taken when the request was assessed. They record the facts the decision was based on; the current item value or threshold may differ later. The history of the settings themselves is in `organisation_settings_history` |
| `bookings.cancellation_reason_code` | Set only for cancelled bookings (checked by `ck_bookings_cancellation_reason`); it records why, which the status alone cannot tell |

Not stored at all: availability, the effective item status (retired from `retired_at`, out of service from active maintenance), the routing reasons of an assessment (`v_assessment_routing_reasons`) and the return history of a requester (`v_requester_return_history`).

## Design decisions

| Topic | Decision and reason |
|---|---|
| Department manager | A department manager is a user with the role `manager` in that department; there is no separate column for the manager of a department. A department may have several managers so that a deputy can decide during an absence; every manager of the department sees its routed requests. A manager never decides an own request, which only other managers of the department may decide. Requests that nobody can decide are not rerouted: like every request not decided by its start date, they are cancelled by the expiry job with the reason `request_expired`, and the requester is notified |
| Status transitions in the database | The allowed transitions are data in `booking_status_transitions`, not code, and the database enforces them for every client. The domain model uses the same list for clear error messages; an integration test keeps both in step |
| Cancellation reasons instead of more statuses | The booking statuses stay those of the specification; the reason of a cancellation (by the requester, maintenance, no decision in time, not collected) is a separate reference value |

## Reference values

Reference rows are inserted by the migrations and are read only for the application role.

| Table | Codes |
|---|---|
| approval_check_types | category_auto_approval, max_duration, not_specialised, organisation_auto_approval, value_threshold |
| assessment_outcomes | approved, pending, routed |
| booking_statuses | approved, cancelled, declined, issued, overdue, requested, returned |
| cancellation_reasons | by_requester, maintenance, not_collected, request_expired |
| decision_outcomes | approved, declined |
| decision_sources | ai_assistant, manager |
| email_statuses | failed, not_required, pending, sent |
| item_conditions | damaged, fair, good, new |
| maintenance_types | calibration, cleaning, inspection, repair |
| notification_types | booking_cancelled_by_maintenance, booking_not_collected, loan_overdue, request_approved, request_declined, request_expired, return_reminder |
| purpose_results | match, no_match, not_run, pending, unavailable |
| roles | administrator, employee, manager |
| routing_reasons | assistant_unavailable, category_auto_approval_off, duration_exceeded, high_value, org_auto_approval_off, purpose_not_confirmed, specialised |
| booking_status_transitions | approved to cancelled, approved to issued, approved to requested, issued to overdue, issued to returned, overdue to returned, requested to approved, requested to cancelled, requested to declined |

## Relationships

Every foreign key of the schema. One-to-one means that the referencing columns are unique; optional means that they may be null. Restrict prevents deleting a referenced row; cascade deletes the referencing rows with it; set null keeps them without the reference.

| Table | Columns | References | Cardinality | On delete |
|---|---|---|---|---|
| approval_assessments | booking_id | bookings (id) | many-to-one, required | restrict |
| approval_assessments | outcome | assessment_outcomes (code) | many-to-one, required | restrict |
| approval_assessments | purpose_result | purpose_results (code) | many-to-one, required | restrict |
| approval_check_types | routing_reason_code | routing_reasons (code) | one-to-one, required | restrict |
| approval_checks | assessment_id | approval_assessments (id) | many-to-one, required | cascade |
| approval_checks | check_type_code | approval_check_types (code) | many-to-one, required | restrict |
| booking_decisions | booking_id | bookings (id) | many-to-one, required | restrict |
| booking_decisions | decided_by_user_id | users (id) | many-to-one, optional | restrict |
| booking_decisions | decision_source_code | decision_sources (code) | many-to-one, required | restrict |
| booking_decisions | outcome | decision_outcomes (code) | many-to-one, required | restrict |
| booking_status_history | booking_id | bookings (id) | many-to-one, required | restrict |
| booking_status_history | changed_by_user_id | users (id) | many-to-one, optional | set null |
| booking_status_history | from_status_code | booking_statuses (code) | many-to-one, optional | restrict |
| booking_status_history | to_status_code | booking_statuses (code) | many-to-one, required | restrict |
| booking_status_transitions | from_status_code | booking_statuses (code) | many-to-one, required | restrict |
| booking_status_transitions | to_status_code | booking_statuses (code) | many-to-one, required | restrict |
| bookings | cancellation_reason_code | cancellation_reasons (code) | many-to-one, optional | restrict |
| bookings | equipment_item_id | equipment_items (id) | many-to-one, required | restrict |
| bookings | requester_id | users (id) | many-to-one, optional | restrict |
| bookings | status_code | booking_statuses (code) | many-to-one, required | restrict |
| equipment_condition_changes | changed_by_user_id | users (id) | many-to-one, optional | set null |
| equipment_condition_changes | equipment_item_id | equipment_items (id) | many-to-one, required | restrict |
| equipment_condition_changes | from_condition_code | item_conditions (code) | many-to-one, optional | restrict |
| equipment_condition_changes | to_condition_code | item_conditions (code) | many-to-one, required | restrict |
| equipment_item_features | equipment_item_id | equipment_items (id) | many-to-one, required | cascade |
| equipment_item_features | feature_id | features (id) | many-to-one, required | cascade |
| equipment_items | category_id | categories (id) | many-to-one, required | restrict |
| equipment_items | condition_code | item_conditions (code) | many-to-one, required | restrict |
| equipment_items | location_id | locations (id) | many-to-one, required | restrict |
| loans | booking_id | bookings (id) | one-to-one, required | restrict |
| loans | issued_by_user_id | users (id) | many-to-one, required | restrict |
| loans | received_by_user_id | users (id) | many-to-one, optional | restrict |
| loans | return_condition_code | item_conditions (code) | many-to-one, optional | restrict |
| maintenance_records | equipment_item_id | equipment_items (id) | many-to-one, required | restrict |
| maintenance_records | maintenance_type_code | maintenance_types (code) | many-to-one, required | restrict |
| maintenance_records | recorded_by_user_id | users (id) | many-to-one, required | restrict |
| notifications | booking_id | bookings (id) | many-to-one, optional | cascade |
| notifications | email_status | email_statuses (code) | many-to-one, required | restrict |
| notifications | notification_type_code | notification_types (code) | many-to-one, required | restrict |
| notifications | user_id | users (id) | many-to-one, required | cascade |
| organisation_settings | updated_by_user_id | users (id) | many-to-one, optional | set null |
| organisation_settings_history | changed_by_user_id | users (id) | many-to-one, optional | set null |
| refresh_tokens | replaced_by_token_id | refresh_tokens (id) | many-to-one, optional | set null |
| refresh_tokens | user_id | users (id) | many-to-one, required | cascade |
| users | department_id | departments (id) | many-to-one, required | restrict |
| users | role_code | roles (code) | many-to-one, required | restrict |

## Tables

### approval_assessments

Decision log of the approval route: the purpose check and the outcome; the results of the policy checks are in approval_checks.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| booking_id | bigint | NOT NULL | FK bookings.id |  | Assessed booking. |
| assessed_at | timestamptz | NOT NULL |  |  | Time of the assessment. |
| purpose_result | varchar(40) | NOT NULL | FK purpose_results.code |  | Result of the purpose check by the language model. |
| outcome | varchar(40) | NOT NULL | FK assessment_outcomes.code |  | Pending, approved automatically or routed to the manager. |
| summary | varchar(1000) | NULL |  |  | Short summary for the manager prepared by the assistant. |
| model_name | varchar(100) | NULL |  |  | Language model used for the purpose check. |
| prompt_version | varchar(20) | NULL |  |  | Version of the prompt template used. |

### approval_check_types

Fixed policy checks of the approval route and the routing reason recorded when a check fails.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| routing_reason_code | varchar(40) | NOT NULL | FK routing_reasons.code, UQ |  | Routing reason when the check fails. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### approval_checks

Result of every policy check of an assessment with the values it compared.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| assessment_id | bigint | NOT NULL | PK, FK approval_assessments.id |  | Assessment the check belongs to. |
| check_type_code | varchar(40) | NOT NULL | PK, FK approval_check_types.code |  | Policy check. |
| passed | boolean | NOT NULL |  |  | The check passed. |
| observed_value | numeric(12,2) | NULL |  |  | Value of the request that was compared, for example the item value in EUR or the loan length in days; null for switches and flags. |
| limit_value | numeric(12,2) | NULL |  |  | Limit in force at the time of the assessment, for example the low value threshold or the maximum loan days; null for switches and flags. |

Rules: `ck_approval_checks_values`: `CHECK (((observed_value IS NULL) = (limit_value IS NULL)))`

### assessment_outcomes

Outcomes of the approval route for a request.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### booking_decisions

Decisions on requests. A date change supersedes the current decision, so a booking can have several.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| booking_id | bigint | NOT NULL | FK bookings.id |  | Decided booking. |
| decision_source_code | varchar(40) | NOT NULL | FK decision_sources.code |  | Manager or AI assistant. |
| decided_by_user_id | bigint | NULL | FK users.id |  | Manager who decided; null for the assistant. |
| outcome | varchar(40) | NOT NULL | FK decision_outcomes.code |  | Approved or declined; the assistant can only approve. |
| comment | varchar(1000) | NULL |  |  | Comment of the manager; required for a decline. |
| decided_at | timestamptz | NOT NULL |  |  | Decision time. |
| superseded_at | timestamptz | NULL |  |  | Set when a date change sends the booking through the route again. |

Rules: `ck_booking_decisions_assistant_never_declines`: `CHECK ((((decision_source_code)::text <> 'ai_assistant'::text) OR ((decided_by_user_id IS NULL) AND ((outcome)::text = 'approved'::text))))`<br>`ck_booking_decisions_decline_comment`: `CHECK ((((outcome)::text <> 'declined'::text) OR (comment IS NOT NULL)))`<br>`ck_booking_decisions_manager_identified`: `CHECK ((((decision_source_code)::text <> 'manager'::text) OR (decided_by_user_id IS NOT NULL)))`

### booking_status_history

Every status change of a booking, written by trigger.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| booking_id | bigint | NOT NULL | FK bookings.id |  | Changed booking. |
| from_status_code | varchar(40) | NULL | FK booking_statuses.code |  | Previous status; null when the booking was created. |
| to_status_code | varchar(40) | NOT NULL | FK booking_statuses.code |  | New status. |
| changed_at | timestamptz | NOT NULL |  | now() | Change time. |
| changed_by_user_id | bigint | NULL | FK users.id |  | User who made the change; null for background jobs and after anonymisation. |

### booking_status_transitions

Allowed status changes of a booking, enforced by the trigger fn_check_booking_transition.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| from_status_code | varchar(40) | NOT NULL | PK, FK booking_statuses.code |  | Current status. |
| to_status_code | varchar(40) | NOT NULL | PK, FK booking_statuses.code |  | Status the booking may change to. |

### booking_statuses

Statuses of the booking workflow.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### bookings

Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned; an overdue loan blocks the item until it is returned.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| equipment_item_id | bigint | NOT NULL | FK equipment_items.id |  | Requested item. |
| requester_id | bigint | NULL | FK users.id |  | Employee who requested the item; null after anonymisation. |
| anonymous_ref | varchar(40) | NULL |  |  | Random reference replacing the requester after the retention period. |
| anonymised_at | timestamptz | NULL |  |  | When the personal data of the loan was anonymised. |
| start_date | date | NOT NULL |  |  | First day of the booking. |
| end_date | date | NOT NULL |  |  | Last day of the booking (inclusive). |
| purpose | varchar(1000) | NOT NULL |  |  | Purpose stated by the employee; must not contain personal data. |
| status_code | varchar(40) | NOT NULL | FK booking_statuses.code |  | Current status. |
| created_at | timestamptz | NOT NULL |  | now() | Creation time. |
| updated_at | timestamptz | NOT NULL |  | now() | Last change time, set by trigger. |
| cancellation_reason_code | varchar(40) | NULL | FK cancellation_reasons.code |  | Why the booking was cancelled; set exactly when the status is cancelled. |

Rules: `ck_bookings_anonymised`: `CHECK (((requester_id IS NULL) = (anonymised_at IS NOT NULL)))`<br>`ck_bookings_anonymised_returned`: `CHECK (((anonymised_at IS NULL) OR ((status_code)::text = 'returned'::text)))`<br>`ck_bookings_anonymous_ref`: `CHECK (((anonymised_at IS NULL) OR (anonymous_ref IS NOT NULL)))`<br>`ck_bookings_cancellation_reason`: `CHECK ((((status_code)::text = 'cancelled'::text) = (cancellation_reason_code IS NOT NULL)))`<br>`ck_bookings_dates`: `CHECK ((end_date >= start_date))`<br>`ck_bookings_purpose_length`: `CHECK (((char_length((purpose)::text) >= 3) AND (char_length((purpose)::text) <= 1000)))`<br>`ex_bookings_no_overlap`: `EXCLUDE USING gist (equipment_item_id WITH =, daterange(start_date, end_date, '[]'::text) WITH &&) WHERE (((status_code)::text = ANY ((ARRAY['requested'::character varying, 'approved'::character varying, 'issued'::character varying, 'overdue'::character varying])::text[])))`

### cancellation_reasons

Reasons why a booking was cancelled; every cancelled booking has one.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### categories

Groups of items with the same loan limit, usage guidelines and approval settings.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| name | varchar(100) | NOT NULL | UQ |  | Unique category name. |
| description | varchar(500) | NULL |  |  | Optional description. |
| max_loan_days | integer | NOT NULL |  |  | Longest loan in days that can be approved automatically. |
| usage_guidelines | varchar(2000) | NOT NULL |  |  | What the items may be used for; the assistant compares the stated purpose with this text. |
| is_specialised | boolean | NOT NULL |  | false | Requests for items of a specialised category always go to a manager. |
| auto_approval_enabled | boolean | NOT NULL |  | true | Switch for automatic approval of this category. |

Rules: `ck_categories_max_loan_days`: `CHECK ((max_loan_days > 0))`

### decision_outcomes

Outcomes of a decision on a request.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### decision_sources

Who took a decision on a request.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### departments

Organisational units; a manager decides on requests of the own department.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| name | varchar(100) | NOT NULL | UQ |  | Unique department name. |

### email_statuses

Delivery states of the e-mail copy of a notification.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### equipment_condition_changes

History of condition changes of items, written by trigger.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| equipment_item_id | bigint | NOT NULL | FK equipment_items.id |  | Item whose condition changed. |
| from_condition_code | varchar(40) | NULL | FK item_conditions.code |  | Previous condition; null for a new item. |
| to_condition_code | varchar(40) | NOT NULL | FK item_conditions.code |  | New condition. |
| changed_at | timestamptz | NOT NULL |  | now() | Change time. |
| changed_by_user_id | bigint | NULL | FK users.id |  | User who caused the change, if known. |

### equipment_item_features

Many-to-many link between items and their features.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| equipment_item_id | bigint | NOT NULL | PK, FK equipment_items.id |  |  |
| feature_id | bigint | NOT NULL | PK, FK features.id |  |  |

### equipment_items

Physical items of shared equipment.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| inventory_number | varchar(30) | NOT NULL | UQ |  | Unique inventory number printed on the item. |
| name | varchar(150) | NOT NULL |  |  | Item name, for example the model. |
| description | varchar(2000) | NULL |  |  | Optional description used by keyword search. |
| category_id | bigint | NOT NULL | FK categories.id |  | Category of the item. |
| location_id | bigint | NOT NULL | FK locations.id |  | Where the item is kept. |
| value | numeric(12,2) | NOT NULL |  |  | Replacement value in EUR; compared with the low value threshold. |
| condition_code | varchar(40) | NOT NULL | FK item_conditions.code |  | Current physical condition. |
| is_specialised | boolean | NOT NULL |  | false | Item level override: specialised even if the category is not. |
| retired_at | timestamptz | NULL |  |  | When the item was retired; null while the item is in use. The effective status is derived from this column and active maintenance. |
| created_at | timestamptz | NOT NULL |  | now() | Creation time. |
| updated_at | timestamptz | NOT NULL |  | now() | Last change time, set by trigger. |

Rules: `ck_equipment_items_value`: `CHECK ((value >= (0)::numeric))`

### features

Searchable properties of items, for example wireless or 4K.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| name | varchar(60) | NOT NULL | UQ |  | Unique feature name. |

### item_conditions

Physical condition of an item, ordered from best to worst.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| sort_order | integer | NOT NULL |  |  | Display order, 1 is the best condition. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### loans

Issue and return of the item of a booking; one loan per booking.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| booking_id | bigint | NOT NULL | PK, FK bookings.id |  | Booking of the loan (primary key and foreign key). |
| issued_at | timestamptz | NOT NULL |  |  | When the item was handed out. |
| issued_by_user_id | bigint | NOT NULL | FK users.id |  | Administrator who issued the item. |
| due_at | timestamptz | NOT NULL |  |  | Due time fixed at issue: end date at the organisation return time. |
| returned_at | timestamptz | NULL |  |  | When the item came back. |
| received_by_user_id | bigint | NULL | FK users.id |  | Administrator who received the item. |
| return_condition_code | varchar(40) | NULL | FK item_conditions.code |  | Condition recorded at return. |
| return_comment | varchar(1000) | NULL |  |  | Comment at return; required when the item is damaged. |

Rules: `ck_loans_damage_comment`: `CHECK ((((return_condition_code)::text IS DISTINCT FROM 'damaged'::text) OR (return_comment IS NOT NULL)))`<br>`ck_loans_return_complete`: `CHECK ((((returned_at IS NULL) = (received_by_user_id IS NULL)) AND ((returned_at IS NULL) = (return_condition_code IS NULL))))`<br>`ck_loans_returned_after_issue`: `CHECK (((returned_at IS NULL) OR (returned_at >= issued_at)))`

### locations

Places where items are kept when not on loan.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| name | varchar(100) | NOT NULL | UQ |  | Unique location name, for example a room or a storage. |

### maintenance_records

Maintenance and repairs of items; an out of service record blocks bookings in its period.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| equipment_item_id | bigint | NOT NULL | FK equipment_items.id |  | Maintained item. |
| maintenance_type_code | varchar(40) | NOT NULL | FK maintenance_types.code |  | Type of work. |
| start_date | date | NOT NULL |  |  | First day of the maintenance. |
| end_date | date | NULL |  |  | Last day of the maintenance; null while open ended. |
| takes_out_of_service | boolean | NOT NULL |  |  | The item cannot be booked during the period. |
| description | varchar(2000) | NOT NULL |  |  | What was done or found. |
| recorded_by_user_id | bigint | NOT NULL | FK users.id |  | Administrator who recorded the entry. |
| created_at | timestamptz | NOT NULL |  | now() | Creation time. |

Rules: `ck_maintenance_records_dates`: `CHECK (((end_date IS NULL) OR (end_date >= start_date)))`

### maintenance_types

Types of maintenance work.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### notification_types

Types of in-app notifications.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### notifications

In-app notifications; the e-mail columns form an outbox for a future e-mail provider.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| user_id | bigint | NOT NULL | FK users.id |  | Recipient. |
| notification_type_code | varchar(40) | NOT NULL | FK notification_types.code |  | Type of notification. |
| booking_id | bigint | NULL | FK bookings.id |  | Related booking, if any. |
| title | varchar(150) | NOT NULL |  |  | Short title. |
| body | varchar(1000) | NOT NULL |  |  | Message text. |
| created_at | timestamptz | NOT NULL |  |  | Creation time. |
| read_at | timestamptz | NULL |  |  | When the recipient marked it as read. |
| email_status | varchar(40) | NOT NULL | FK email_statuses.code |  | Delivery state of the e-mail copy; not_required while no provider is configured. |
| email_attempts | integer | NOT NULL |  | 0 | Number of e-mail delivery attempts. |
| email_sent_at | timestamptz | NULL |  |  | When the e-mail copy was sent. |

Rules: `ck_notifications_email_attempts`: `CHECK ((email_attempts >= 0))`

### organisation_settings

Organisation wide settings, exactly one row.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | smallint | NOT NULL | PK |  | Always 1. |
| low_value_threshold | numeric(12,2) | NOT NULL |  |  | Items with a value equal to or above it are high value (EUR). |
| auto_approval_enabled | boolean | NOT NULL |  |  | Organisation wide switch for automatic approval. |
| retention_days | integer | NOT NULL |  |  | Days after the return after which personal data of a loan is anonymised. |
| time_zone | varchar(64) | NOT NULL |  |  | IANA time zone of the organisation. |
| return_time | time | NOT NULL |  |  | Local time on the end date by which items must be returned. |
| updated_at | timestamptz | NOT NULL |  | now() | Last change time, set by trigger. |
| updated_by_user_id | bigint | NULL | FK users.id |  | Administrator who changed the settings last. |

Rules: `ck_organisation_settings_retention`: `CHECK ((retention_days >= 30))`<br>`ck_organisation_settings_singleton`: `CHECK ((id = 1))`<br>`ck_organisation_settings_threshold`: `CHECK ((low_value_threshold > (0)::numeric))`

### organisation_settings_history

Every version of the organisation settings, written by trigger.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| changed_at | timestamptz | NOT NULL |  | now() | When this version came into force. |
| changed_by_user_id | bigint | NULL | FK users.id |  | Administrator who made the change; null for the initial settings and after a user was removed. |
| low_value_threshold | numeric(12,2) | NOT NULL |  |  | Low value threshold in EUR. |
| auto_approval_enabled | boolean | NOT NULL |  |  | Organisation wide switch for automatic approval. |
| retention_days | integer | NOT NULL |  |  | Retention period of personal data of loans in days. |
| time_zone | varchar(64) | NOT NULL |  |  | IANA time zone of the organisation. |
| return_time | time | NOT NULL |  |  | Local return time on the end date. |

### purpose_results

Results of the purpose check by the language model.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### refresh_tokens

Hashes of refresh tokens; every use rotates the token.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| user_id | bigint | NOT NULL | FK users.id |  | Owner of the token. |
| token_hash | varchar(64) | NOT NULL | UQ |  | SHA-256 hash of the token in hexadecimal; the token itself is never stored. |
| created_at | timestamptz | NOT NULL |  |  | Issue time. |
| expires_at | timestamptz | NOT NULL |  |  | Expiry time. |
| revoked_at | timestamptz | NULL |  |  | Revocation time after rotation, sign out, role change or deactivation. |
| replaced_by_token_id | bigint | NULL | FK refresh_tokens.id |  | Token issued when this one was rotated. |

Rules: `ck_refresh_tokens_expiry`: `CHECK ((expires_at > created_at))`

### roles

Fixed list of user roles.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### routing_reasons

Reasons why a request is routed to the department manager instead of being approved automatically.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### users

People who sign in. Personal data: e-mail and full name.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| email | citext | NOT NULL | UQ |  | Work e-mail address used to sign in, unique without regard to case. |
| full_name | varchar(150) | NOT NULL |  |  | Full name shown to managers and administrators. |
| password_hash | varchar(200) | NOT NULL |  |  | PBKDF2-HMAC-SHA512 hash with salt (ASP.NET Core Identity format v3); empty until a password is set. |
| role_code | varchar(40) | NOT NULL | FK roles.code |  | Role of the user. |
| department_id | bigint | NOT NULL | FK departments.id |  | Department of the user. |
| is_active | boolean | NOT NULL |  | true | Inactive users cannot sign in. |
| failed_sign_in_count | integer | NOT NULL |  | 0 | Consecutive failed sign ins since the last success or lockout. |
| lockout_ends_at | timestamptz | NULL |  |  | Sign in is refused until this time after too many failed attempts. |
| privacy_notice_acknowledged_at | timestamptz | NULL |  |  | When the user first saw the privacy notice (acknowledgement, not consent). |
| created_at | timestamptz | NOT NULL |  | now() | Creation time. |
| updated_at | timestamptz | NOT NULL |  | now() | Last change time, set by trigger. |

Rules: `ck_users_email_format`: `CHECK ((email ~ '^[^@\s]+@[^@\s]+\.[^@\s]+$'::citext))`<br>`ck_users_failed_sign_in_count`: `CHECK ((failed_sign_in_count >= 0))`

