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
    public class CreateTeamService
    {
        private readonly ILogger<CreateTeamService> logger;
        private readonly SessionService sessionService;
        private readonly SecurityConnectionService security;
        private readonly ValidationService validationService;

        public class CreateTeamInput
        {
            [Required]
            [StringLength(50), PgIdentifier]
            public string? TeamName { get; set; }
        }

        public class CreateTeamOutput
        {
            public string TeamId { get; set; } = null!;
        }

        public CreateTeamService(
            ILogger<CreateTeamService> logger,
            SessionService sessionService,
            SecurityConnectionService admin,
            ValidationService validationService)
        {
            this.logger = logger;
            this.sessionService = sessionService;
            this.security = admin;
            this.validationService = validationService;
        }

        public async Task<CreateTeamOutput> CreateTeamAsync(CreateTeamInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (ValidationException ex) when (ex is not DetailedValidationException)
            {
                throw new DetailedValidationException(ex);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new DetailedValidationException($"The \"{input.TeamName}\" team was not created: {ex.MessageText.TrimEnd('.')}. {ex.Detail}");
            }
        }

        private async Task<CreateTeamOutput> ProcessAsync(CreateTeamInput input)
        {
            validationService.Validate(input);

            var sessionUser = sessionService.User;
            var teamName = input.TeamName!;

            // Create the team's database role
            // with the session user an ADMIN of it.
            using var connection = await security.OpenConnectionAsync();
            string sql = $"""
CREATE ROLE {PgQuote.Identifier(teamName)} CREATEDB INHERIT ADMIN {PgQuote.Identifier(sessionUser.DbRole)};
""";

            await connection.ExecuteAsync(sql);
            logger.LogInformation("Created database role as \"{UserId}\" with \"{Sql}\"", sessionUser.UserId, sql);

            return new()
            {
                TeamId = teamName,
            };
        }
    }
}
