-- Role: directory_owner
-- DROP ROLE IF EXISTS directory_owner;

CREATE ROLE directory_owner WITH
  NOLOGIN
  NOSUPERUSER
  NOINHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

-- Role: directory_manager
-- DROP ROLE IF EXISTS directory_manager;

CREATE ROLE directory_manager WITH
  LOGIN
  NOSUPERUSER
  NOINHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

-- Database: directory
-- DROP DATABASE IF EXISTS directory;

CREATE DATABASE directory WITH OWNER directory_owner;
