using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class FetchWorkspaceService
    {
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;

        public class FetchWorkspaceInput
        {
            [Required]
            [PgLaxIdentifier]
            [StringLength(50, MinimumLength = 3)]
            public string? WorkspaceName { get; set; }
        }

        public class FetchWorkspaceOutput
        {
            public string WorkspaceName { get; set; } = null!;
            public string WorkspaceOwner { get; set; } = null!;
        }

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

            using var connection = await connectionService.OpenMaintenanceConnectionAsync();

            var output = await connection.QuerySingleOrDefaultAsync<FetchWorkspaceOutput>($"""
SELECT
    datname {PgQuote.Identifier(nameof(FetchWorkspaceOutput.WorkspaceName))},
    r.rolname {PgQuote.Identifier(nameof(FetchWorkspaceOutput.WorkspaceOwner))}
FROM pg_database d
JOIN pg_roles r ON r.oid = d.datdba
WHERE datname = @datname
""",
new
{
    datname = input.WorkspaceName,
});

            if (output is null)
            {
                throw new DetailedValidationException($"Workspace not found.");
            }

            return output;
        }
    }
}
