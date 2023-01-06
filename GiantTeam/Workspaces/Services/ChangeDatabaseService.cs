using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseDefinition.Changes.Models;
using GiantTeam.Organizations.Organization.Services;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class ChangeDatabaseInput
    {
        [Required, StringLength(50), DatabaseName]
        public string DatabaseName { get; set; } = null!;

        [Required, MinLength(1)]
        public DatabaseChange[] Changes { get; set; } = null!;
    }

    public class ChangeDatabaseOutput
    {
    }

    public class ChangeDatabaseService
    {
        private readonly ILogger<ChangeDatabaseService> logger;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly UserDataFactory organizationDataFactory;

        public ChangeDatabaseService(
            ILogger<ChangeDatabaseService> logger,
            ValidationService validationService,
            SessionService sessionService,
            UserDataFactory organizationDataFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.organizationDataFactory = organizationDataFactory;
        }

        public async Task<ChangeDatabaseOutput> ChangeDatabaseAsync(ChangeDatabaseInput input)
        {
            return await ProcessAsync(input);
        }

        private async Task<ChangeDatabaseOutput> ProcessAsync(ChangeDatabaseInput input)
        {
            validationService.Validate(input);

            string migrationScript =
                $"SET ROLE pg_database_owner;\n" +
                PgDatabaseScripter.Singleton.ScriptChanges(input.Changes);

            logger.LogInformation("Executing change database script as {UserId}: {CommandText}",
                sessionService.User.UserId,
                migrationScript);

            var dataService = organizationDataFactory.NewDataService(input.DatabaseName);
            try
            {
                await dataService.ExecuteUnsanitizedAsync(migrationScript);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "{ExceptionType}: {ExceptionMessage} when executing {CommandText}",
                    ex.GetBaseException().GetType(),
                    ex.GetBaseException().Message,
                    migrationScript);

                if (ex.GetBaseException() is PostgresException postgresException)
                {
                    string message = $"Database error: {postgresException.MessageText.TrimEnd('.')}.";

                    if (postgresException.Detail is not null)
                    {
                        message += $" Detail: {postgresException.Detail.TrimEnd('.')}.";
                    }

                    if (postgresException.Hint is not null)
                    {
                        message += $" Hint: {postgresException.Hint.TrimEnd('.')}.";
                    }

                    message += $" SQL:\n{migrationScript}";

                    throw new ValidationException(message);
                }

                throw;
            }

            return new ChangeDatabaseOutput()
            {
            };
        }
    }
}
