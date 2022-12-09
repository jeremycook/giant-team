-- Create public schema
CREATE SCHEMA IF NOT EXISTS public
    AUTHORIZATION pg_database_owner;

COMMENT ON SCHEMA public
    IS 'standard public schema';

GRANT USAGE ON SCHEMA public TO PUBLIC;

GRANT ALL ON SCHEMA public TO pg_database_owner;

ALTER DEFAULT PRIVILEGES FOR ROLE pg_database_owner IN SCHEMA public
GRANT SELECT ON TABLES TO PUBLIC;

-- Create public.gt_database view
CREATE OR REPLACE VIEW public.gt_database
 AS
 SELECT pg_database.datname AS name,
    pg_database.datdba::regrole::text AS owner,
    pg_database_size(pg_database.datname) AS size
   FROM pg_database
  WHERE has_database_privilege(CURRENT_ROLE, pg_database.datname::text, 'CONNECT'::text) AND pg_database.datname <> current_database();

GRANT ALL ON TABLE public.gt_database TO pg_database_owner;
GRANT SELECT ON TABLE public.gt_database TO PUBLIC;
