using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using GiantTeam.Workspaces.Resources;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class CreateWorkspaceService
    {
        public class CreateWorkspaceInput
        {
            [Required]
            [StringLength(50), PgIdentifier]
            public string? WorkspaceName { get; set; }

            [Required]
            [StringLength(50), PgIdentifier]
            public string? WorkspaceOwner { get; set; }
        }

        public class CreateWorkspaceOutput
        {
            public string WorkspaceName { get; set; } = null!;
        }

        private readonly ILogger<CreateWorkspaceService> logger;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly FetchRoleService fetchRoleService;
        private readonly UserConnectionService connectionService;

        public CreateWorkspaceService(
            ILogger<CreateWorkspaceService> logger,
            ValidationService validationService,
            SessionService sessionService,
            FetchRoleService fetchTeamService,
            UserConnectionService connectionService)
        {
            this.connectionService = connectionService;
            this.logger = logger;
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.fetchRoleService = fetchTeamService;
        }

        public async Task<CreateWorkspaceOutput> CreateWorkspaceAsync(CreateWorkspaceInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The \"{input.WorkspaceName}\" workspace was not created: {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<CreateWorkspaceOutput> ProcessAsync(CreateWorkspaceInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            validationService.Validate(input);

            string workspaceName = input.WorkspaceName!;
            string workspaceOwner = input.WorkspaceOwner!;

            var role = await fetchRoleService.FetchRoleAsync(new()
            {
                RoleName = workspaceOwner,
            });

            if (!role.Inherit || role.CanLogin)
            {
                throw new ValidationException("The workspace owner must be a team role. It appears to be a user role.");
            }

            using var infoDb = await connectionService.OpenInfoConnectionAsync(workspaceOwner);
            try
            {
                await infoDb.ExecuteAsync($"CREATE DATABASE {PgQuote.Identifier(workspaceName)};");

                using var workspaceDb = await connectionService.OpenConnectionAsync(workspaceName, workspaceOwner);

                await workspaceDb.ExecuteAsync($"""
GRANT ALL ON DATABASE {PgQuote.Identifier(workspaceName)} TO pg_database_owner;
REVOKE ALL ON DATABASE {PgQuote.Identifier(workspaceName)} FROM PUBLIC;

DROP SCHEMA IF EXISTS public CASCADE;

CREATE SCHEMA IF NOT EXISTS {PgQuote.Identifier(workspaceName)};
GRANT ALL PRIVILEGES ON SCHEMA {PgQuote.Identifier(workspaceName)} TO pg_database_owner;

{WorkspaceResources.GtPgsql}
""");
            }
            catch (Exception)
            {
                // Cleanup
                try
                {
                    await infoDb.ExecuteAsync($"DROP DATABASE IF EXISTS {PgQuote.Identifier(workspaceName)};");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                }

                throw;
            }

            return new()
            {
                WorkspaceName = workspaceName,
            };
        }
    }
}
