-- Role: anyone
-- DROP ROLE IF EXISTS anyone;

CREATE ROLE anyone WITH
  NOLOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

-- Role: anyuser
-- DROP ROLE IF EXISTS anyuser;

CREATE ROLE anyuser WITH
  NOLOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

GRANT anyone TO anyuser;

-- Role: anyvisitor
-- DROP ROLE IF EXISTS anyvisitor;

CREATE ROLE anyvisitor WITH
  NOLOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

GRANT anyone TO anyvisitor;