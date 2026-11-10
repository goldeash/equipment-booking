# Data dictionary

Database: PostgreSQL 18 (Neon), schema `app`, naming in snake_case. The column tables below are generated from the database catalog and the `COMMENT ON` descriptions written by the migrations (`database/scripts/generate_data_dictionary.sql`), so they always match the deployed structure.

## Conventions

| Item | Rule |
|---|---|
| Primary keys | `bigint GENERATED ALWAYS AS IDENTITY` named `id`; reference tables use a readable text `code`; one-to-one tables (`loans`) reuse the parent key |
| Foreign keys | `<entity>_id` or `<reference>_code`, every foreign key has a B-tree index |
| Money | `numeric(12,2)` in EUR |
| Time | `timestamptz` stored in UTC; calendar days of bookings and maintenance as `date` (inclusive ranges) |
| Text | `varchar(n)` with explicit limits; e-mail addresses as `citext` (unique without regard to case) |
| Audit | `created_at` and `updated_at`; `updated_at` is set by the trigger `fn_set_updated_at` |
| Status values | foreign keys to reference tables or `CHECK` lists, never free text |

## Data quality expectations

| Data | Expectation | Enforced by |
|---|---|---|
| Users | Unique work e-mail with a valid format; every user has exactly one role and one department | `ux_users_email`, `ck_users_email_format`, foreign keys |
| Passwords | Only salted PBKDF2-HMAC-SHA512 hashes, never plain text; empty until set | Application (ASP.NET Core Identity hasher) |
| Items | Unique inventory number; value not negative; retired items have a retirement time | `ux_equipment_items_inventory_number`, `ck_equipment_items_value`, `ck_equipment_items_retired` |
| Categories | Maximum loan duration of at least one day and non-empty usage guidelines | `ck_categories_max_loan_days`, `NOT NULL` |
| Bookings | End date not before start date; purpose of 3 to 1000 characters; no two blocking bookings of an item overlap | `ck_bookings_dates`, `ck_bookings_purpose_length`, `ex_bookings_no_overlap` |
| Maintenance | Out of service periods never overlap blocking bookings | Trigger `fn_check_booking_maintenance` (SQLSTATE `EB001`) |
| Decisions | A decline always has a comment; the AI assistant never declines and is never a person; one current decision per booking | `ck_booking_decisions_*`, `ux_booking_decisions_current` |
| Loans | A return is complete (time, administrator, condition); a damaged return has a comment | `ck_loans_return_complete`, `ck_loans_damage_comment` |
| Anonymisation | An anonymised booking has no requester and has a random reference | `ck_bookings_anonymised`, `ck_bookings_anonymous_ref` |
| Settings | Exactly one row; threshold above zero; retention at least 30 days | `ck_organisation_settings_*` |
| History | Status and condition history are append only for the application | Triggers and revoked `UPDATE`/`DELETE` privileges |

## Reference values

| Table | Codes |
|---|---|
| roles | employee, manager, administrator |
| item_statuses | available, retired (out_of_service is derived from active maintenance) |
| item_conditions | new, good, fair, damaged |
| booking_statuses | requested, approved, declined, cancelled, issued, returned, overdue |
| decision_sources | manager, ai_assistant |
| routing_reasons | high_value, specialised, duration_exceeded, org_auto_approval_off, category_auto_approval_off, purpose_not_confirmed, assistant_unavailable |
| maintenance_types | inspection, repair, calibration, cleaning |
| notification_types | request_approved, request_declined, return_reminder, loan_overdue, booking_cancelled_by_maintenance |

Reference rows are inserted by the initial migration and are read only for the application role.

## Relationships

| Relationship | Type | Description |
|---|---|---|
| departments to users | one-to-many | Every user belongs to one department |
| roles to users | one-to-many | Every user has one role |
| categories to equipment_items | one-to-many | Every item belongs to one category |
| locations to equipment_items | one-to-many | Every item is kept at one location |
| equipment_items to features | many-to-many | Through `equipment_item_features` |
| equipment_items to bookings | one-to-many | An item has many bookings over time |
| users to bookings | one-to-many | A requester has many bookings; null after anonymisation |
| bookings to booking_decisions | one-to-many | A date change supersedes the current decision |
| bookings to loans | one-to-one | A booking becomes a loan when the item is issued |
| bookings to approval_assessments | one-to-many | Every run of the approval route is logged |
| approval_assessments to routing_reasons | many-to-many | Through `approval_assessment_reasons` |
| equipment_items to maintenance_records | one-to-many | Maintenance history of an item |
| users to notifications | one-to-many | In-app notifications |
| refresh_tokens to refresh_tokens | self reference | Rotation chain of refresh tokens |

## Normalisation

The schema is in third normal form: every non-key column depends on the key, the whole key and nothing else. Deliberate exceptions are documented:

- `equipment_items.is_specialised` and `categories.is_specialised` are not duplicates: the item flag overrides a category that is not specialised.
- `loans.due_at` is derived from the end date and the organisation return time but is frozen at issue time, so later changes of the return time do not move existing due dates.
- Availability, the effective item status and the return history of a requester are not stored; they are computed by the functions and views listed in [integrity-and-transactions.md](integrity-and-transactions.md).

## Tables

### approval_assessment_reasons

Many-to-many link between assessments and every reason why the request was routed.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| assessment_id | bigint | NOT NULL | PK, FK approval_assessments.id |  | Assessment. |
| routing_reason_code | varchar(40) | NOT NULL | PK, FK routing_reasons.code |  | Reason for routing. |

### approval_assessments

Decision log of the approval route: result of every policy check, the purpose check and the outcome.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| booking_id | bigint | NOT NULL | FK bookings.id |  | Assessed booking. |
| assessed_at | timestamptz | NOT NULL |  |  | Time of the assessment. |
| value_check_passed | boolean | NOT NULL |  |  | The item value is below the low value threshold. |
| specialised_check_passed | boolean | NOT NULL |  |  | Neither the item nor its category is specialised. |
| duration_check_passed | boolean | NOT NULL |  |  | The loan does not exceed the maximum duration of the category. |
| organisation_auto_approval_enabled | boolean | NOT NULL |  |  | Automatic approval was switched on for the organisation. |
| category_auto_approval_enabled | boolean | NOT NULL |  |  | Automatic approval was switched on for the category. |
| purpose_result | varchar(20) | NOT NULL |  |  | Result of the purpose check by the language model. |
| outcome | varchar(20) | NOT NULL |  |  | pending, approved or routed to the manager. |
| summary | varchar(1000) | NULL |  |  | Short summary for the manager prepared by the assistant. |
| model_name | varchar(100) | NULL |  |  | Language model used for the purpose check. |
| prompt_version | varchar(20) | NULL |  |  | Version of the prompt template used. |

Rules: `ck_approval_assessments_approved_needs_match`: `CHECK ((((outcome)::text <> 'approved'::text) OR ((purpose_result)::text = 'match'::text)))`<br>`ck_approval_assessments_outcome`: `CHECK (((outcome)::text = ANY ((ARRAY['pending'::character varying, 'approved'::character varying, 'routed'::character varying])::text[])))`<br>`ck_approval_assessments_purpose_result`: `CHECK (((purpose_result)::text = ANY ((ARRAY['pending'::character varying, 'match'::character varying, 'no_match'::character varying, 'unavailable'::character varying, 'not_run'::character varying])::text[])))`

### booking_decisions

Decisions on requests. A date change supersedes the current decision, so a booking can have several.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| id | bigint | NOT NULL | PK | identity | Surrogate key. |
| booking_id | bigint | NOT NULL | FK bookings.id |  | Decided booking. |
| decision_source_code | varchar(40) | NOT NULL | FK decision_sources.code |  | Manager or AI assistant. |
| decided_by_user_id | bigint | NULL | FK users.id |  | Manager who decided; null for the assistant. |
| outcome | varchar(20) | NOT NULL |  |  | approved or declined; the assistant can only approve. |
| comment | varchar(1000) | NULL |  |  | Comment of the manager; required for a decline. |
| decided_at | timestamptz | NOT NULL |  |  | Decision time. |
| superseded_at | timestamptz | NULL |  |  | Set when a date change sends the booking through the route again. |

Rules: `ck_booking_decisions_assistant_never_declines`: `CHECK ((((decision_source_code)::text <> 'ai_assistant'::text) OR ((decided_by_user_id IS NULL) AND ((outcome)::text = 'approved'::text))))`<br>`ck_booking_decisions_decline_comment`: `CHECK ((((outcome)::text <> 'declined'::text) OR (comment IS NOT NULL)))`<br>`ck_booking_decisions_manager_identified`: `CHECK ((((decision_source_code)::text <> 'manager'::text) OR (decided_by_user_id IS NOT NULL)))`<br>`ck_booking_decisions_outcome`: `CHECK (((outcome)::text = ANY ((ARRAY['approved'::character varying, 'declined'::character varying])::text[])))`

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

### booking_statuses

Statuses of the booking workflow.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
| name | varchar(100) | NOT NULL |  |  | Display name in English. |

### bookings

Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned.

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

Rules: `ck_bookings_anonymised`: `CHECK (((requester_id IS NULL) = (anonymised_at IS NOT NULL)))`<br>`ck_bookings_anonymous_ref`: `CHECK (((anonymised_at IS NULL) OR (anonymous_ref IS NOT NULL)))`<br>`ck_bookings_dates`: `CHECK ((end_date >= start_date))`<br>`ck_bookings_purpose_length`: `CHECK (((char_length((purpose)::text) >= 3) AND (char_length((purpose)::text) <= 1000)))`<br>`ex_bookings_no_overlap`: `EXCLUDE USING gist (equipment_item_id WITH =, daterange(start_date, end_date, '[]'::text) WITH &&) WHERE (((status_code)::text = ANY ((ARRAY['requested'::character varying, 'approved'::character varying, 'issued'::character varying, 'overdue'::character varying])::text[])))`

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
| status_code | varchar(40) | NOT NULL | FK item_statuses.code |  | Stored status: available or retired. |
| is_specialised | boolean | NOT NULL |  | false | Item level override: specialised even if the category is not. |
| retired_at | timestamptz | NULL |  |  | When the item was retired; set together with the status retired. |
| created_at | timestamptz | NOT NULL |  | now() | Creation time. |
| updated_at | timestamptz | NOT NULL |  | now() | Last change time, set by trigger. |

Rules: `ck_equipment_items_retired`: `CHECK ((((status_code)::text = 'retired'::text) = (retired_at IS NOT NULL)))`<br>`ck_equipment_items_value`: `CHECK ((value >= (0)::numeric))`

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

### item_statuses

Stored statuses of equipment items. Out of service is derived from active maintenance records.

| Column | Type | Null | Keys | Default | Description |
|---|---|---|---|---|---|
| code | varchar(40) | NOT NULL | PK |  | Stable code used by the application and constraints. |
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
| email_status | varchar(20) | NOT NULL |  |  | Delivery state of the e-mail copy; not_required while no provider is configured. |
| email_attempts | integer | NOT NULL |  | 0 | Number of e-mail delivery attempts. |
| email_sent_at | timestamptz | NULL |  |  | When the e-mail copy was sent. |

Rules: `ck_notifications_email_attempts`: `CHECK ((email_attempts >= 0))`<br>`ck_notifications_email_status`: `CHECK (((email_status)::text = ANY ((ARRAY['pending'::character varying, 'sent'::character varying, 'failed'::character varying, 'not_required'::character varying])::text[])))`

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

