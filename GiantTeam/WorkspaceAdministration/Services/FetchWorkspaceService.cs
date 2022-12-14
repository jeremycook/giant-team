using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;

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
WITH columns AS (
    SELECT
        c.table_catalog catalog_name,
        c.table_schema schema_name,
        c.table_name,
        c.column_name "Name",
        c.data_type "DataType",
        case when c.is_nullable = 'YES' then true else false end "IsNullable",
        case when c.is_updatable = 'YES' then true else false end "IsUpdatable"
    FROM information_schema.columns c
),
tables AS (
    SELECT
        st.table_catalog catalog_name,
        st.table_schema schema_name,
        st.table_name "Name",
        t.tableowner "Owner",
		st.is_insertable_into "IsInsertableInto",
        COALESCE(c.columns, '[]') "Columns"
    FROM information_schema.tables st
	JOIN pg_catalog.pg_tables t ON st.table_schema = t.schemaname AND st.table_name = t.tablename 
	LEFT JOIN (
		SELECT c.catalog_name, c.schema_name, c.table_name, json_agg(to_jsonb(c) - ARRAY['catalog_name', 'schema_name', 'table_name'] ORDER BY 'Name') columns
		FROM columns c
		GROUP BY c.catalog_name, c.schema_name, c.table_name
	) c ON c.catalog_name = st.table_catalog AND c.schema_name = st.table_schema AND c.table_name = st.table_name
),
schemas AS (
    SELECT
        s.schema_name "Name",
        s.schema_owner "Owner",
        COALESCE(t.tables, '[]') "Tables"
    FROM information_schema.schemata s
    LEFT JOIN (
		SELECT t.catalog_name, t.schema_name, json_agg(to_jsonb(t) - ARRAY['catalog_name','schema_name'] ORDER BY 'Name') tables
		FROM tables t
		GROUP BY t.catalog_name, t.schema_name
	) t ON t.catalog_name = s.catalog_name AND t.schema_name = s.schema_name
    WHERE s.schema_name NOT IN ('information_schema', 'pg_catalog')
)
SELECT CASE WHEN COUNT(schemas) > 0 THEN json_agg(schemas.* ORDER BY "Name") ELSE '[]' END
FROM schemas;
""";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var schemas = reader.GetFieldValue<FetchWorkspaceSchema[]>(0);
                    output.Schemas.AddRange(schemas);
                }
            }

            return output;
        }
    }
}
