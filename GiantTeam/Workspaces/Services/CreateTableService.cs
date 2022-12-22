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

            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"Database error: {ex.MessageText.TrimEnd('.')}. {ex.Detail}");
            }
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
            string migrationScript = scripter.Script(database);

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

                throw;
            }

            return new CreateTable()
            {
            };
        }
    }
}
