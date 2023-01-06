SET ROLE pg_database_owner;

-- SCHEMA: spaces
-- DROP SCHEMA IF EXISTS spaces CASCADE;
CREATE SCHEMA IF NOT EXISTS spaces
    AUTHORIZATION pg_database_owner;
GRANT USAGE ON SCHEMA spaces TO PUBLIC;
GRANT ALL ON SCHEMA spaces TO pg_database_owner;

-- TABLE: spaces.spaces
-- DROP TABLE IF EXISTS spaces.spaces;

CREATE TABLE IF NOT EXISTS spaces.spaces
(
    space_id character varying(50) COLLATE pg_catalog."default" NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    schema_name character varying(50) COLLATE pg_catalog."default",
    created timestamp with time zone DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    CONSTRAINT space_pkey PRIMARY KEY (space_id),
    CONSTRAINT space_check CHECK (schema_name::text = space_id::text)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS spaces.spaces
    OWNER to pg_database_owner;

-- VIEW: spaces.database_definition
-- DROP VIEW spaces.database_definition;
CREATE OR REPLACE VIEW spaces.database_definition
 AS
 WITH columns AS (
         SELECT c.table_catalog AS catalog_name,
            c.table_schema AS schema_name,
            c.table_name,
            c.ordinal_position AS "position",
            c.column_name AS name,
            c.udt_name AS store_type,
                CASE
                    WHEN c.is_nullable::text = 'YES'::text THEN true
                    ELSE false
                END AS is_nullable,
            c.column_default,
            c.generation_expression,
                CASE
                    WHEN c.is_updatable::text = 'YES'::text THEN true
                    ELSE false
                END AS is_updatable
           FROM information_schema.columns c
        ), indexes AS (
         SELECT schema_ns.nspname AS schema_name,
            table_class.relname AS table_name,
            index_class.relname AS name,
                CASE
                    WHEN i.indisprimary THEN 2
                    WHEN i.indisunique THEN 1
                    ELSE 0
                END AS index_type,
            json_agg(c.column_name) AS columns
           FROM pg_namespace schema_ns
             JOIN pg_class index_class ON index_class.relnamespace = schema_ns.oid
             JOIN pg_index i ON i.indexrelid = index_class.oid
             JOIN pg_class table_class ON i.indrelid = table_class.oid
             JOIN information_schema.columns c ON c.table_schema::name = schema_ns.nspname AND c.table_name::name = table_class.relname AND (c.ordinal_position::integer = ANY (i.indkey::smallint[]))
          WHERE schema_ns.nspname !~ '^pg_'::text AND index_class.relkind = 'i'::"char"
          GROUP BY schema_ns.nspname, table_class.relname, index_class.relname, i.indisunique, i.indisprimary
        ), tables AS (
         SELECT st.table_catalog AS catalog_name,
            st.table_schema AS schema_name,
            st.table_name AS name,
            t.tableowner AS owner,
                CASE
                    WHEN st.is_insertable_into::text = 'YES'::text THEN true
                    ELSE false
                END AS is_insertable_into,
            COALESCE(c.columns, '[]'::json) AS columns,
            COALESCE(i.indexes, '[]'::json) AS indexes
           FROM information_schema.tables st
             JOIN pg_tables t ON st.table_schema::name = t.schemaname AND st.table_name::name = t.tablename
             LEFT JOIN ( SELECT c_1.catalog_name,
                    c_1.schema_name,
                    c_1.table_name,
                    json_agg(to_jsonb(c_1.*) - ARRAY['catalog_name'::text, 'schema_name'::text, 'table_name'::text] ORDER BY c_1."position", c_1.name) AS columns
                   FROM columns c_1
                  GROUP BY c_1.catalog_name, c_1.schema_name, c_1.table_name) c ON c.catalog_name::name = st.table_catalog::name AND c.schema_name::name = st.table_schema::name AND c.table_name::name = st.table_name::name
             LEFT JOIN ( SELECT c_1.schema_name,
                    c_1.table_name,
                    json_agg(to_jsonb(c_1.*) - ARRAY['catalog_name'::text, 'schema_name'::text, 'table_name'::text]) AS indexes
                   FROM indexes c_1
                  GROUP BY c_1.schema_name, c_1.table_name) i ON i.schema_name = st.table_schema::name AND i.table_name = st.table_name::name
        ), schemas AS (
         SELECT s.catalog_name,
            s.schema_name AS name,
            s.schema_owner AS owner,
            COALESCE(t.tables, '[]'::json) AS tables
           FROM information_schema.schemata s
             LEFT JOIN ( SELECT t_1.catalog_name,
                    t_1.schema_name,
                    json_agg(to_jsonb(t_1.*) - ARRAY['catalog_name'::text, 'schema_name'::text] ORDER BY t_1.name) AS tables
                   FROM tables t_1
                  GROUP BY t_1.catalog_name, t_1.schema_name) t ON t.catalog_name::name = s.catalog_name::name AND t.schema_name::name = s.schema_name::name
          WHERE s.schema_name::name <> ALL (ARRAY['information_schema'::name, 'pg_catalog'::name])
        ), databases AS (
         SELECT d.catalog_name AS name,
            d.catalog_owner AS owner,
            COALESCE(s.schemas, '[]'::json) AS schemas
           FROM ( SELECT d_1.datname::text AS catalog_name,
                    d_1.datdba::regrole::text AS catalog_owner
                   FROM pg_database d_1
                  WHERE d_1.datname = CURRENT_CATALOG) d
             LEFT JOIN ( SELECT s_1.catalog_name,
                    json_agg(to_jsonb(s_1.*) - ARRAY['catalog_name'::text] ORDER BY 'Name'::text) AS schemas
                   FROM schemas s_1
                  GROUP BY s_1.catalog_name) s ON s.catalog_name::name = d.catalog_name
        )
 SELECT databases.name,
    databases.owner,
    databases.schemas
   FROM databases;

ALTER TABLE spaces.database_definition
    OWNER TO pg_database_owner;

GRANT ALL ON TABLE spaces.database_definition TO pg_database_owner;
GRANT SELECT ON TABLE spaces.database_definition TO PUBLIC;
