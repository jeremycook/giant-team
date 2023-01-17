SELECT '-- Review the following lines, and run as a superuser in the directory DB.'

UNION ALL

SELECT format('DROP DATABASE IF EXISTS %I (FORCE);', datname)
FROM pg_database
WHERE datname NOT IN ('postgres', 'dataprotection', 'directory')
AND datname NOT LIKE 'template%'

UNION ALL

SELECT format('DROP ROLE IF EXISTS %I;', rolname)
FROM pg_roles
WHERE rolname LIKE '_:%' 
AND rolname NOT IN ('u:owner', 'u:owner:e', 'u:admin', 'u:admin:e', 'u:member', 'u:member:e')

\gexec

SELECT '-- Review the following lines, and run as a superuser in the directory DB.'

UNION ALL

SELECT format('DELETE FROM directory.organization WHERE organization_id = %L;', organization_id)
FROM directory.organization

UNION ALL

SELECT format('DELETE FROM directory.user_password WHERE user_id = %L; -- %s', user_id, username)
FROM directory.user
WHERE username NOT IN ('owner', 'admin', 'member')

UNION ALL

SELECT format('DELETE FROM directory."user" WHERE user_id = %L; -- %s', user_id, username)
FROM directory.user
WHERE username NOT IN ('owner', 'admin', 'member')

\gexec
