-- Prints the table part of docs/database/data-dictionary.md from the database catalog and the
-- COMMENT ON descriptions written by the migrations. Run with:
--   psql "<connection>" -X -A -t -f database/scripts/generate_data_dictionary.sql > dictionary-tables.md

WITH columns AS (
    SELECT c.table_name,
           c.ordinal_position,
           c.column_name,
           CASE
               WHEN c.data_type = 'character varying' THEN 'varchar(' || c.character_maximum_length || ')'
               WHEN c.data_type = 'numeric' THEN 'numeric(' || c.numeric_precision || ',' || c.numeric_scale || ')'
               WHEN c.data_type = 'USER-DEFINED' THEN c.udt_name
               WHEN c.data_type = 'timestamp with time zone' THEN 'timestamptz'
               WHEN c.data_type = 'time without time zone' THEN 'time'
               ELSE c.data_type
           END AS data_type,
           c.is_nullable = 'NO' AS not_null,
           c.is_identity = 'YES' AS is_identity,
           c.column_default,
           col_description(format('app.%I', c.table_name)::regclass, c.ordinal_position) AS description
    FROM information_schema.columns c
    WHERE c.table_schema = 'app'
      AND c.table_name IN (SELECT table_name FROM information_schema.tables WHERE table_schema = 'app' AND table_type = 'BASE TABLE')
      AND c.table_name <> '__EFMigrationsHistory'
),
keys AS (
    SELECT kcu.table_name,
           kcu.column_name,
           string_agg(
               CASE tc.constraint_type
                   WHEN 'PRIMARY KEY' THEN 'PK'
                   WHEN 'UNIQUE' THEN 'UQ'
                   WHEN 'FOREIGN KEY' THEN 'FK ' || ccu.table_name || '.' || ccu.column_name
               END, ', ' ORDER BY tc.constraint_type DESC) AS key_info
    FROM information_schema.table_constraints tc
             JOIN information_schema.key_column_usage kcu
                  ON kcu.constraint_name = tc.constraint_name AND kcu.table_schema = tc.table_schema
             LEFT JOIN information_schema.constraint_column_usage ccu
                       ON ccu.constraint_name = tc.constraint_name AND tc.constraint_type = 'FOREIGN KEY'
    WHERE tc.table_schema = 'app' AND tc.constraint_type IN ('PRIMARY KEY', 'UNIQUE', 'FOREIGN KEY')
    GROUP BY kcu.table_name, kcu.column_name
),
unique_indexes AS (
    SELECT t.relname AS table_name, a.attname AS column_name
    FROM pg_index i
             JOIN pg_class t ON t.oid = i.indrelid
             JOIN pg_namespace n ON n.oid = t.relnamespace AND n.nspname = 'app'
             JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY (i.indkey)
    WHERE i.indisunique AND NOT i.indisprimary AND i.indnatts = 1 AND i.indpred IS NULL
),
tables AS (
    SELECT c.relname AS table_name, obj_description(c.oid, 'pg_class') AS description
    FROM pg_class c
             JOIN pg_namespace n ON n.oid = c.relnamespace
    WHERE n.nspname = 'app' AND c.relkind = 'r' AND c.relname <> '__EFMigrationsHistory'
),
checks AS (
    SELECT conrelid::regclass::text AS table_name,
           string_agg('`' || conname || '`: `' || pg_get_constraintdef(oid) || '`', '<br>' ORDER BY conname) AS rules
    FROM pg_constraint
    WHERE connamespace = 'app'::regnamespace AND contype IN ('c', 'x')
    GROUP BY conrelid
)
SELECT string_agg(section, E'\n' ORDER BY table_name)
FROM (
    SELECT t.table_name,
           '### ' || t.table_name || E'\n\n' || coalesce(t.description, '') || E'\n\n'
               || E'| Column | Type | Null | Keys | Default | Description |\n|---|---|---|---|---|---|\n'
               || (SELECT string_agg(
                              '| ' || c.column_name
                                  || ' | ' || c.data_type
                                  || ' | ' || CASE WHEN c.not_null THEN 'NOT NULL' ELSE 'NULL' END
                                  || ' | ' || trim(both ', ' FROM concat_ws(', ', k.key_info,
                                                   CASE WHEN u.column_name IS NOT NULL AND coalesce(k.key_info, '') NOT LIKE '%UQ%' THEN 'UQ' END))
                                  || ' | ' || CASE WHEN c.is_identity THEN 'identity' ELSE coalesce(replace(c.column_default, '|', '\|'), '') END
                                  || ' | ' || coalesce(c.description, '') || ' |',
                              E'\n' ORDER BY c.ordinal_position)
                   FROM columns c
                            LEFT JOIN keys k ON k.table_name = c.table_name AND k.column_name = c.column_name
                            LEFT JOIN unique_indexes u ON u.table_name = c.table_name AND u.column_name = c.column_name
                   WHERE c.table_name = t.table_name)
               || coalesce(E'\n\nRules: ' || ch.rules, '')
               || E'\n' AS section
    FROM tables t
             LEFT JOIN checks ch ON ch.table_name = 'app.' || t.table_name OR ch.table_name = t.table_name
) sections;
