SELECT format('DROP DATABASE IF EXISTS %I (FORCE);', datname)
FROM pg_database
WHERE datname NOT IN ('postgres', 'directory')
AND datname NOT LIKE 'template%'

UNION ALL

SELECT format('DROP ROLE IF EXISTS %I;', rolname)
FROM pg_roles
WHERE rolname LIKE '_:%'

-- \gexec
