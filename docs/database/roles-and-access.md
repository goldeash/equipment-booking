# Roles and access

The application never connects as the owner of the schema or as a superuser, and each role gets only the privileges its work needs.

| Role | Kind | Privileges | Used by |
|---|---|---|---|
| `app_read` | group (NOLOGIN) | `SELECT` on tables and views without personal data, `v_users_masked` instead of `users`, `EXECUTE` on the availability functions | Read-only login `eb_readonly` for reporting and review |
| `app_write` | group (NOLOGIN) | Member of `app_read`. `SELECT`, `INSERT` and `UPDATE` on application tables; `DELETE` only on `notifications` (anonymisation) and `equipment_item_features` (changing the features of an item); views, reference tables and the migration history are read only; history tables, check results and the decision log are append only, except that anonymisation may clear `changed_by_user_id` of the status history; sequence usage, `EXECUTE` on functions; no DDL | API login `eb_api` |
| `app_admin` | group (NOLOGIN) | All privileges on every table, view, sequence and function of the schema, also on objects created later | Migrator (the owner of the schema) |

## Privilege matrix

S = `SELECT`, I = `INSERT`, U = `UPDATE`, D = `DELETE`. The matrix is generated from the database with `database/scripts/generate_privileges.sql`.

| Object | Kind | app_read | app_write | app_admin |
|---|---|---|---|---|
| v_assessment_routing_reasons | view | S | S | SIUD |
| v_equipment_with_status | view | S | S | SIUD |
| v_overdue_loans | view | S | S | SIUD |
| v_requester_return_history | view | S | S | SIUD |
| v_users_masked | view | S | S | SIUD |
| approval_assessments | table | S | SI | SIUD |
| approval_check_types | table | S | S | SIUD |
| approval_checks | table | S | SI | SIUD |
| assessment_outcomes | table | S | S | SIUD |
| booking_decisions | table | S | SIU | SIUD |
| booking_status_history | table | S | SIU (columns) | SIUD |
| booking_status_transitions | table | S | S | SIUD |
| booking_statuses | table | S | S | SIUD |
| bookings | table | S | SIU | SIUD |
| cancellation_reasons | table | S | S | SIUD |
| categories | table | S | SIU | SIUD |
| decision_outcomes | table | S | S | SIUD |
| decision_sources | table | S | S | SIUD |
| departments | table | S | SIU | SIUD |
| email_statuses | table | S | S | SIUD |
| equipment_condition_changes | table | S | SI | SIUD |
| equipment_item_features | table | S | SIUD | SIUD |
| equipment_items | table | S | SIU | SIUD |
| features | table | S | SIU | SIUD |
| item_conditions | table | S | S | SIUD |
| loans | table | S | SIU | SIUD |
| locations | table | S | SIU | SIUD |
| maintenance_records | table | S | SIU | SIUD |
| maintenance_types | table | S | S | SIUD |
| notification_types | table | S | S | SIUD |
| notifications | table | - | SIUD | SIUD |
| organisation_settings | table | S | SIU | SIUD |
| organisation_settings_history | table | S | SI | SIUD |
| purpose_results | table | S | S | SIUD |
| refresh_tokens | table | - | SIU | SIUD |
| roles | table | S | S | SIUD |
| routing_reasons | table | S | S | SIUD |
| users | table | - | SIU | SIUD |


## Rules

- Group roles and privileges are created by the migration `DatabaseRoles` (`Persistence/Sql/003_database_roles_up.sql`) and tightened by `IntegrityRules` (`Persistence/Sql/005_integrity_rules_up.sql`). Roles are cluster wide, so they are created only when missing and are never dropped by a down migration.
- Every migration that adds objects sets their privileges. In addition, `ALTER DEFAULT PRIVILEGES` gives new tables `SELECT`, `INSERT` and `UPDATE` (no `DELETE`) to `app_write` and all privileges to `app_admin`; `app_read` receives explicit grants per object, so new personal data are never exposed by default.
- `EXECUTE` on functions is revoked from `PUBLIC`.
- Login roles with passwords are created by `database/scripts/create_login_roles.sql`; passwords are passed as psql variables and never stored in the repository:

```bash
psql "<owner connection>" -v api_password=... -v readonly_password=... -f database/scripts/create_login_roles.sql
```

- The integration tests run the API with the `eb_api` login and verify that it cannot create tables.
- User passwords of the application are stored only as salted PBKDF2-HMAC-SHA512 hashes (ASP.NET Core Identity format v3).
