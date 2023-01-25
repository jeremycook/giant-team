SET ROLE pg_database_owner;

-- SCHEMA: etc
-- DROP SCHEMA IF EXISTS etc CASCADE;

CREATE SCHEMA IF NOT EXISTS etc
    AUTHORIZATION pg_database_owner;
GRANT ALL ON SCHEMA etc TO pg_database_owner;
REVOKE ALL ON SCHEMA etc FROM PUBLIC;
GRANT USAGE ON SCHEMA etc TO PUBLIC;

-- Type: etc.permission_id
-- DROP TYPE IF EXISTS etc.permission_id;

SET search_path = etc;

CREATE TYPE etc.permission_id AS ENUM (
	'r', -- read
	'a', -- append
	'w', -- write
	'd', -- delete
	'm'  -- manage
);

ALTER TYPE etc.permission_id
    OWNER TO pg_database_owner;

GRANT USAGE ON TYPE etc.permission_id TO PUBLIC;

-- Table: etc.role
-- DROP TABLE IF EXISTS etc.role;

CREATE TABLE IF NOT EXISTS etc.role
(
    role_id character varying(50) NOT NULL, -- TODO: Check that it matches 'r:...' and is a member of this database
	name text NOT NULL,
	created timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
    CONSTRAINT role_role_id_format_check CHECK (role_id ~ '^r:[0-9a-f]{32}$'),
    CONSTRAINT role_role_id_connect_check CHECK (has_database_privilege(role_id::name, current_database()::text, 'CONNECT'::text)),
    CONSTRAINT role_pkey PRIMARY KEY (role_id),
    CONSTRAINT role_key UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.role
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.role TO pg_database_owner;
GRANT SELECT ON TABLE etc.role TO PUBLIC;

-- Table: etc.inode_type
-- DROP TABLE IF EXISTS etc.inode_type;

CREATE TABLE IF NOT EXISTS etc.inode_type
(
    inode_type_id text NOT NULL,
    CONSTRAINT inode_type_pkey PRIMARY KEY (inode_type_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.inode_type
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.inode_type TO pg_database_owner;
GRANT SELECT ON TABLE etc.inode_type TO PUBLIC;

-- Table: etc.inode_type_constraint
-- DROP TABLE IF EXISTS etc.inode_type_constraint;

CREATE TABLE IF NOT EXISTS etc.inode_type_constraint
(
    inode_type_id text NOT NULL,
    parent_inode_type_id text NOT NULL,
    CONSTRAINT inode_type_constraint_pkey PRIMARY KEY (inode_type_id, parent_inode_type_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.inode_type_constraint
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.inode_type_constraint TO pg_database_owner;
GRANT SELECT ON TABLE etc.inode_type_constraint TO PUBLIC;

-- Table: etc.inode
-- DROP TABLE IF EXISTS etc.inode;

CREATE TABLE IF NOT EXISTS etc.inode
(
    inode_id uuid NOT NULL DEFAULT (gen_random_uuid()),
    parent_inode_id uuid NOT NULL,
    name character varying(248) NOT NULL,
    ugly_name character varying(248) NOT NULL,
    inode_type_id text COLLATE pg_catalog."default" NOT NULL,
	created timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
	path text NOT NULL DEFAULT ('/*/' || gen_random_uuid()),
    CONSTRAINT inode_check CHECK (
		name ~ '^[^<>:"/\|?*]+$'
		AND ((inode_id <> parent_inode_id AND ugly_name ~ '^[^<>:"/\|?*]+$') OR (inode_id = '00000000-0000-0000-0000-000000000000' AND parent_inode_id = '00000000-0000-0000-0000-000000000000' AND ugly_name = ''))
	),
    CONSTRAINT inode_pkey PRIMARY KEY (inode_id),
    CONSTRAINT inode_key UNIQUE (path),
    CONSTRAINT inode_parent_inode_id_fkey FOREIGN KEY (parent_inode_id)
        REFERENCES etc.inode (inode_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT inode_inode_type_id_fkey FOREIGN KEY (inode_type_id)
        REFERENCES etc.inode_type (inode_type_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.inode
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.inode TO pg_database_owner;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE etc.inode TO PUBLIC;

-- FUNCTION: etc.inode_inode_type_id_is_valid(text, uuid)
-- DROP FUNCTION IF EXISTS etc.inode_inode_type_id_is_valid(text, uuid);

CREATE OR REPLACE FUNCTION etc.inode_inode_type_id_is_valid(
	_inode_inode_type_id text,
	_inode_parent_inode_id uuid)
    RETURNS boolean
    LANGUAGE 'sql'
    COST 100
    STABLE STRICT PARALLEL SAFE 

RETURN CASE
	WHEN _inode_inode_type_id IS NULL OR _inode_parent_inode_id IS NULL THEN NULL
	ELSE EXISTS (
		SELECT 1 
		FROM (etc.inode parent_inode 
			  JOIN etc.inode_type_constraint tc ON ((tc.parent_inode_type_id = parent_inode.inode_type_id))) 
		WHERE ((parent_inode.inode_id = inode_inode_type_id_is_valid._inode_parent_inode_id) 
			   AND (tc.inode_type_id = inode_inode_type_id_is_valid._inode_inode_type_id))
	)
END;

ALTER FUNCTION etc.inode_inode_type_id_is_valid(text, uuid)
    OWNER TO pg_database_owner;

GRANT ALL ON FUNCTION etc.inode_inode_type_id_is_valid(text, uuid) TO pg_database_owner;
GRANT EXECUTE ON FUNCTION etc.inode_inode_type_id_is_valid(text, uuid) TO PUBLIC;

-- FUNCTION: etc.inode_path_upserting_trigger()
-- Modifies the path when name or parent_inode_id change.

CREATE OR REPLACE FUNCTION etc.inode_path_upserting_trigger()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
    BEGIN
		IF NEW.inode_id = '00000000-0000-0000-0000-000000000000'::uuid THEN
			IF NEW.path IS NULL OR NEW.path <> '' THEN
				NEW.path = '';
			END IF;
		ELSIF NEW.parent_inode_id IS NOT NULL THEN
			NEW.path = (SELECT (CASE WHEN length(parent.path) > 0 THEN parent.path || '/' ELSE '' END) || NEW.ugly_name FROM etc.inode parent WHERE parent.inode_id = NEW.parent_inode_id);
		END IF;
        RETURN NEW;
    END;
$BODY$;

GRANT ALL ON FUNCTION etc.inode_path_upserting_trigger() TO pg_database_owner;
GRANT EXECUTE ON FUNCTION etc.inode_path_upserting_trigger() TO PUBLIC;

CREATE OR REPLACE TRIGGER inode_path_upserting_trigger
    BEFORE INSERT OR UPDATE OF name, parent_inode_id
    ON etc.inode
    FOR EACH ROW
    EXECUTE FUNCTION etc.inode_path_upserting_trigger();

-- FUNCTION: etc.inode_path_upserted_trigger()
-- Modifies the paths of its children when its path changes.

CREATE OR REPLACE FUNCTION etc.inode_path_upserted_trigger()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
    BEGIN
		-- Update children
		-- All descendants will be updated via this trigger recursively firing as each path is changed
		UPDATE etc.inode
		SET path = (CASE WHEN length(NEW.path) > 0 THEN NEW.path || '/' ELSE '' END) || inode.ugly_name
		WHERE inode.parent_inode_id = NEW.inode_id AND inode_id <> parent_inode_id;
        RETURN NEW;
    END;
$BODY$;

GRANT ALL ON FUNCTION etc.inode_path_upserted_trigger() TO pg_database_owner;
GRANT EXECUTE ON FUNCTION etc.inode_path_upserted_trigger() TO PUBLIC;

CREATE OR REPLACE TRIGGER inode_path_upserted_trigger
    AFTER INSERT OR UPDATE OF name, parent_inode_id, path
    ON etc.inode
    FOR EACH ROW
    EXECUTE FUNCTION etc.inode_path_upserted_trigger();

-- Insert Root inode before applying the inode_inode_type_id_check

INSERT INTO etc.inode_type (inode_type_id) values 
	('Root')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.inode_type_constraint (inode_type_id, parent_inode_type_id) values 
	('Root', 'Root')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.inode (inode_id, parent_inode_id, inode_type_id, name, ugly_name) values
	('00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', 'Root', 'New Organization', '')
	ON CONFLICT DO NOTHING;

-- FUNCTION: etc.get_inode_tree(uuid)
-- DROP FUNCTION IF EXISTS etc.get_inode_tree(uuid);

CREATE OR REPLACE FUNCTION etc.get_inode_tree(
	_inode_id uuid)
    RETURNS jsonb
    LANGUAGE 'plpgsql'
    COST 100
    STABLE STRICT PARALLEL UNSAFE
AS $BODY$
BEGIN
	RETURN json_build_object(
		'inode_id',
		inode_id,
		'name',
		"name",
		'children',
		array(
			SELECT etc.get_inode_tree(inode_id)
			FROM etc.inode 
			WHERE parent_inode_id = _inode_id AND inode_id <> parent_inode_id
		)
	)
	FROM etc.inode
	WHERE inode_id = _inode_id;
END;
$BODY$;

ALTER FUNCTION etc.get_inode_tree(uuid)
    OWNER TO pg_database_owner;

GRANT ALL ON FUNCTION etc.get_inode_tree(uuid) TO pg_database_owner;
GRANT EXECUTE ON FUNCTION etc.get_inode_tree(uuid) TO PUBLIC;

-- Check: inode_inode_type_id_check
-- ALTER TABLE etc.inode DROP CONSTRAINT IF EXISTS inode_inode_type_id_check;

DO $$BEGIN
IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_constraint WHERE conname = 'inode_inode_type_id_check') THEN
	ALTER TABLE etc.inode ADD CONSTRAINT inode_inode_type_id_check CHECK (etc.inode_inode_type_id_is_valid(inode_type_id, parent_inode_id));
END IF;
END$$;

-- Table: etc.permission
-- DROP TABLE IF EXISTS etc.permission;

CREATE TABLE IF NOT EXISTS etc.permission
(
    permission_id etc.permission_id NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT permission_pkey PRIMARY KEY (permission_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.permission
    OWNER to pg_database_owner;

INSERT INTO etc.permission (permission_id, name) VALUES 
	('r', 'Read file, list directory contents, select rows from table'),
	('a', 'Append data to end of file, create file in directory, insert rows into table'),
	('w', 'Write data into file, create subdirectory in directory, update rows in table'),
	('d', 'Delete file, delete directory, delete rows from table'),
	('m', 'Write the named attributes of the file/directory, alter the table')
	ON CONFLICT DO NOTHING;

-- Table: etc.inode_access
-- DROP TABLE IF EXISTS etc.inode_access;

CREATE TABLE IF NOT EXISTS etc.inode_access
(
    inode_id uuid NOT NULL,
    role_id text COLLATE pg_catalog."default" NOT NULL,
    permissions etc.permission_id[] NOT NULL,
    CONSTRAINT inode_access_pkey PRIMARY KEY (inode_id, role_id),
    CONSTRAINT inode_access_inode_id_fkey FOREIGN KEY (inode_id)
        REFERENCES etc.inode (inode_id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT inode_access_role_id_fkey FOREIGN KEY (role_id)
        REFERENCES etc.role (role_id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.inode_access
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.inode_access TO pg_database_owner;

GRANT SELECT ON TABLE etc.inode_access TO PUBLIC;

-- Index: inode_access_permissions_index
-- DROP INDEX IF EXISTS etc.inode_access_permissions_index;

CREATE INDEX IF NOT EXISTS inode_access_permissions_index
    ON etc.inode_access USING gin
    (permissions)
    TABLESPACE pg_default;

-- FUNCTION: etc.inode_access_granted(uuid, etc.permission_id[])
-- DROP FUNCTION IF EXISTS etc.inode_access_granted(uuid, etc.permission_id[]);

CREATE OR REPLACE FUNCTION etc.inode_access_granted(
	_inode_id uuid,
	_permissions etc.permission_id[])
    RETURNS boolean
    LANGUAGE 'sql'
    COST 100
    STABLE STRICT PARALLEL SAFE 

RETURN ( EXISTS (SELECT 1 FROM etc.inode_access WHERE (_inode_id = inode_id AND (inode_access_granted._permissions <@ inode_access.permissions) AND (pg_has_role(CURRENT_USER, (inode_access.role_id)::name, 'USAGE'::text) OR pg_has_role(CURRENT_USER, 'pg_database_owner'::name, 'USAGE'::text))) LIMIT 1));

ALTER FUNCTION etc.inode_access_granted(uuid, etc.permission_id[])
    OWNER TO pg_database_owner;

GRANT EXECUTE ON FUNCTION etc.inode_access_granted(uuid, etc.permission_id[]) TO pg_database_owner;
GRANT EXECUTE ON FUNCTION etc.inode_access_granted(uuid, etc.permission_id[]) TO PUBLIC;

-- POLICY: inode_select_rls
-- DROP POLICY IF EXISTS inode_select_rls ON etc.inode;

CREATE POLICY inode_select_rls
    ON etc.inode
    AS PERMISSIVE
    FOR SELECT
    TO public
    USING (etc.inode_access_granted(inode_id, ARRAY['r'::etc.permission_id]));

-- POLICY: inode_insert_rls
-- DROP POLICY IF EXISTS inode_insert_rls ON etc.inode;

CREATE POLICY inode_insert_rls
    ON etc.inode
    AS PERMISSIVE
    FOR INSERT
    TO public
    WITH CHECK (etc.inode_access_granted(inode_id, ARRAY['a'::etc.permission_id]));

-- POLICY: inode_update_rls
-- DROP POLICY IF EXISTS inode_update_rls ON etc.inode;

CREATE POLICY inode_update_rls
    ON etc.inode
    AS PERMISSIVE
    FOR UPDATE
    TO public
    USING (etc.inode_access_granted(inode_id, ARRAY['w'::etc.permission_id]));

-- POLICY: inode_delete_rls
-- DROP POLICY IF EXISTS inode_delete_rls ON etc.inode;

CREATE POLICY inode_delete_rls
    ON etc.inode
    AS PERMISSIVE
    FOR DELETE
    TO public
    USING (etc.inode_access_granted(inode_id, ARRAY['d'::etc.permission_id]));

ALTER TABLE IF EXISTS etc.inode
    ENABLE ROW LEVEL SECURITY;

-- Table: etc.file
-- DROP TABLE IF EXISTS etc.file;

CREATE TABLE IF NOT EXISTS etc.file
(
	inode_id uuid NOT NULL,
    content_type text NOT NULL,
    data bytea NOT NULL,
    CONSTRAINT file_pkey PRIMARY KEY (inode_id),
    CONSTRAINT file_inode_id_fkey FOREIGN KEY (inode_id)
        REFERENCES etc.inode (inode_id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS etc.file
    OWNER to pg_database_owner;

GRANT ALL ON TABLE etc.file TO pg_database_owner;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE etc.inode TO PUBLIC;

-- POLICY: file_select_rls
-- DROP POLICY IF EXISTS file_select_rls ON etc.file;

CREATE POLICY file_select_rls
    ON etc.file
    AS PERMISSIVE
    FOR SELECT
    TO public
    USING (etc.inode_access_granted(inode_id, ARRAY['r'::etc.permission_id]));

-- POLICY: file_insert_rls
-- DROP POLICY IF EXISTS file_insert_rls ON etc.file;

CREATE POLICY file_insert_rls
    ON etc.file
    AS PERMISSIVE
    FOR INSERT
    TO public
    WITH CHECK (etc.inode_access_granted(inode_id, ARRAY['a'::etc.permission_id]));

-- POLICY: file_update_rls
-- DROP POLICY IF EXISTS file_update_rls ON etc.file;

CREATE POLICY file_update_rls
    ON etc.file
    AS PERMISSIVE
    FOR UPDATE
    TO public
    USING (etc.inode_access_granted(inode_id, ARRAY['w'::etc.permission_id]));

-- POLICY: file_delete_rls
-- DROP POLICY IF EXISTS file_delete_rls ON etc.file;

CREATE POLICY file_delete_rls
    ON etc.file
    AS PERMISSIVE
    FOR DELETE
    TO public
    USING (etc.inode_access_granted(inode_id, ARRAY['d'::etc.permission_id]));

ALTER TABLE IF EXISTS etc.file
    ENABLE ROW LEVEL SECURITY;

-- VIEW: etc.database_definition
-- DROP VIEW etc.database_definition;

CREATE OR REPLACE VIEW etc.database_definition
 AS
 WITH columns AS (
         SELECT c.table_catalog AS catalog_name,
            c.table_schema AS schema_name,
            c.table_name,
            c.ordinal_position AS position,
            c.column_name AS name,
            c.udt_name AS "storeInodeType",
                CASE
                    WHEN c.is_nullable::text = 'YES'::text THEN true
                    ELSE false
                END AS "isNullable",
            c.column_default AS "columnDefault",
            c.generation_expression AS "generationExpression",
                CASE
                    WHEN c.is_updatable::text = 'YES'::text THEN true
                    ELSE false
                END AS "isUpdatable"
           FROM information_schema.columns c
        ), indexes AS (
         SELECT schema_ns.nspname AS schema_name,
            table_class.relname AS table_name,
            index_class.relname AS name,
                CASE
                    WHEN i.indisprimary THEN 2
                    WHEN i.indisunique THEN 1
                    ELSE 0
                END AS "indexInodeType",
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
                END AS "isInsertableInto",
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
GRANT SELECT ON TABLE etc.database_definition TO PUBLIC;

-- DATA:

INSERT INTO etc.inode_type (inode_type_id) values 
	('Space'),
	('Folder'),
	('File'),
	('Table')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.inode_type_constraint (inode_type_id, parent_inode_type_id) values 
	('Space', 'Root'),
	('Folder', 'Space'), ('Folder', 'Folder'),
	('File', 'Space'), ('File', 'Folder'),
	('Table', 'Space'), ('Table', 'Folder')
	ON CONFLICT DO NOTHING;

INSERT INTO etc.inode (inode_id, parent_inode_id, inode_type_id, name, ugly_name) values
	('3e544ebc-f30a-471f-a8ec-f9e3ac84f19a', '00000000-0000-0000-0000-000000000000', 'Space', 'etc', 'etc')
	ON CONFLICT DO NOTHING;
