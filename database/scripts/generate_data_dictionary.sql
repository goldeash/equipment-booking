-- Prints the generated part of docs/database/data-dictionary.md (reference values, relationships and
-- tables) from the database catalog and the COMMENT ON descriptions written by the migrations. Run with:
--   psql "<connection>" -X -A -t -f database/scripts/generate_data_dictionary.sql > dictionary-generated.md

-- Reference values: every table whose primary key is a readable code, and the allowed status changes.
SELECT E'## Reference values\n\nReference rows are inserted by the migrations and are read only for the application role.\n\n'
           || E'| Table | Codes |\n|---|---|\n'
           || string_agg('| ' || t.table_name || ' | ' || t.codes || ' |', E'\n' ORDER BY t.table_name)
           || E'\n| booking_status_transitions | '
           || (SELECT string_agg(from_status_code || ' to ' || to_status_code, ', ' ORDER BY from_status_code, to_status_code)
               FROM app.booking_status_transitions)
           || E' |\n'
FROM (
    SELECT c.relname AS table_name,
           (xpath('/row/codes/text()',
                  query_to_xml(format('SELECT string_agg(code, '', '' ORDER BY code) AS codes FROM app.%I', c.relname), false, true, '')))[1]::text AS codes
    FROM pg_constraint pk
             JOIN pg_class c ON c.oid = pk.conrelid
             JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = pk.conkey[1]
    WHERE pk.connamespace = 'app'::regnamespace
      AND pk.contype = 'p'
      AND array_length(pk.conkey, 1) = 1
      AND a.attname = 'code'
) t;

-- Relationships: every foreign key with its cardinality, whether it is required and its delete rule.
SELECT E'## Relationships\n\nEvery foreign key of the schema. One-to-one means that the referencing columns are unique; '
           || E'optional means that they may be null. Restrict prevents deleting a referenced row; cascade deletes the '
           || E'referencing rows with it; set null keeps them without the reference.\n\n'
           || E'| Table | Columns | References | Cardinality | On delete |\n|---|---|---|---|---|\n'
           || string_agg('| ' || f.child || ' | ' || f.columns || ' | ' || f.parent || ' | ' || f.cardinality || ' | ' || f.on_delete || ' |',
                         E'\n' ORDER BY f.child, f.columns)
           || E'\n'
FROM (
    SELECT child.relname AS child,
           (SELECT string_agg(a.attname, ', ' ORDER BY array_position(con.conkey, a.attnum))
            FROM pg_attribute a WHERE a.attrelid = con.conrelid AND a.attnum = ANY (con.conkey)) AS columns,
           parent.relname || ' (' || (SELECT string_agg(a.attname, ', ' ORDER BY array_position(con.confkey, a.attnum))
                                      FROM pg_attribute a WHERE a.attrelid = con.confrelid AND a.attnum = ANY (con.confkey)) || ')' AS parent,
           CASE
               WHEN EXISTS (SELECT 1 FROM pg_index i
                            WHERE i.indrelid = con.conrelid AND i.indisunique AND i.indpred IS NULL
                              AND i.indnatts = array_length(con.conkey, 1) AND i.indkey::int2[] @> con.conkey)
                   THEN 'one-to-one'
               ELSE 'many-to-one'
           END
               || CASE
                      WHEN (SELECT bool_and(a.attnotnull) FROM pg_attribute a WHERE a.attrelid = con.conrelid AND a.attnum = ANY (con.conkey))
                          THEN ', required'
                      ELSE ', optional'
                  END AS cardinality,
           CASE con.confdeltype
               WHEN 'r' THEN 'restrict'
               WHEN 'c' THEN 'cascade'
               WHEN 'n' THEN 'set null'
               WHEN 'd' THEN 'set default'
               ELSE 'no action'
           END AS on_delete
    FROM pg_constraint con
             JOIN pg_class child ON child.oid = con.conrelid
             JOIN pg_class parent ON parent.oid = con.confrelid
    WHERE con.connamespace = 'app'::regnamespace AND con.contype = 'f'
) f;

-- Tables: columns, types, keys, defaults, descriptions and rules.
SELECT E'## Tables\n';

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
