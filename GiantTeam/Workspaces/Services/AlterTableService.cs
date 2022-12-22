using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseModeling.Models;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class AlterTableInput
    {
        [Required, StringLength(50), PgIdentifier]
        public string DatabaseName { get; set; } = null!;

        [Required, StringLength(50), PgIdentifier]
        public string SchemaName { get; set; } = null!;

        [Required]
        public Table Table { get; set; } = null!;
    }

    public class AlterTable
    {
    }

    public class AlterTableService
    {
        private readonly ILogger<AlterTableService> logger;
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;

        public AlterTableService(
            ILogger<AlterTableService> logger,
            ValidationService validationService,
            UserConnectionService connectionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.connectionService = connectionService;
        }

        public async Task<AlterTable> AlterTableAsync(AlterTableInput input)
        {
            validationService.Validate(input);

            return await ProcessAsync(input);
        }

        private async Task<AlterTable> ProcessAsync(AlterTableInput input)
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
            string migrationScript = scripter.ScriptAlterTables(database);

            logger.LogInformation("Executing alter table script: {CommandText}", migrationScript);

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

            return new AlterTable()
            {
            };
        }
    }
}
