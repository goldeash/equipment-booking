-- Creates or updates the login roles of the application. Run with psql as the migrator after the
-- migrations, passing the passwords as variables so that they never appear in the repository:
--
--   psql "<migrator connection>" -v api_password=... -v readonly_password=... -f database/scripts/create_login_roles.sql
--
-- eb_api       login of the API, member of app_write (no DDL, not the owner, not a superuser)
-- eb_readonly  read-only login, member of app_read (no access to personal data except masked views)

\set ON_ERROR_STOP on

SELECT format('CREATE ROLE eb_api LOGIN PASSWORD %L', :'api_password')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'eb_api')
\gexec

SELECT format('ALTER ROLE eb_api PASSWORD %L', :'api_password')
\gexec

SELECT format('CREATE ROLE eb_readonly LOGIN PASSWORD %L', :'readonly_password')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'eb_readonly')
\gexec

SELECT format('ALTER ROLE eb_readonly PASSWORD %L', :'readonly_password')
\gexec

GRANT app_write TO eb_api;
GRANT app_read TO eb_readonly;

ALTER ROLE eb_api SET search_path = app, public;
ALTER ROLE eb_readonly SET search_path = app, public;

SELECT format('GRANT CONNECT ON DATABASE %I TO eb_api, eb_readonly', current_database())
\gexec
