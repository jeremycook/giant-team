using Dapper;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseModeling.Changes;
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

        [Required, StringLength(50), PgIdentifier]
        public string TableName { get; set; } = null!;

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
        private readonly FetchWorkspaceService fetchWorkspaceService;

        public AlterTableService(
            ILogger<AlterTableService> logger,
            ValidationService validationService,
            UserConnectionService connectionService,
            FetchWorkspaceService fetchWorkspaceService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.connectionService = connectionService;
            this.fetchWorkspaceService = fetchWorkspaceService;
        }

        public async Task<AlterTable> AlterTableAsync(AlterTableInput input)
        {
            validationService.Validate(input);

            return await ProcessAsync(input);
        }

        private async Task<AlterTable> ProcessAsync(AlterTableInput input)
        {
            var workspace = await fetchWorkspaceService.FetchWorkspaceAsync(new()
            {
                WorkspaceName = input.DatabaseName,
            });

            Table? table = workspace
                .Schemas.SingleOrDefault(o => o.Name == input.SchemaName)
                ?.Tables.SingleOrDefault(o => o.Name == input.TableName);

            if (table is null)
            {
                throw new NotFoundException();
            }

            List<DatabaseChange> changes = DatabaseChangeCalculator.CalculateTableChanges(input.SchemaName, table, input.Table);

            string migrationScript = PgDatabaseScripter.Singleton.ScriptChanges(changes);

            if (migrationScript != string.Empty)
            {
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
                        string message = $"Database Error: {postgresException.MessageText.TrimEnd('.')}.";

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
            }

            return new AlterTable()
            {
            };
        }
    }
}
