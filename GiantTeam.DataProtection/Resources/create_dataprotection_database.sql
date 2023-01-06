-- Role: dataprotection_owner
-- DROP ROLE IF EXISTS dataprotection_owner;

CREATE ROLE dataprotection_owner WITH
  NOLOGIN
  NOSUPERUSER
  NOINHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

-- Role: dataprotection_manager
-- DROP ROLE IF EXISTS dataprotection_manager;

CREATE ROLE dataprotection_manager WITH
  LOGIN
  NOSUPERUSER
  NOINHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

-- Database: dataprotection
-- DROP DATABASE IF EXISTS dataprotection;

CREATE DATABASE dataprotection
    WITH
    OWNER = dataprotection_owner
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

GRANT ALL ON DATABASE dataprotection TO dataprotection_owner;
GRANT ALL ON DATABASE dataprotection TO pg_database_owner;
GRANT TEMPORARY, CONNECT ON DATABASE dataprotection TO dataprotection_manager;
REVOKE ALL ON DATABASE dataprotection FROM PUBLIC;