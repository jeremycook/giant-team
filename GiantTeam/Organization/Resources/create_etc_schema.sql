SET ROLE pg_database_owner;

-- SCHEMA: etc
-- DROP SCHEMA IF EXISTS etc CASCADE;

CREATE SCHEMA IF NOT EXISTS etc
    AUTHORIZATION pg_database_owner;
GRANT ALL ON SCHEMA etc TO pg_database_owner;
GRANT USAGE ON SCHEMA etc TO anyone;
REVOKE ALL ON SCHEMA etc FROM PUBLIC;

SET search_path = etc;

-- Table: etc.type
-- DROP TABLE IF EXISTS etc.type;

CREATE TABLE IF NOT EXISTS etc.type
(
    type_id text NOT NULL,
    CONSTRAINT type_pkey PRIMARY KEY (type_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.type
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.type TO pg_database_owner;
GRANT SELECT ON TABLE etc.type TO anyone;

-- Table: etc.type_constraint
-- DROP TABLE IF EXISTS etc.type_constraint;

CREATE TABLE IF NOT EXISTS etc.type_constraint
(
    type_id text NOT NULL,
    parent_type_id text NOT NULL,
    CONSTRAINT type_constraint_pkey PRIMARY KEY (type_id, parent_type_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.type_constraint
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.type_constraint TO pg_database_owner;
GRANT SELECT ON TABLE etc.type_constraint TO anyone;

-- Table: etc.node
-- DROP TABLE IF EXISTS etc.node;

CREATE TABLE IF NOT EXISTS etc.node
(
    node_id uuid NOT NULL DEFAULT (gen_random_uuid()),
    parent_id uuid NOT NULL,
    name character varying(248) NOT NULL,
    type_id text COLLATE pg_catalog."default" NOT NULL,
	created timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
	name_lower character varying(248) NOT NULL GENERATED ALWAYS AS (lower(name)) STORED,
    CONSTRAINT node_check CHECK ((node_id <> parent_id AND name ~ '^[^<>:"/\|?*]+$') OR (node_id = '00000000-0000-0000-0000-000000000000' AND parent_id = '00000000-0000-0000-0000-000000000000')),
    CONSTRAINT node_pkey PRIMARY KEY (node_id),
    CONSTRAINT node_key UNIQUE (parent_id, name_lower),
    CONSTRAINT node_parent_id_fkey FOREIGN KEY (parent_id)
        REFERENCES etc.node (node_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT node_type_id_fkey FOREIGN KEY (type_id)
        REFERENCES etc.type (type_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.node
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.node TO pg_database_owner;

-- Function: etc.check_node_type
-- DROP FUNCTION IF EXISTS etc.check_node_type;

CREATE OR REPLACE FUNCTION etc.check_node_type(node_parent_id uuid, node_type_id text) RETURNS boolean
    LANGUAGE SQL
    IMMUTABLE
	RETURN EXISTS (SELECT 1
				   FROM etc.node parent_node
				   JOIN etc.type_constraint tc ON tc.parent_type_id = parent_node.type_id
				   WHERE parent_node.node_id = node_parent_id AND tc.type_id = node_type_id);

-- Insert Root node before applying the node_type_id_check

INSERT INTO etc.type (type_id) values 
	('Root')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.type_constraint (type_id, parent_type_id) values 
	('Root', 'Root')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.node (node_id, parent_id, name, type_id) values
	('00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 'Root', 'Root')
	ON CONFLICT DO NOTHING;

-- Check: node_type_id_check
-- ALTER TABLE etc.node DROP CONSTRAINT IF EXISTS node_type_id_check;

DO $$BEGIN
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'node_type_id_check') THEN
	ALTER TABLE etc.node ADD CONSTRAINT node_type_id_check CHECK (etc.check_node_type(parent_id, type_id));
END IF;
END$$;

-- Table: etc.file
-- DROP TABLE IF EXISTS etc.file;

CREATE TABLE IF NOT EXISTS etc.file
(
	node_id uuid NOT NULL,
    content_type text NOT NULL,
    data bytea NOT NULL,
    CONSTRAINT file_pkey PRIMARY KEY (node_id),
    CONSTRAINT file_node_id_fkey FOREIGN KEY (node_id)
        REFERENCES etc.node (node_id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.file
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.file TO pg_database_owner;
GRANT SELECT, INSERT, UPDATE ON TABLE etc.file TO anyone;

-- VIEW: etc.database_definition
-- DROP VIEW etc.database_definition;

CREATE OR REPLACE VIEW etc.database_definition
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

ALTER TABLE etc.database_definition
    OWNER TO pg_database_owner;

GRANT ALL ON TABLE etc.database_definition TO pg_database_owner;
GRANT SELECT ON TABLE etc.database_definition TO anyone;

-- View: etc.node_path

-- DROP VIEW etc.node_path;

CREATE OR REPLACE VIEW etc.node_path
 AS
 WITH RECURSIVE cte AS (
         SELECT node.node_id,
            '/'::text || node.name::text AS path
           FROM etc.node
          WHERE node.parent_id = '00000000-0000-0000-0000-000000000000'::uuid AND node.node_id <> node.parent_id
        UNION ALL
         SELECT node.node_id,
            (cte_1.path || '/'::text) || node.name::text
           FROM cte cte_1
             JOIN etc.node ON cte_1.node_id = node.parent_id
        )
 SELECT cte.node_id,
    cte.path
   FROM cte
  ORDER BY cte.path;

ALTER TABLE etc.node_path
    OWNER TO pg_database_owner;

GRANT ALL ON TABLE etc.node_path TO pg_database_owner;
GRANT SELECT ON TABLE etc.node_path TO anyone;

-- DATA:

INSERT INTO etc.type (type_id) values 
	('Space'),
	('Folder'),
	('File')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.type_constraint (type_id, parent_type_id) values 
	('Space', 'Root'),
	('Folder', 'Space'), ('Folder', 'Folder'),
	('File', 'Space'), ('File', 'Folder')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.node (node_id, parent_id, name, type_id) values
	('3e544ebc-f30a-471f-a8ec-f9e3ac84f19a', '00000000-0000-0000-0000-000000000000', 'etc', 'Space')
	ON CONFLICT DO NOTHING;

-- TODO: Remove these test nodes

INSERT INTO etc.node (node_id, parent_id, name, type_id) values
	('9b3eb00c-5845-4b0e-97d0-779985883568', '3e544ebc-f30a-471f-a8ec-f9e3ac84f19a', 'My Folder', 'Folder');

INSERT INTO etc.node (node_id, parent_id, name, type_id) values
	('2b49819a-22f1-4645-a81f-f2d8baa3595f', '3e544ebc-f30a-471f-a8ec-f9e3ac84f19a', 'My File', 'File'),
	('36e41bf0-1eb3-44ee-ae50-9d18befd965b', '9b3eb00c-5845-4b0e-97d0-779985883568', 'My File', 'File');

INSERT INTO etc.file (node_id, content_type, data) values
	('2b49819a-22f1-4645-a81f-f2d8baa3595f', 'text/plain', convert_to('Hello World', 'UTF8'));
