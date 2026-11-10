DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
        CREATE SCHEMA app;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS app."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
            CREATE SCHEMA app;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
            CREATE SCHEMA app;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE EXTENSION IF NOT EXISTS btree_gist;
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
            CREATE SCHEMA app;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE EXTENSION IF NOT EXISTS citext;
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
            CREATE SCHEMA app;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE EXTENSION IF NOT EXISTS pg_trgm;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.booking_statuses (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_booking_statuses PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.booking_statuses IS 'Statuses of the booking workflow.';
    COMMENT ON COLUMN app.booking_statuses.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.booking_statuses.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.categories (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(100) NOT NULL,
        description character varying(500),
        max_loan_days integer NOT NULL,
        usage_guidelines character varying(2000) NOT NULL,
        is_specialised boolean NOT NULL DEFAULT FALSE,
        auto_approval_enabled boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT pk_categories PRIMARY KEY (id),
        CONSTRAINT ck_categories_max_loan_days CHECK (max_loan_days > 0)
    );
    COMMENT ON TABLE app.categories IS 'Groups of items with the same loan limit, usage guidelines and approval settings.';
    COMMENT ON COLUMN app.categories.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.categories.name IS 'Unique category name.';
    COMMENT ON COLUMN app.categories.description IS 'Optional description.';
    COMMENT ON COLUMN app.categories.max_loan_days IS 'Longest loan in days that can be approved automatically.';
    COMMENT ON COLUMN app.categories.usage_guidelines IS 'What the items may be used for; the assistant compares the stated purpose with this text.';
    COMMENT ON COLUMN app.categories.is_specialised IS 'Requests for items of a specialised category always go to a manager.';
    COMMENT ON COLUMN app.categories.auto_approval_enabled IS 'Switch for automatic approval of this category.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.decision_sources (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_decision_sources PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.decision_sources IS 'Who took a decision on a request.';
    COMMENT ON COLUMN app.decision_sources.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.decision_sources.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.departments (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_departments PRIMARY KEY (id)
    );
    COMMENT ON TABLE app.departments IS 'Organisational units; a manager decides on requests of the own department.';
    COMMENT ON COLUMN app.departments.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.departments.name IS 'Unique department name.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.features (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(60) NOT NULL,
        CONSTRAINT pk_features PRIMARY KEY (id)
    );
    COMMENT ON TABLE app.features IS 'Searchable properties of items, for example wireless or 4K.';
    COMMENT ON COLUMN app.features.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.features.name IS 'Unique feature name.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.item_conditions (
        code character varying(40) NOT NULL,
        sort_order integer NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_item_conditions PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.item_conditions IS 'Physical condition of an item, ordered from best to worst.';
    COMMENT ON COLUMN app.item_conditions.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.item_conditions.sort_order IS 'Display order, 1 is the best condition.';
    COMMENT ON COLUMN app.item_conditions.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.item_statuses (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_item_statuses PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.item_statuses IS 'Stored statuses of equipment items. Out of service is derived from active maintenance records.';
    COMMENT ON COLUMN app.item_statuses.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.item_statuses.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.locations (
        id bigint GENERATED ALWAYS AS IDENTITY,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_locations PRIMARY KEY (id)
    );
    COMMENT ON TABLE app.locations IS 'Places where items are kept when not on loan.';
    COMMENT ON COLUMN app.locations.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.locations.name IS 'Unique location name, for example a room or a storage.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.maintenance_types (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_maintenance_types PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.maintenance_types IS 'Types of maintenance work.';
    COMMENT ON COLUMN app.maintenance_types.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.maintenance_types.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.notification_types (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_notification_types PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.notification_types IS 'Types of in-app notifications.';
    COMMENT ON COLUMN app.notification_types.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.notification_types.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.roles (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_roles PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.roles IS 'Fixed list of user roles.';
    COMMENT ON COLUMN app.roles.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.roles.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.routing_reasons (
        code character varying(40) NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_routing_reasons PRIMARY KEY (code)
    );
    COMMENT ON TABLE app.routing_reasons IS 'Reasons why a request is routed to the department manager instead of being approved automatically.';
    COMMENT ON COLUMN app.routing_reasons.code IS 'Stable code used by the application and constraints.';
    COMMENT ON COLUMN app.routing_reasons.name IS 'Display name in English.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.equipment_items (
        id bigint GENERATED ALWAYS AS IDENTITY,
        inventory_number character varying(30) NOT NULL,
        name character varying(150) NOT NULL,
        description character varying(2000),
        category_id bigint NOT NULL,
        location_id bigint NOT NULL,
        value numeric(12,2) NOT NULL,
        condition_code character varying(40) NOT NULL,
        status_code character varying(40) NOT NULL,
        is_specialised boolean NOT NULL DEFAULT FALSE,
        retired_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_equipment_items PRIMARY KEY (id),
        CONSTRAINT ck_equipment_items_retired CHECK ((status_code = 'retired') = (retired_at IS NOT NULL)),
        CONSTRAINT ck_equipment_items_value CHECK (value >= 0),
        CONSTRAINT fk_equipment_items_categories_category_id FOREIGN KEY (category_id) REFERENCES app.categories (id) ON DELETE RESTRICT,
        CONSTRAINT fk_equipment_items_item_conditions_condition_code FOREIGN KEY (condition_code) REFERENCES app.item_conditions (code) ON DELETE RESTRICT,
        CONSTRAINT fk_equipment_items_item_statuses_status_code FOREIGN KEY (status_code) REFERENCES app.item_statuses (code) ON DELETE RESTRICT,
        CONSTRAINT fk_equipment_items_locations_location_id FOREIGN KEY (location_id) REFERENCES app.locations (id) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.equipment_items IS 'Physical items of shared equipment.';
    COMMENT ON COLUMN app.equipment_items.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.equipment_items.inventory_number IS 'Unique inventory number printed on the item.';
    COMMENT ON COLUMN app.equipment_items.name IS 'Item name, for example the model.';
    COMMENT ON COLUMN app.equipment_items.description IS 'Optional description used by keyword search.';
    COMMENT ON COLUMN app.equipment_items.category_id IS 'Category of the item.';
    COMMENT ON COLUMN app.equipment_items.location_id IS 'Where the item is kept.';
    COMMENT ON COLUMN app.equipment_items.value IS 'Replacement value in EUR; compared with the low value threshold.';
    COMMENT ON COLUMN app.equipment_items.condition_code IS 'Current physical condition.';
    COMMENT ON COLUMN app.equipment_items.status_code IS 'Stored status: available or retired.';
    COMMENT ON COLUMN app.equipment_items.is_specialised IS 'Item level override: specialised even if the category is not.';
    COMMENT ON COLUMN app.equipment_items.retired_at IS 'When the item was retired; set together with the status retired.';
    COMMENT ON COLUMN app.equipment_items.created_at IS 'Creation time.';
    COMMENT ON COLUMN app.equipment_items.updated_at IS 'Last change time, set by trigger.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.users (
        id bigint GENERATED ALWAYS AS IDENTITY,
        email citext NOT NULL,
        full_name character varying(150) NOT NULL,
        password_hash character varying(200) NOT NULL,
        role_code character varying(40) NOT NULL,
        department_id bigint NOT NULL,
        is_active boolean NOT NULL DEFAULT TRUE,
        failed_sign_in_count integer NOT NULL DEFAULT 0,
        lockout_ends_at timestamp with time zone,
        privacy_notice_acknowledged_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_users PRIMARY KEY (id),
        CONSTRAINT ck_users_email_format CHECK (email ~ '^[^@\s]+@[^@\s]+\.[^@\s]+$'),
        CONSTRAINT ck_users_failed_sign_in_count CHECK (failed_sign_in_count >= 0),
        CONSTRAINT fk_users_departments_department_id FOREIGN KEY (department_id) REFERENCES app.departments (id) ON DELETE RESTRICT,
        CONSTRAINT fk_users_roles_role_code FOREIGN KEY (role_code) REFERENCES app.roles (code) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.users IS 'People who sign in. Personal data: e-mail and full name.';
    COMMENT ON COLUMN app.users.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.users.email IS 'Work e-mail address used to sign in, unique without regard to case.';
    COMMENT ON COLUMN app.users.full_name IS 'Full name shown to managers and administrators.';
    COMMENT ON COLUMN app.users.password_hash IS 'PBKDF2-HMAC-SHA512 hash with salt (ASP.NET Core Identity format v3); empty until a password is set.';
    COMMENT ON COLUMN app.users.role_code IS 'Role of the user.';
    COMMENT ON COLUMN app.users.department_id IS 'Department of the user.';
    COMMENT ON COLUMN app.users.is_active IS 'Inactive users cannot sign in.';
    COMMENT ON COLUMN app.users.failed_sign_in_count IS 'Consecutive failed sign ins since the last success or lockout.';
    COMMENT ON COLUMN app.users.lockout_ends_at IS 'Sign in is refused until this time after too many failed attempts.';
    COMMENT ON COLUMN app.users.privacy_notice_acknowledged_at IS 'When the user first saw the privacy notice (acknowledgement, not consent).';
    COMMENT ON COLUMN app.users.created_at IS 'Creation time.';
    COMMENT ON COLUMN app.users.updated_at IS 'Last change time, set by trigger.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.equipment_item_features (
        equipment_item_id bigint NOT NULL,
        feature_id bigint NOT NULL,
        CONSTRAINT pk_equipment_item_features PRIMARY KEY (equipment_item_id, feature_id),
        CONSTRAINT fk_equipment_item_features_equipment_items_equipment_item_id FOREIGN KEY (equipment_item_id) REFERENCES app.equipment_items (id) ON DELETE CASCADE,
        CONSTRAINT fk_equipment_item_features_features_feature_id FOREIGN KEY (feature_id) REFERENCES app.features (id) ON DELETE CASCADE
    );
    COMMENT ON TABLE app.equipment_item_features IS 'Many-to-many link between items and their features.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.bookings (
        id bigint GENERATED ALWAYS AS IDENTITY,
        equipment_item_id bigint NOT NULL,
        requester_id bigint,
        anonymous_ref character varying(40),
        anonymised_at timestamp with time zone,
        start_date date NOT NULL,
        end_date date NOT NULL,
        purpose character varying(1000) NOT NULL,
        status_code character varying(40) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_bookings PRIMARY KEY (id),
        CONSTRAINT ck_bookings_anonymised CHECK ((requester_id IS NULL) = (anonymised_at IS NOT NULL)),
        CONSTRAINT ck_bookings_anonymous_ref CHECK (anonymised_at IS NULL OR anonymous_ref IS NOT NULL),
        CONSTRAINT ck_bookings_dates CHECK (end_date >= start_date),
        CONSTRAINT ck_bookings_purpose_length CHECK (char_length(purpose) BETWEEN 3 AND 1000),
        CONSTRAINT fk_bookings_booking_statuses_status_code FOREIGN KEY (status_code) REFERENCES app.booking_statuses (code) ON DELETE RESTRICT,
        CONSTRAINT fk_bookings_equipment_items_equipment_item_id FOREIGN KEY (equipment_item_id) REFERENCES app.equipment_items (id) ON DELETE RESTRICT,
        CONSTRAINT fk_bookings_users_requester_id FOREIGN KEY (requester_id) REFERENCES app.users (id) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.bookings IS 'Requests for an item for a date range. A request blocks the item from creation until it is declined, cancelled or returned.';
    COMMENT ON COLUMN app.bookings.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.bookings.equipment_item_id IS 'Requested item.';
    COMMENT ON COLUMN app.bookings.requester_id IS 'Employee who requested the item; null after anonymisation.';
    COMMENT ON COLUMN app.bookings.anonymous_ref IS 'Random reference replacing the requester after the retention period.';
    COMMENT ON COLUMN app.bookings.anonymised_at IS 'When the personal data of the loan was anonymised.';
    COMMENT ON COLUMN app.bookings.start_date IS 'First day of the booking.';
    COMMENT ON COLUMN app.bookings.end_date IS 'Last day of the booking (inclusive).';
    COMMENT ON COLUMN app.bookings.purpose IS 'Purpose stated by the employee; must not contain personal data.';
    COMMENT ON COLUMN app.bookings.status_code IS 'Current status.';
    COMMENT ON COLUMN app.bookings.created_at IS 'Creation time.';
    COMMENT ON COLUMN app.bookings.updated_at IS 'Last change time, set by trigger.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.equipment_condition_changes (
        id bigint GENERATED ALWAYS AS IDENTITY,
        equipment_item_id bigint NOT NULL,
        from_condition_code character varying(40),
        to_condition_code character varying(40) NOT NULL,
        changed_at timestamp with time zone NOT NULL DEFAULT (now()),
        changed_by_user_id bigint,
        CONSTRAINT pk_equipment_condition_changes PRIMARY KEY (id),
        CONSTRAINT fk_equipment_condition_changes_equipment_items_equipment_item_ FOREIGN KEY (equipment_item_id) REFERENCES app.equipment_items (id) ON DELETE CASCADE,
        CONSTRAINT fk_equipment_condition_changes_item_conditions_from_condition_ FOREIGN KEY (from_condition_code) REFERENCES app.item_conditions (code) ON DELETE RESTRICT,
        CONSTRAINT fk_equipment_condition_changes_item_conditions_to_condition_co FOREIGN KEY (to_condition_code) REFERENCES app.item_conditions (code) ON DELETE RESTRICT,
        CONSTRAINT fk_equipment_condition_changes_users_changed_by_user_id FOREIGN KEY (changed_by_user_id) REFERENCES app.users (id) ON DELETE SET NULL
    );
    COMMENT ON TABLE app.equipment_condition_changes IS 'History of condition changes of items, written by trigger.';
    COMMENT ON COLUMN app.equipment_condition_changes.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.equipment_condition_changes.equipment_item_id IS 'Item whose condition changed.';
    COMMENT ON COLUMN app.equipment_condition_changes.from_condition_code IS 'Previous condition; null for a new item.';
    COMMENT ON COLUMN app.equipment_condition_changes.to_condition_code IS 'New condition.';
    COMMENT ON COLUMN app.equipment_condition_changes.changed_at IS 'Change time.';
    COMMENT ON COLUMN app.equipment_condition_changes.changed_by_user_id IS 'User who caused the change, if known.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.maintenance_records (
        id bigint GENERATED ALWAYS AS IDENTITY,
        equipment_item_id bigint NOT NULL,
        maintenance_type_code character varying(40) NOT NULL,
        start_date date NOT NULL,
        end_date date,
        takes_out_of_service boolean NOT NULL,
        description character varying(2000) NOT NULL,
        recorded_by_user_id bigint NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_maintenance_records PRIMARY KEY (id),
        CONSTRAINT ck_maintenance_records_dates CHECK (end_date IS NULL OR end_date >= start_date),
        CONSTRAINT fk_maintenance_records_equipment_items_equipment_item_id FOREIGN KEY (equipment_item_id) REFERENCES app.equipment_items (id) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_records_maintenance_types_maintenance_type_code FOREIGN KEY (maintenance_type_code) REFERENCES app.maintenance_types (code) ON DELETE RESTRICT,
        CONSTRAINT fk_maintenance_records_users_recorded_by_user_id FOREIGN KEY (recorded_by_user_id) REFERENCES app.users (id) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.maintenance_records IS 'Maintenance and repairs of items; an out of service record blocks bookings in its period.';
    COMMENT ON COLUMN app.maintenance_records.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.maintenance_records.equipment_item_id IS 'Maintained item.';
    COMMENT ON COLUMN app.maintenance_records.maintenance_type_code IS 'Type of work.';
    COMMENT ON COLUMN app.maintenance_records.start_date IS 'First day of the maintenance.';
    COMMENT ON COLUMN app.maintenance_records.end_date IS 'Last day of the maintenance; null while open ended.';
    COMMENT ON COLUMN app.maintenance_records.takes_out_of_service IS 'The item cannot be booked during the period.';
    COMMENT ON COLUMN app.maintenance_records.description IS 'What was done or found.';
    COMMENT ON COLUMN app.maintenance_records.recorded_by_user_id IS 'Administrator who recorded the entry.';
    COMMENT ON COLUMN app.maintenance_records.created_at IS 'Creation time.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.organisation_settings (
        id smallint NOT NULL,
        low_value_threshold numeric(12,2) NOT NULL,
        auto_approval_enabled boolean NOT NULL,
        retention_days integer NOT NULL,
        time_zone character varying(64) NOT NULL,
        return_time time without time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_by_user_id bigint,
        CONSTRAINT pk_organisation_settings PRIMARY KEY (id),
        CONSTRAINT ck_organisation_settings_retention CHECK (retention_days >= 30),
        CONSTRAINT ck_organisation_settings_singleton CHECK (id = 1),
        CONSTRAINT ck_organisation_settings_threshold CHECK (low_value_threshold > 0),
        CONSTRAINT fk_organisation_settings_users_updated_by_user_id FOREIGN KEY (updated_by_user_id) REFERENCES app.users (id) ON DELETE SET NULL
    );
    COMMENT ON TABLE app.organisation_settings IS 'Organisation wide settings, exactly one row.';
    COMMENT ON COLUMN app.organisation_settings.id IS 'Always 1.';
    COMMENT ON COLUMN app.organisation_settings.low_value_threshold IS 'Items with a value equal to or above it are high value (EUR).';
    COMMENT ON COLUMN app.organisation_settings.auto_approval_enabled IS 'Organisation wide switch for automatic approval.';
    COMMENT ON COLUMN app.organisation_settings.retention_days IS 'Days after the return after which personal data of a loan is anonymised.';
    COMMENT ON COLUMN app.organisation_settings.time_zone IS 'IANA time zone of the organisation.';
    COMMENT ON COLUMN app.organisation_settings.return_time IS 'Local time on the end date by which items must be returned.';
    COMMENT ON COLUMN app.organisation_settings.updated_at IS 'Last change time, set by trigger.';
    COMMENT ON COLUMN app.organisation_settings.updated_by_user_id IS 'Administrator who changed the settings last.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.refresh_tokens (
        id bigint GENERATED ALWAYS AS IDENTITY,
        user_id bigint NOT NULL,
        token_hash character varying(64) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        revoked_at timestamp with time zone,
        replaced_by_token_id bigint,
        CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
        CONSTRAINT ck_refresh_tokens_expiry CHECK (expires_at > created_at),
        CONSTRAINT fk_refresh_tokens_refresh_tokens_replaced_by_token_id FOREIGN KEY (replaced_by_token_id) REFERENCES app.refresh_tokens (id) ON DELETE SET NULL,
        CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES app.users (id) ON DELETE CASCADE
    );
    COMMENT ON TABLE app.refresh_tokens IS 'Hashes of refresh tokens; every use rotates the token.';
    COMMENT ON COLUMN app.refresh_tokens.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.refresh_tokens.user_id IS 'Owner of the token.';
    COMMENT ON COLUMN app.refresh_tokens.token_hash IS 'SHA-256 hash of the token in hexadecimal; the token itself is never stored.';
    COMMENT ON COLUMN app.refresh_tokens.created_at IS 'Issue time.';
    COMMENT ON COLUMN app.refresh_tokens.expires_at IS 'Expiry time.';
    COMMENT ON COLUMN app.refresh_tokens.revoked_at IS 'Revocation time after rotation, sign out, role change or deactivation.';
    COMMENT ON COLUMN app.refresh_tokens.replaced_by_token_id IS 'Token issued when this one was rotated.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.approval_assessments (
        id bigint GENERATED ALWAYS AS IDENTITY,
        booking_id bigint NOT NULL,
        assessed_at timestamp with time zone NOT NULL,
        value_check_passed boolean NOT NULL,
        specialised_check_passed boolean NOT NULL,
        duration_check_passed boolean NOT NULL,
        organisation_auto_approval_enabled boolean NOT NULL,
        category_auto_approval_enabled boolean NOT NULL,
        purpose_result character varying(20) NOT NULL,
        outcome character varying(20) NOT NULL,
        summary character varying(1000),
        model_name character varying(100),
        prompt_version character varying(20),
        CONSTRAINT pk_approval_assessments PRIMARY KEY (id),
        CONSTRAINT ck_approval_assessments_approved_needs_match CHECK (outcome <> 'approved' OR purpose_result = 'match'),
        CONSTRAINT ck_approval_assessments_outcome CHECK (outcome IN ('pending', 'approved', 'routed')),
        CONSTRAINT ck_approval_assessments_purpose_result CHECK (purpose_result IN ('pending', 'match', 'no_match', 'unavailable', 'not_run')),
        CONSTRAINT fk_approval_assessments_bookings_booking_id FOREIGN KEY (booking_id) REFERENCES app.bookings (id) ON DELETE CASCADE
    );
    COMMENT ON TABLE app.approval_assessments IS 'Decision log of the approval route: result of every policy check, the purpose check and the outcome.';
    COMMENT ON COLUMN app.approval_assessments.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.approval_assessments.booking_id IS 'Assessed booking.';
    COMMENT ON COLUMN app.approval_assessments.assessed_at IS 'Time of the assessment.';
    COMMENT ON COLUMN app.approval_assessments.value_check_passed IS 'The item value is below the low value threshold.';
    COMMENT ON COLUMN app.approval_assessments.specialised_check_passed IS 'Neither the item nor its category is specialised.';
    COMMENT ON COLUMN app.approval_assessments.duration_check_passed IS 'The loan does not exceed the maximum duration of the category.';
    COMMENT ON COLUMN app.approval_assessments.organisation_auto_approval_enabled IS 'Automatic approval was switched on for the organisation.';
    COMMENT ON COLUMN app.approval_assessments.category_auto_approval_enabled IS 'Automatic approval was switched on for the category.';
    COMMENT ON COLUMN app.approval_assessments.purpose_result IS 'Result of the purpose check by the language model.';
    COMMENT ON COLUMN app.approval_assessments.outcome IS 'pending, approved or routed to the manager.';
    COMMENT ON COLUMN app.approval_assessments.summary IS 'Short summary for the manager prepared by the assistant.';
    COMMENT ON COLUMN app.approval_assessments.model_name IS 'Language model used for the purpose check.';
    COMMENT ON COLUMN app.approval_assessments.prompt_version IS 'Version of the prompt template used.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.booking_decisions (
        id bigint GENERATED ALWAYS AS IDENTITY,
        booking_id bigint NOT NULL,
        decision_source_code character varying(40) NOT NULL,
        decided_by_user_id bigint,
        outcome character varying(20) NOT NULL,
        comment character varying(1000),
        decided_at timestamp with time zone NOT NULL,
        superseded_at timestamp with time zone,
        CONSTRAINT pk_booking_decisions PRIMARY KEY (id),
        CONSTRAINT ck_booking_decisions_assistant_never_declines CHECK (decision_source_code <> 'ai_assistant' OR (decided_by_user_id IS NULL AND outcome = 'approved')),
        CONSTRAINT ck_booking_decisions_decline_comment CHECK (outcome <> 'declined' OR comment IS NOT NULL),
        CONSTRAINT ck_booking_decisions_manager_identified CHECK (decision_source_code <> 'manager' OR decided_by_user_id IS NOT NULL),
        CONSTRAINT ck_booking_decisions_outcome CHECK (outcome IN ('approved', 'declined')),
        CONSTRAINT fk_booking_decisions_bookings_booking_id FOREIGN KEY (booking_id) REFERENCES app.bookings (id) ON DELETE CASCADE,
        CONSTRAINT fk_booking_decisions_decision_sources_decision_source_code FOREIGN KEY (decision_source_code) REFERENCES app.decision_sources (code) ON DELETE RESTRICT,
        CONSTRAINT fk_booking_decisions_users_decided_by_user_id FOREIGN KEY (decided_by_user_id) REFERENCES app.users (id) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.booking_decisions IS 'Decisions on requests. A date change supersedes the current decision, so a booking can have several.';
    COMMENT ON COLUMN app.booking_decisions.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.booking_decisions.booking_id IS 'Decided booking.';
    COMMENT ON COLUMN app.booking_decisions.decision_source_code IS 'Manager or AI assistant.';
    COMMENT ON COLUMN app.booking_decisions.decided_by_user_id IS 'Manager who decided; null for the assistant.';
    COMMENT ON COLUMN app.booking_decisions.outcome IS 'approved or declined; the assistant can only approve.';
    COMMENT ON COLUMN app.booking_decisions.comment IS 'Comment of the manager; required for a decline.';
    COMMENT ON COLUMN app.booking_decisions.decided_at IS 'Decision time.';
    COMMENT ON COLUMN app.booking_decisions.superseded_at IS 'Set when a date change sends the booking through the route again.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.booking_status_history (
        id bigint GENERATED ALWAYS AS IDENTITY,
        booking_id bigint NOT NULL,
        from_status_code character varying(40),
        to_status_code character varying(40) NOT NULL,
        changed_at timestamp with time zone NOT NULL DEFAULT (now()),
        changed_by_user_id bigint,
        CONSTRAINT pk_booking_status_history PRIMARY KEY (id),
        CONSTRAINT fk_booking_status_history_booking_statuses_from_status_code FOREIGN KEY (from_status_code) REFERENCES app.booking_statuses (code) ON DELETE RESTRICT,
        CONSTRAINT fk_booking_status_history_booking_statuses_to_status_code FOREIGN KEY (to_status_code) REFERENCES app.booking_statuses (code) ON DELETE RESTRICT,
        CONSTRAINT fk_booking_status_history_bookings_booking_id FOREIGN KEY (booking_id) REFERENCES app.bookings (id) ON DELETE CASCADE,
        CONSTRAINT fk_booking_status_history_users_changed_by_user_id FOREIGN KEY (changed_by_user_id) REFERENCES app.users (id) ON DELETE SET NULL
    );
    COMMENT ON TABLE app.booking_status_history IS 'Every status change of a booking, written by trigger.';
    COMMENT ON COLUMN app.booking_status_history.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.booking_status_history.booking_id IS 'Changed booking.';
    COMMENT ON COLUMN app.booking_status_history.from_status_code IS 'Previous status; null when the booking was created.';
    COMMENT ON COLUMN app.booking_status_history.to_status_code IS 'New status.';
    COMMENT ON COLUMN app.booking_status_history.changed_at IS 'Change time.';
    COMMENT ON COLUMN app.booking_status_history.changed_by_user_id IS 'User who made the change; null for background jobs and after anonymisation.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.loans (
        booking_id bigint NOT NULL,
        issued_at timestamp with time zone NOT NULL,
        issued_by_user_id bigint NOT NULL,
        due_at timestamp with time zone NOT NULL,
        returned_at timestamp with time zone,
        received_by_user_id bigint,
        return_condition_code character varying(40),
        return_comment character varying(1000),
        CONSTRAINT pk_loans PRIMARY KEY (booking_id),
        CONSTRAINT ck_loans_damage_comment CHECK (return_condition_code IS DISTINCT FROM 'damaged' OR return_comment IS NOT NULL),
        CONSTRAINT ck_loans_return_complete CHECK ((returned_at IS NULL) = (received_by_user_id IS NULL) AND (returned_at IS NULL) = (return_condition_code IS NULL)),
        CONSTRAINT ck_loans_returned_after_issue CHECK (returned_at IS NULL OR returned_at >= issued_at),
        CONSTRAINT fk_loans_bookings_booking_id FOREIGN KEY (booking_id) REFERENCES app.bookings (id) ON DELETE CASCADE,
        CONSTRAINT fk_loans_item_conditions_return_condition_code FOREIGN KEY (return_condition_code) REFERENCES app.item_conditions (code) ON DELETE RESTRICT,
        CONSTRAINT fk_loans_users_issued_by_user_id FOREIGN KEY (issued_by_user_id) REFERENCES app.users (id) ON DELETE RESTRICT,
        CONSTRAINT fk_loans_users_received_by_user_id FOREIGN KEY (received_by_user_id) REFERENCES app.users (id) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.loans IS 'Issue and return of the item of a booking; one loan per booking.';
    COMMENT ON COLUMN app.loans.booking_id IS 'Booking of the loan (primary key and foreign key).';
    COMMENT ON COLUMN app.loans.issued_at IS 'When the item was handed out.';
    COMMENT ON COLUMN app.loans.issued_by_user_id IS 'Administrator who issued the item.';
    COMMENT ON COLUMN app.loans.due_at IS 'Due time fixed at issue: end date at the organisation return time.';
    COMMENT ON COLUMN app.loans.returned_at IS 'When the item came back.';
    COMMENT ON COLUMN app.loans.received_by_user_id IS 'Administrator who received the item.';
    COMMENT ON COLUMN app.loans.return_condition_code IS 'Condition recorded at return.';
    COMMENT ON COLUMN app.loans.return_comment IS 'Comment at return; required when the item is damaged.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.notifications (
        id bigint GENERATED ALWAYS AS IDENTITY,
        user_id bigint NOT NULL,
        notification_type_code character varying(40) NOT NULL,
        booking_id bigint,
        title character varying(150) NOT NULL,
        body character varying(1000) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        read_at timestamp with time zone,
        email_status character varying(20) NOT NULL,
        email_attempts integer NOT NULL DEFAULT 0,
        email_sent_at timestamp with time zone,
        CONSTRAINT pk_notifications PRIMARY KEY (id),
        CONSTRAINT ck_notifications_email_attempts CHECK (email_attempts >= 0),
        CONSTRAINT ck_notifications_email_status CHECK (email_status IN ('pending', 'sent', 'failed', 'not_required')),
        CONSTRAINT fk_notifications_bookings_booking_id FOREIGN KEY (booking_id) REFERENCES app.bookings (id) ON DELETE CASCADE,
        CONSTRAINT fk_notifications_notification_types_notification_type_code FOREIGN KEY (notification_type_code) REFERENCES app.notification_types (code) ON DELETE RESTRICT,
        CONSTRAINT fk_notifications_users_user_id FOREIGN KEY (user_id) REFERENCES app.users (id) ON DELETE CASCADE
    );
    COMMENT ON TABLE app.notifications IS 'In-app notifications; the e-mail columns form an outbox for a future e-mail provider.';
    COMMENT ON COLUMN app.notifications.id IS 'Surrogate key.';
    COMMENT ON COLUMN app.notifications.user_id IS 'Recipient.';
    COMMENT ON COLUMN app.notifications.notification_type_code IS 'Type of notification.';
    COMMENT ON COLUMN app.notifications.booking_id IS 'Related booking, if any.';
    COMMENT ON COLUMN app.notifications.title IS 'Short title.';
    COMMENT ON COLUMN app.notifications.body IS 'Message text.';
    COMMENT ON COLUMN app.notifications.created_at IS 'Creation time.';
    COMMENT ON COLUMN app.notifications.read_at IS 'When the recipient marked it as read.';
    COMMENT ON COLUMN app.notifications.email_status IS 'Delivery state of the e-mail copy; not_required while no provider is configured.';
    COMMENT ON COLUMN app.notifications.email_attempts IS 'Number of e-mail delivery attempts.';
    COMMENT ON COLUMN app.notifications.email_sent_at IS 'When the e-mail copy was sent.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE TABLE app.approval_assessment_reasons (
        assessment_id bigint NOT NULL,
        routing_reason_code character varying(40) NOT NULL,
        CONSTRAINT pk_approval_assessment_reasons PRIMARY KEY (assessment_id, routing_reason_code),
        CONSTRAINT fk_approval_assessment_reasons_approval_assessments_assessment FOREIGN KEY (assessment_id) REFERENCES app.approval_assessments (id) ON DELETE CASCADE,
        CONSTRAINT fk_approval_assessment_reasons_routing_reasons_routing_reason_ FOREIGN KEY (routing_reason_code) REFERENCES app.routing_reasons (code) ON DELETE RESTRICT
    );
    COMMENT ON TABLE app.approval_assessment_reasons IS 'Many-to-many link between assessments and every reason why the request was routed.';
    COMMENT ON COLUMN app.approval_assessment_reasons.assessment_id IS 'Assessment.';
    COMMENT ON COLUMN app.approval_assessment_reasons.routing_reason_code IS 'Reason for routing.';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('approved', 'Approved');
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('cancelled', 'Cancelled');
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('declined', 'Declined');
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('issued', 'Issued');
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('overdue', 'Overdue');
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('requested', 'Requested');
    INSERT INTO app.booking_statuses (code, name)
    VALUES ('returned', 'Returned');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.decision_sources (code, name)
    VALUES ('ai_assistant', 'AI assistant');
    INSERT INTO app.decision_sources (code, name)
    VALUES ('manager', 'Department manager');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.item_conditions (code, name, sort_order)
    VALUES ('damaged', 'Damaged', 4);
    INSERT INTO app.item_conditions (code, name, sort_order)
    VALUES ('fair', 'Fair', 3);
    INSERT INTO app.item_conditions (code, name, sort_order)
    VALUES ('good', 'Good', 2);
    INSERT INTO app.item_conditions (code, name, sort_order)
    VALUES ('new', 'New', 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.item_statuses (code, name)
    VALUES ('available', 'Available');
    INSERT INTO app.item_statuses (code, name)
    VALUES ('retired', 'Retired');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.maintenance_types (code, name)
    VALUES ('calibration', 'Calibration');
    INSERT INTO app.maintenance_types (code, name)
    VALUES ('cleaning', 'Cleaning');
    INSERT INTO app.maintenance_types (code, name)
    VALUES ('inspection', 'Inspection');
    INSERT INTO app.maintenance_types (code, name)
    VALUES ('repair', 'Repair');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.notification_types (code, name)
    VALUES ('booking_cancelled_by_maintenance', 'Booking cancelled because of maintenance');
    INSERT INTO app.notification_types (code, name)
    VALUES ('loan_overdue', 'Loan overdue');
    INSERT INTO app.notification_types (code, name)
    VALUES ('request_approved', 'Request approved');
    INSERT INTO app.notification_types (code, name)
    VALUES ('request_declined', 'Request declined');
    INSERT INTO app.notification_types (code, name)
    VALUES ('return_reminder', 'Return reminder');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.organisation_settings (id, auto_approval_enabled, low_value_threshold, retention_days, return_time, time_zone, updated_by_user_id)
    VALUES (1, TRUE, 500.0, 365, TIME '18:00:00', 'Europe/Vilnius', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.roles (code, name)
    VALUES ('administrator', 'Administrator');
    INSERT INTO app.roles (code, name)
    VALUES ('employee', 'Employee');
    INSERT INTO app.roles (code, name)
    VALUES ('manager', 'Department manager');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('assistant_unavailable', 'The AI assistant is not available');
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('category_auto_approval_off', 'Automatic approval is switched off for the category');
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('duration_exceeded', 'The loan exceeds the maximum duration of the category');
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('high_value', 'The item is high value');
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('org_auto_approval_off', 'Automatic approval is switched off for the organisation');
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('purpose_not_confirmed', 'The assistant could not confirm the purpose');
    INSERT INTO app.routing_reasons (code, name)
    VALUES ('specialised', 'The item or its category is specialised');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_approval_assessment_reasons_routing_reason_code ON app.approval_assessment_reasons (routing_reason_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_approval_assessments_assessed_at ON app.approval_assessments (assessed_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_approval_assessments_booking_id ON app.approval_assessments (booking_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_booking_decisions_decided_by_user_id ON app.booking_decisions (decided_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_booking_decisions_decision_source_code ON app.booking_decisions (decision_source_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_booking_decisions_current ON app.booking_decisions (booking_id) WHERE superseded_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_booking_status_history_booking_changed_at ON app.booking_status_history (booking_id, changed_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_booking_status_history_changed_by_user_id ON app.booking_status_history (changed_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_booking_status_history_from_status_code ON app.booking_status_history (from_status_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_booking_status_history_to_status_code ON app.booking_status_history (to_status_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_bookings_item_start_date ON app.bookings (equipment_item_id, start_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_bookings_requester_id ON app.bookings (requester_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_bookings_status_start_date ON app.bookings (status_code, start_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_categories_name ON app.categories (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_departments_name ON app.departments (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_condition_changes_changed_by_user_id ON app.equipment_condition_changes (changed_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_condition_changes_from_condition_code ON app.equipment_condition_changes (from_condition_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_condition_changes_item_changed_at ON app.equipment_condition_changes (equipment_item_id, changed_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_condition_changes_to_condition_code ON app.equipment_condition_changes (to_condition_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_item_features_feature_id ON app.equipment_item_features (feature_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_items_category_id ON app.equipment_items (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_items_condition_code ON app.equipment_items (condition_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_items_location_id ON app.equipment_items (location_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_equipment_items_status_code ON app.equipment_items (status_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_equipment_items_inventory_number ON app.equipment_items (inventory_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_features_name ON app.features (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_loans_issued_by_user_id ON app.loans (issued_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_loans_open_due_at ON app.loans (due_at) WHERE returned_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_loans_received_by_user_id ON app.loans (received_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_loans_return_condition_code ON app.loans (return_condition_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_locations_name ON app.locations (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_maintenance_records_item_start_date ON app.maintenance_records (equipment_item_id, start_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_maintenance_records_maintenance_type_code ON app.maintenance_records (maintenance_type_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_maintenance_records_recorded_by_user_id ON app.maintenance_records (recorded_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_notifications_booking_id ON app.notifications (booking_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_notifications_email_pending ON app.notifications (created_at) WHERE email_status = 'pending';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_notifications_notification_type_code ON app.notifications (notification_type_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_notifications_user_created_at ON app.notifications (user_id, created_at DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_organisation_settings_updated_by_user_id ON app.organisation_settings (updated_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_refresh_tokens_replaced_by_token_id ON app.refresh_tokens (replaced_by_token_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_refresh_tokens_user_id ON app.refresh_tokens (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_refresh_tokens_token_hash ON app.refresh_tokens (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_users_department_id ON app.users (department_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE INDEX ix_users_role_code ON app.users (role_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    CREATE UNIQUE INDEX ux_users_email ON app.users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173654_InitialSchema') THEN
    INSERT INTO app."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261110173654_InitialSchema', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173700_BookingRules') THEN
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

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173700_BookingRules') THEN
    INSERT INTO app."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261110173700_BookingRules', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173742_DatabaseRoles') THEN
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

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110173742_DatabaseRoles') THEN
    INSERT INTO app."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261110173742_DatabaseRoles', '10.0.15');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110214825_BookingExpiry') THEN
    INSERT INTO app.notification_types (code, name)
    VALUES ('booking_not_collected', 'Booking cancelled because the item was not collected');
    INSERT INTO app.notification_types (code, name)
    VALUES ('request_expired', 'Request expired without a decision');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM app."__EFMigrationsHistory" WHERE "migration_id" = '20261110214825_BookingExpiry') THEN
    INSERT INTO app."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261110214825_BookingExpiry', '10.0.15');
    END IF;
END $EF$;
COMMIT;

