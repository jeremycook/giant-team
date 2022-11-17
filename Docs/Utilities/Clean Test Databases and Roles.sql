SELECT format('DROP DATABASE IF EXISTS %I;', datname)
FROM pg_database
WHERE datname ILIKE '%test%'

UNION ALL

SELECT format('DROP ROLE IF EXISTS %I;', rolname)
FROM pg_roles
WHERE rolname ILIKE '%test%'
-- AND rolname <> 'test-user'

-- \gexec
