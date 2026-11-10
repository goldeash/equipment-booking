-- Removes the privileges of this migration; the cluster wide roles themselves are kept.
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE EXECUTE ON FUNCTIONS FROM app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE USAGE, SELECT ON SEQUENCES FROM app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA app REVOKE SELECT, INSERT, UPDATE, DELETE ON TABLES FROM app_write;

REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA app FROM app_read, app_write, app_admin;
REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA app FROM app_read, app_write, app_admin;
REVOKE ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA app FROM app_read, app_write, app_admin;
REVOKE ALL ON SCHEMA app FROM app_read, app_write, app_admin;
