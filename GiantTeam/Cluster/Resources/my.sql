SET ROLE pg_database_owner;

-- SCHEMA: my
-- DROP SCHEMA IF EXISTS my ;

CREATE SCHEMA IF NOT EXISTS my
    AUTHORIZATION pg_database_owner;

GRANT ALL ON SCHEMA my TO pg_database_owner;

GRANT USAGE ON SCHEMA my TO PUBLIC;

-- VIEW: my.organizations
-- DROP VIEW my.organizations;

CREATE OR REPLACE VIEW my.organizations
 AS
 SELECT pg_database.datname AS id,
    pg_database.datname AS database_name
   FROM pg_database
     JOIN pg_roles owner ON owner.oid = pg_database.datdba
  WHERE NOT owner.rolsuper AND pg_database.datname <> current_database() AND has_database_privilege(CURRENT_ROLE, pg_database.oid, 'CONNECT'::text);

ALTER TABLE my.organizations
    OWNER TO pg_database_owner;

GRANT ALL ON TABLE my.organizations TO pg_database_owner;
GRANT SELECT ON TABLE my.organizations TO PUBLIC;
