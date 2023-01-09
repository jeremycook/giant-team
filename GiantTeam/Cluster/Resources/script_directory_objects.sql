-- Log into the directory database as directory_owner or a member of it to run this script

SET ROLE directory_owner;

-- Configure database access

GRANT ALL ON DATABASE directory TO pg_database_owner;
GRANT TEMPORARY, CONNECT ON DATABASE directory TO security_manager;
GRANT TEMPORARY, CONNECT ON DATABASE directory TO directory_manager;
GRANT TEMPORARY, CONNECT ON DATABASE directory TO anyone;
REVOKE ALL ON DATABASE directory FROM public;

SET ROLE pg_database_owner;

-- Drop the public schema

DROP SCHEMA IF EXISTS public;

-- SCHEMA: directory
-- DROP SCHEMA IF EXISTS directory CASCADE;

CREATE SCHEMA IF NOT EXISTS directory
    AUTHORIZATION pg_database_owner;

GRANT ALL ON SCHEMA directory TO pg_database_owner;
GRANT USAGE ON SCHEMA directory TO directory_manager;
GRANT USAGE ON SCHEMA directory TO anyone;

-- Table: directory.organization
-- DROP TABLE IF EXISTS directory.organization;

CREATE TABLE IF NOT EXISTS directory.organization
(
    organization_id character varying(50) NOT NULL,
    name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    database_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    created timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    CONSTRAINT organization_pkey PRIMARY KEY (organization_id),
    CONSTRAINT organization_database_name_key UNIQUE (database_name),
    CONSTRAINT organization_database_name_check CHECK (database_name::text ~ '^[a-z][a-z0-9_]*$'::text)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.organization
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.organization TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.organization TO directory_manager;
GRANT SELECT ON TABLE directory.organization TO anyone;

-- Table: directory.organization_role
-- DROP TABLE IF EXISTS directory.organization_role;

CREATE TABLE IF NOT EXISTS directory.organization_role
(
    organization_role_id uuid NOT NULL DEFAULT gen_random_uuid(),
    created timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    organization_id character varying(50) NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    db_role text COLLATE pg_catalog."default" NOT NULL,
    description text COLLATE pg_catalog."default",
    CONSTRAINT organization_role_pkey PRIMARY KEY (organization_role_id),
    CONSTRAINT organization_role_organization_id_fkey FOREIGN KEY (organization_id)
        REFERENCES directory.organization (organization_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.organization_role
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.organization_role TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.organization_role TO directory_manager;

-- Table: directory.user
-- DROP TABLE IF EXISTS directory.user;

CREATE TABLE IF NOT EXISTS directory.user
(
    user_id uuid NOT NULL DEFAULT gen_random_uuid(),
    name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    username character varying(50) COLLATE pg_catalog."default" NOT NULL,
    db_user character varying(60) COLLATE pg_catalog."default" NOT NULL,
    email character varying(200) COLLATE pg_catalog."default" NOT NULL,
    email_verified boolean NOT NULL DEFAULT false,
    email_lower character varying(200) COLLATE pg_catalog."default" NOT NULL GENERATED ALWAYS AS (lower(email)) STORED,
    created timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    CONSTRAINT user_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_username_key UNIQUE (username),
    CONSTRAINT user_username_check CHECK (username::text ~ '^[a-z][a-z0-9_]*$'::text),
    CONSTRAINT user_db_user_check CHECK (db_user::text ~ '^u:[a-z][a-z0-9_]*$'::text),
    CONSTRAINT user_email_lower_key UNIQUE (email_lower)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.user
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.user TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.user TO directory_manager;

-- Table: directory.user_password

-- DROP TABLE IF EXISTS directory.user_password;

CREATE TABLE IF NOT EXISTS directory.user_password
(
    password_digest text COLLATE pg_catalog."default" NOT NULL,
    user_id uuid NOT NULL,
    CONSTRAINT user_password_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_password_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES directory.user (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.user_password
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.user_password TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.user_password TO directory_manager;
