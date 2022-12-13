using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseModeling;
using GiantTeam.Postgres;
using System.Data;
using System.Text.Json;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class FetchWorkspaceService
    {
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;


        public FetchWorkspaceService(
            ValidationService validationService,
            UserConnectionService connectionService)
        {
            this.validationService = validationService;
            this.connectionService = connectionService;
        }

        public async Task<FetchWorkspaceOutput> FetchWorkspaceAsync(FetchWorkspaceInput input)
        {
            validationService.Validate(input);

            FetchWorkspaceOutput output;
            using (var connection = await connectionService.OpenInfoConnectionAsync())
            {

                output = await connection.QuerySingleOrDefaultAsync<FetchWorkspaceOutput>($"""
SELECT
    datname {PgQuote.Identifier(nameof(FetchWorkspaceOutput.WorkspaceName))},
    r.rolname {PgQuote.Identifier(nameof(FetchWorkspaceOutput.WorkspaceOwner))}
FROM pg_database d
JOIN pg_roles r ON r.oid = d.datdba
WHERE datname = @datname;
""",
    new
    {
        datname = input.WorkspaceName,
    });
            }

            if (output is null)
            {
                throw new DetailedValidationException($"Workspace not found.");
            }

            using (var connection = await connectionService.OpenConnectionAsync(input.WorkspaceName!))
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"""
WITH tables AS (
    SELECT
        t.tablename "Name",
        t.tableowner "Owner",
        t.schemaname schema_name
    FROM pg_catalog.pg_tables t
    ORDER BY t.tablename
),
schemas AS (
    SELECT
        s.schema_name "Name",
        s.schema_owner "Owner",
        json_agg(t.* ORDER BY t."Name") "Tables"
    FROM information_schema.schemata s
    LEFT JOIN tables t ON t.schema_name::name = s.schema_name::name
    WHERE s.schema_name::name <> ALL (ARRAY['information_schema'::name, 'pg_catalog'::name])
    GROUP BY s.schema_name, s.schema_owner
    ORDER BY s.schema_name, s.schema_owner
)
SELECT to_jsonb(schemas.*)
FROM schemas;
""";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var schema = reader.GetFieldValue<FetchWorkspaceSchema>(0);
                    output.Schemas.Add(schema);
                }
            }

            return output;
        }
    }
}
