DELETE FROM rm."Workspaces" WHERE "WorkspaceName" ILIKE '%test%';
DELETE FROM rm."Teams" WHERE "Name" ILIKE '%test%';

-- DROP SCHEMA rm CASCADE;
