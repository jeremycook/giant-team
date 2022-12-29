using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.UserManagement.Services
{
    public class CreateWorkspaceRoleInput
    {
        [Required]
        [StringLength(50), PgIdentifier]
        public string? WorkspaceName { get; set; }

        [Required]
        [StringLength(50), PgIdentifier]
        public string? RoleName { get; set; }
    }

    public class CreateWorkspaceRoleOutput
    {
    }

    public class CreateWorkspaceRoleService
    {
        private readonly ILogger<CreateWorkspaceRoleService> logger;
        private readonly SecurityConnectionService securityConnectionService;
        private readonly ValidationService validationService;

        public CreateWorkspaceRoleService(
            ILogger<CreateWorkspaceRoleService> logger,
            SecurityConnectionService securityConnectionService,
            ValidationService validationService)
        {
            this.logger = logger;
            this.securityConnectionService = securityConnectionService;
            this.validationService = validationService;
        }

        public async Task<CreateWorkspaceRoleOutput> CreateRoleAsync(CreateWorkspaceRoleInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogError(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The \"{input.RoleName}\" role was not created: {ex.MessageText.TrimEnd('.')}. {ex.Detail}");
            }
        }

        private async Task<CreateWorkspaceRoleOutput> ProcessAsync(CreateWorkspaceRoleInput input)
        {
            validationService.Validate(input);

            using var securityConnection = await securityConnectionService.OpenConnectionAsync();
            string sql = $"CREATE ROLE {PgQuote.Identifier(input.WorkspaceName! + ":" + input.RoleName!)} WITH NOLOGIN NOSUPERUSER NOINHERIT NOCREATEDB NOCREATEROLE NOREPLICATION ROLE {PgQuote.Identifier(input.WorkspaceName! + ":Owner")};";

            await securityConnection.ExecuteAsync(sql);
            logger.LogInformation("Created workspace role with {Sql}", sql);

            return new()
            {
            };
        }
    }
}
