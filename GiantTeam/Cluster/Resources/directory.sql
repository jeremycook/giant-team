SET ROLE "directory:owner";

-- DATABASE: directory

GRANT ALL ON DATABASE directory TO pg_database_owner;
GRANT TEMPORARY, CONNECT ON DATABASE directory TO "directory:manager";
GRANT TEMPORARY, CONNECT ON DATABASE directory TO anyone;

SET ROLE pg_database_owner;

-- SCHEMA: directory
-- DROP SCHEMA IF EXISTS directory CASCADE;

CREATE SCHEMA IF NOT EXISTS directory
    AUTHORIZATION pg_database_owner;

GRANT ALL ON SCHEMA directory TO pg_database_owner;
GRANT USAGE ON SCHEMA directory TO "directory:manager";
GRANT USAGE ON SCHEMA directory TO anyone;

-- Table: directory.organizations
-- DROP TABLE IF EXISTS directory.organizations;

CREATE TABLE IF NOT EXISTS directory.organizations
(
    organization_id character varying(50) NOT NULL,
    name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    database_name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    created timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    CONSTRAINT organizations_pkey PRIMARY KEY (organization_id),
    CONSTRAINT organizations_database_name_key UNIQUE (database_name),
    CONSTRAINT organizations_database_name_check CHECK (database_name::text ~ '^[a-z][a-z0-9_]*$'::text)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.organizations
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.organizations TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.organizations TO "directory:manager";
GRANT SELECT ON TABLE directory.organizations TO anyone;

-- Table: directory.organization_roles
-- DROP TABLE IF EXISTS directory.organization_roles;

CREATE TABLE IF NOT EXISTS directory.organization_roles
(
    organization_role_id uuid NOT NULL DEFAULT gen_random_uuid(),
    created timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    organization_id character varying(50) NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    db_role text COLLATE pg_catalog."default" NOT NULL,
    description text COLLATE pg_catalog."default",
    CONSTRAINT organization_roles_pkey PRIMARY KEY (organization_role_id),
    CONSTRAINT organization_roles_organization_id_fkey FOREIGN KEY (organization_id)
        REFERENCES directory.organizations (organization_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.organization_roles
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.organization_roles TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.organization_roles TO "directory:manager";

-- Table: directory.users
-- DROP TABLE IF EXISTS directory.users;

CREATE TABLE IF NOT EXISTS directory.users
(
    user_id uuid NOT NULL DEFAULT gen_random_uuid(),
    name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    username character varying(50) COLLATE pg_catalog."default" NOT NULL,
    db_user character varying(60) COLLATE pg_catalog."default" NOT NULL,
    email character varying(200) COLLATE pg_catalog."default" NOT NULL,
    email_verified boolean NOT NULL DEFAULT false,
    email_lower character varying(200) COLLATE pg_catalog."default" NOT NULL GENERATED ALWAYS AS (lower(email)) STORED,
    created timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'::text),
    CONSTRAINT users_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_username_key UNIQUE (username),
    CONSTRAINT users_username_check CHECK (username::text ~ '^[a-z][a-z0-9_]*$'::text),
    CONSTRAINT users_db_user_check CHECK (db_user::text ~ '^u:[a-z][a-z0-9_]*$'::text),
    CONSTRAINT user_email_lower_key UNIQUE (email_lower)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.users
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.users TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.users TO "directory:manager";

-- Table: directory.user_passwords

-- DROP TABLE IF EXISTS directory.user_passwords;

CREATE TABLE IF NOT EXISTS directory.user_passwords
(
    password_digest text COLLATE pg_catalog."default" NOT NULL,
    user_id uuid NOT NULL,
    CONSTRAINT user_passwords_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_passwords_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES directory.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS directory.user_passwords
    OWNER to pg_database_owner;

GRANT ALL ON TABLE directory.user_passwords TO pg_database_owner;
GRANT DELETE, INSERT, SELECT, UPDATE ON TABLE directory.user_passwords TO "directory:manager";
