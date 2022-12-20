-- SCHEMA: gt
-- DROP SCHEMA IF EXISTS gt CASCADE;

CREATE SCHEMA IF NOT EXISTS gt
    AUTHORIZATION pg_database_owner;

GRANT ALL ON SCHEMA gt TO pg_database_owner;
GRANT USAGE ON SCHEMA gt TO PUBLIC;

-- VIEW: gt.workspace
-- DROP VIEW gt.workspace;

CREATE OR REPLACE VIEW gt.workspace
 AS
 WITH columns AS (
         SELECT c.table_catalog AS catalog_name,
            c.table_schema AS schema_name,
            c.table_name,
            c.column_name AS "Name",
            c.data_type AS "DataType",
                CASE
                    WHEN c.is_nullable::text = 'YES'::text THEN true
                    ELSE false
                END AS "IsNullable",
                CASE
                    WHEN c.is_updatable::text = 'YES'::text THEN true
                    ELSE false
                END AS "IsUpdatable"
           FROM information_schema.columns c
        ), tables AS (
         SELECT st.table_catalog AS catalog_name,
            st.table_schema AS schema_name,
            st.table_name AS "Name",
            t.tableowner AS "Owner",
                CASE
                    WHEN st.is_insertable_into::text = 'YES'::text THEN true
                    ELSE false
                END AS "IsInsertableInto",
            COALESCE(c.columns, '[]'::json) AS "Columns"
           FROM information_schema.tables st
             JOIN pg_tables t ON st.table_schema::name = t.schemaname AND st.table_name::name = t.tablename
             LEFT JOIN ( SELECT c_1.catalog_name,
                    c_1.schema_name,
                    c_1.table_name,
                    json_agg(to_jsonb(c_1.*) - ARRAY['catalog_name'::text, 'schema_name'::text, 'table_name'::text] ORDER BY 'Name'::text) AS columns
                   FROM columns c_1
                  GROUP BY c_1.catalog_name, c_1.schema_name, c_1.table_name) c ON c.catalog_name::name = st.table_catalog::name AND c.schema_name::name = st.table_schema::name AND c.table_name::name = st.table_name::name
        ), schemas AS (
         SELECT s.catalog_name,
            s.schema_name AS "Name",
            s.schema_owner AS "Owner",
            COALESCE(t.tables, '[]'::json) AS "Tables"
           FROM information_schema.schemata s
             LEFT JOIN ( SELECT t_1.catalog_name,
                    t_1.schema_name,
                    json_agg(to_jsonb(t_1.*) - ARRAY['catalog_name'::text, 'schema_name'::text] ORDER BY 'Name'::text) AS tables
                   FROM tables t_1
                  GROUP BY t_1.catalog_name, t_1.schema_name) t ON t.catalog_name::name = s.catalog_name::name AND t.schema_name::name = s.schema_name::name
          WHERE s.schema_name::name <> ALL (ARRAY['information_schema'::name, 'pg_catalog'::name])
        ), databases AS (
         SELECT d.catalog_name AS "Name",
            d.catalog_owner AS "Owner",
            COALESCE(s.schemas, '[]'::json) AS "Schemas"
           FROM ( SELECT d_1.datname::text AS catalog_name,
                    d_1.datdba::regrole::text AS catalog_owner
                   FROM pg_database d_1
                  WHERE d_1.datname = CURRENT_CATALOG) d
             LEFT JOIN ( SELECT s_1.catalog_name,
                    json_agg(to_jsonb(s_1.*) - ARRAY['catalog_name'::text] ORDER BY 'Name'::text) AS schemas
                   FROM schemas s_1
                  GROUP BY s_1.catalog_name) s ON s.catalog_name::name = d.catalog_name
        )
 SELECT databases."Name",
    databases."Owner",
    databases."Schemas"
   FROM databases;

ALTER TABLE gt.workspace OWNER TO pg_database_owner;

GRANT ALL ON TABLE gt.workspace TO pg_database_owner;
GRANT SELECT ON TABLE gt.workspace TO PUBLIC;
