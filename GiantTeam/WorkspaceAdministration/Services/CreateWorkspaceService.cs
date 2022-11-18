using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.UserManagement.Services;
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
            [PgLaxIdentifier]
            [StringLength(50, MinimumLength = 3)]
            public string? WorkspaceName { get; set; }

            [Required]
            [PgLaxIdentifier]
            [StringLength(50, MinimumLength = 3)]
            public string? WorkspaceOwner { get; set; }
        }

        public class CreateWorkspaceOutput
        {
            public CreateWorkspaceOutput(CreateWorkspaceStatus status)
            {
                Status = status;
            }

            public CreateWorkspaceStatus Status { get; }

            public string? Message { get; init; }

            public string? WorkspaceName { get; set; }
        }

        public enum CreateWorkspaceStatus
        {
            /// <summary>
            /// Creation was not successful.
            /// Check out <see cref="CreateWorkspaceOutput.Message"/>.
            /// </summary>
            Problem = 400,

            /// <summary>
            /// Created the workspace.
            /// Check out the <see cref="CreateWorkspaceOutput.WorkspaceName"/>.
            /// </summary>
            Success = 200,
        }

        private readonly ILogger<CreateWorkspaceService> logger;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly FetchRoleService fetchTeamService;
        private readonly WorkspaceConnectionService connectionService;

        public CreateWorkspaceService(
            ILogger<CreateWorkspaceService> logger,
            ValidationService validationService,
            SessionService sessionService,
            FetchRoleService fetchTeamService,
            WorkspaceConnectionService connectionService)
        {
            this.connectionService = connectionService;
            this.logger = logger;
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.fetchTeamService = fetchTeamService;
        }

        public async Task<CreateWorkspaceOutput> CreateWorkspaceAsync(CreateWorkspaceInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (ValidationException ex)
            {
                return new(CreateWorkspaceStatus.Problem)
                {
                    Message = ex.Message,
                };
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogInformation(exception, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetType(), ex.Message);
                return new(CreateWorkspaceStatus.Problem)
                {
                    Message = $"The \"{input.WorkspaceName}\" workspace was not created: {ex.MessageText.TrimEnd('.')}. {ex.Detail}",
                };
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

            using var maintenance = await connectionService.OpenMaintenanceConnectionAsync(workspaceOwner);
            try
            {
                await maintenance.ExecuteAsync($"CREATE DATABASE {PgQuote.Identifier(workspaceName)} OWNER {PgQuote.Identifier(workspaceOwner)};");

                using var workspaceDb = await connectionService.OpenConnectionAsync(workspaceName, workspaceOwner);
                await workspaceDb.ExecuteAsync("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
            }
            catch (Exception)
            {
                // Cleanup
                try
                { await maintenance.ExecuteAsync($"DROP DATABASE IF EXISTS {PgQuote.Identifier(workspaceName)};"); }
                catch (Exception) { }

                throw;
            }

            return new(CreateWorkspaceStatus.Success)
            {
                WorkspaceName = workspaceName,
            };
        }
    }
}
