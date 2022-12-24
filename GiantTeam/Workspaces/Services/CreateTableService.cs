using Dapper;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseModeling.Models;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class CreateTableService
    {
        private readonly ILogger<CreateTableService> logger;
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;

        public CreateTableService(
            ILogger<CreateTableService> logger,
            ValidationService validationService,
            UserConnectionService connectionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.connectionService = connectionService;
        }

        public async Task<CreateTable> CreateTableAsync(CreateTableInput input)
        {
            validationService.Validate(input);

            return await ProcessAsync(input);
        }

        private async Task<CreateTable> ProcessAsync(CreateTableInput input)
        {
            Database database = new()
            {
                Schemas =
                {
                    new(input.SchemaName)
                    {
                        Tables =
                        {
                            input.Table
                        },
                    }
                },
            };

            PgDatabaseScripter scripter = new();
            string migrationScript = scripter.ScriptCreateTables(database);

            logger.LogInformation("Executing table creation script: {CommandText}", migrationScript);

            using NpgsqlConnection designConnection = await connectionService.OpenConnectionAsync(input.DatabaseName);
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

            return new CreateTable()
            {
            };
        }
    }
}
