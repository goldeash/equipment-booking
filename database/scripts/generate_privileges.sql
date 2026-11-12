-- Prints the privilege matrix of docs/database/roles-and-access.md from the database catalog. Run with:
--   psql "<connection>" -X -A -t -f database/scripts/generate_privileges.sql > privileges.md
-- S = SELECT, I = INSERT, U = UPDATE, D = DELETE; a dash means no access.

SELECT E'| Object | Kind | app_read | app_write | app_admin |\n|---|---|---|---|---|\n'
           || string_agg('| ' || o.name || ' | ' || o.kind || ' | ' || o.app_read || ' | ' || o.app_write || ' | ' || o.app_admin || ' |',
                         E'\n' ORDER BY o.kind DESC, o.name)
           || E'\n'
FROM (
    SELECT c.relname AS name,
           CASE c.relkind WHEN 'v' THEN 'view' ELSE 'table' END AS kind,
           privileges.app_read,
           privileges.app_write,
           privileges.app_admin
    FROM pg_class c
             JOIN pg_namespace n ON n.oid = c.relnamespace
             CROSS JOIN LATERAL (
        SELECT max(CASE WHEN r.role = 'app_read' THEN r.letters END) AS app_read,
               max(CASE WHEN r.role = 'app_write' THEN r.letters END) AS app_write,
               max(CASE WHEN r.role = 'app_admin' THEN r.letters END) AS app_admin
        FROM (
            SELECT role,
                   coalesce(nullif(concat(
                       CASE WHEN has_table_privilege(role, c.oid, 'SELECT') THEN 'S' END,
                       CASE WHEN has_table_privilege(role, c.oid, 'INSERT') THEN 'I' END,
                       CASE WHEN has_table_privilege(role, c.oid, 'UPDATE') THEN 'U'
                            WHEN has_any_column_privilege(role, c.oid, 'UPDATE') THEN 'U (columns)' END,
                       CASE WHEN has_table_privilege(role, c.oid, 'DELETE') THEN 'D' END), ''), '-') AS letters
            FROM unnest(ARRAY ['app_read', 'app_write', 'app_admin']) AS role
        ) r
    ) privileges
    WHERE n.nspname = 'app' AND c.relkind IN ('r', 'v') AND c.relname <> '__EFMigrationsHistory'
) o;
