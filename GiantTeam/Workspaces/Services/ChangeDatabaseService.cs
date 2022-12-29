using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseModeling.Changes.Models;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class ChangeDatabaseInput
    {
        [Required, StringLength(50), PgIdentifier]
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
        private readonly UserConnectionService connectionService;

        public ChangeDatabaseService(
            ILogger<ChangeDatabaseService> logger,
            ValidationService validationService,
            UserConnectionService connectionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.connectionService = connectionService;
        }

        public async Task<ChangeDatabaseOutput> ChangeDatabaseAsync(ChangeDatabaseInput input)
        {
            validationService.Validate(input);

            return await ProcessAsync(input);
        }

        private async Task<ChangeDatabaseOutput> ProcessAsync(ChangeDatabaseInput input)
        {
            PgDatabaseScripter scripter = new();
            string migrationScript = scripter.ScriptChanges(input.Changes);

            string dbRole = $"{input.DatabaseName}:Owner";

            logger.LogInformation("Executing change database script as {DatabaseRole}: {CommandText}", dbRole, migrationScript);

            using NpgsqlConnection designConnection = await connectionService.OpenConnectionAsync(input.DatabaseName, dbRole);
            try
            {
                await designConnection.ExecuteAsync(migrationScript);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "{ExceptionType}: {ExceptionMessage} when executing {Sql}",
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
