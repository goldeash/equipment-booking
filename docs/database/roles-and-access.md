# Roles and access

The application never connects as the owner of the schema or as a superuser.

| Role | Kind | Privileges | Used by |
|---|---|---|---|
| `app_read` | group (NOLOGIN) | `SELECT` on non-personal tables and views, `v_users_masked` instead of `users`, `EXECUTE` on the availability functions | Read-only login `eb_readonly` for reporting and review |
| `app_write` | group (NOLOGIN) | Member of `app_read`; `SELECT, INSERT, UPDATE, DELETE` on application tables, sequence usage, `EXECUTE` on functions; reference tables and the migration history are read only; history tables are append only (only `changed_by_user_id` of the status history may be cleared by anonymisation); no DDL | API login `eb_api` |
| `app_admin` | group (NOLOGIN) | All privileges on the schema | Migrator (the schema owner, `neondb_owner` on Neon) |

- Group roles and privileges are created by the migration `DatabaseRoles` (`Persistence/Sql/003_database_roles_up.sql`). Roles are cluster wide, so they are created only when missing and are never dropped by a down migration.
- `ALTER DEFAULT PRIVILEGES` gives `app_write` access to tables, sequences and functions created by later migrations; `app_read` receives explicit grants per object so that new personal data is never exposed by default.
- `EXECUTE` on functions is revoked from `PUBLIC`.
- Login roles with passwords are created by `database/scripts/create_login_roles.sql`; passwords are passed as psql variables and never stored in the repository:

```bash
psql "<owner connection>" -v api_password=... -v readonly_password=... -f database/scripts/create_login_roles.sql
```

- The integration tests verify that `eb_api` cannot create tables.
- User passwords of the application are stored only as salted PBKDF2-HMAC-SHA512 hashes (ASP.NET Core Identity format v3).
