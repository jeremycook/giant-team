using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Npgsql;
using Npgsql.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class FetchRecordsService
    {
        private readonly ILogger<FetchRecordsService> logger;
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;

        public FetchRecordsService(
            ILogger<FetchRecordsService> logger,
            ValidationService validationService,
            UserConnectionService connectionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.connectionService = connectionService;
        }

        public async Task<FetchRecordsOutput> FetchRecordsAsync(FetchRecordsInput input)
        {
            validationService.Validate(input);

            using var connection = await connectionService.OpenConnectionAsync(input.Database);

            var columnSchema = await input.GetColumnSchemaAsync(connection);

            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = input.ToSql(columnSchema, command.Parameters);

            logger.LogDebug("Fetching records as ({UserId},{UserRole},{LoginRole}) from {Database} with SQL: {Dql}",
                connectionService.User.UserId,
                connectionService.User.DbRole,
                connectionService.User.DbLogin,
                input.Database,
                command.CommandText);

            var output = new FetchRecordsOutput();

            if (input.Verbose == true)
            {
                output.Sql = command.CommandText;
            }

            output.Columns.AddRange(columnSchema.Values.Select(o => new FetchRecordsOutputColumn()
            {
                Name = o.ColumnName,
                DataType = o.DataTypeName,
                Nullable = o.AllowDBNull == true,
            }));

            using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                object?[] record = new object?[columnSchema.Count];
                for (int i = 0; i < columnSchema.Count; i++)
                {
                    object? value = reader[i];
                    record[i] = value;
                }
                output.Records.Add(record);
            }

            return output;
        }
    }
}
