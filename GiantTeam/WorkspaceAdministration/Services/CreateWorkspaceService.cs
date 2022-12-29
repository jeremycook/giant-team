using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.Text;
using GiantTeam.Workspaces.Resources;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class CreateWorkspaceInput
    {
        [Required]
        [StringLength(50), PgIdentifier]
        public string? WorkspaceName { get; set; }

        public bool IsPublic { get; set; }
    }

    public class CreateWorkspaceOutput
    {
    }

    public class CreateWorkspaceService
    {
        private readonly ILogger<CreateWorkspaceService> logger;
        private readonly ValidationService validationService;
        private readonly SecurityConnectionService securityConnectionService;
        private readonly UserConnectionService connectionService;

        public CreateWorkspaceService(
            ILogger<CreateWorkspaceService> logger,
            ValidationService validationService,
            SecurityConnectionService securityConnectionService,
            UserConnectionService connectionService)
        {
            this.connectionService = connectionService;
            this.logger = logger;
            this.validationService = validationService;
            this.securityConnectionService = securityConnectionService;
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
            string workspaceOwner = $"{workspaceName}:Owner";

            try
            {
                // The workspace owner must be created before connecting to the info database.
                // CREATEDB will be allowed until the workspace database is created.
                using (var securityDb = await securityConnectionService.OpenConnectionAsync())
                    await securityDb.ExecuteAsync($"CREATE ROLE {PgQuote.Identifier(workspaceOwner)} WITH CREATEDB INHERIT NOLOGIN NOSUPERUSER NOCREATEROLE NOREPLICATION ROLE {PgQuote.Identifier(connectionService.User.DbRole)};");

                // Create the database
                using (var infoDb = await connectionService.OpenInfoConnectionAsync(workspaceOwner))
                    await infoDb.ExecuteAsync($"CREATE DATABASE {PgQuote.Identifier(workspaceName)};");

                // Connect to the info database as the new workspace owner.
                // Initialize the new workspace database.
                var sb = new StringBuilder();
                sb.AppendLF($"GRANT ALL ON DATABASE {PgQuote.Identifier(workspaceName)} TO pg_database_owner;");
                sb.AppendLF();
                sb.AppendLF($"REVOKE ALL ON DATABASE {PgQuote.Identifier(workspaceName)} FROM public;");
                if (input.IsPublic)
                {
                    sb.AppendLF($"GRANT CONNECT ON DATABASE {PgQuote.Identifier(workspaceName)} TO public;");
                }
                sb.AppendLF();
                sb.AppendLF($"SET ROLE pg_database_owner;");
                sb.AppendLF();
                sb.AppendLF($"DROP SCHEMA IF EXISTS public CASCADE;");
                sb.AppendLF();
                sb.AppendLF(WorkspaceResources.WsSql);

                using (var workspaceDb = await connectionService.OpenConnectionAsync(workspaceName))
                    await workspaceDb.ExecuteAsync(sb.ToString());
            }
            catch (Exception)
            {
                try
                {
                    using (var infoDb = await connectionService.OpenInfoConnectionAsync(workspaceOwner))
                        await infoDb.ExecuteAsync($"DROP DATABASE IF EXISTS {PgQuote.Identifier(workspaceName)};");

                    using (var securityDb = await securityConnectionService.OpenConnectionAsync())
                        await securityDb.ExecuteAsync($"DROP ROLE IF EXISTS {PgQuote.Identifier(workspaceOwner)};");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Suppressed cleanup failure {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                }

                throw;
            }

            // Remove the CREATEDB privilege.
            using (var securityDb = await securityConnectionService.OpenConnectionAsync())
                await securityDb.ExecuteAsync($"ALTER ROLE {PgQuote.Identifier(workspaceOwner)} NOCREATEDB;");

            return new()
            {
            };
        }
    }
}
