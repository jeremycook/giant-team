using GiantTeam.ComponentModel.Services;
using GiantTeam.Linq;
using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Npgsql;
using Npgsql.Schema;
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

        public async Task<FetchRecords> FetchRecordsAsync(FetchRecordsInput input)
        {
            validationService.Validate(input);

            using var connection = await connectionService.OpenConnectionAsync(input.Database);

            var columnSchema = await GetColumnSchemaAsync(input, connection);

            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = BuildSql(input, columnSchema, command.Parameters);

            logger.LogDebug("Fetching records as ({UserId},{UserRole},{LoginRole}) from {Database} with SQL: {Dql}",
                connectionService.User.UserId,
                connectionService.User.DbRole,
                connectionService.User.DbLogin,
                input.Database,
                command.CommandText);

            var output = new FetchRecords();

            if (input.Verbose == true)
            {
                output.Sql = command.CommandText;
            }

            output.Columns.AddRange(columnSchema.Values.Select(o => new FetchRecordsColumn()
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

        private static async Task<IDictionary<string, NpgsqlDbColumn>> GetColumnSchemaAsync(FetchRecordsInput input, NpgsqlConnection openConnection)
        {
            var sb = new StringBuilder();

            using var cmd = new NpgsqlCommand();
            NpgsqlParameterCollection parameters = cmd.Parameters;

            sb.Append(BuildSelect(input.Columns) + "\n");
            sb.Append($"FROM {PgQuote.Identifier(input.Schema, input.Table)}" + "\n");
            sb.Append($"LIMIT @Take OFFSET @Skip;");

            parameters.AddWithValue("Take", input.Take ?? 100);
            parameters.AddWithValue("Skip", input.Skip ?? 0);

            cmd.CommandText = sb.ToString();
            cmd.Connection = openConnection;
            using NpgsqlDataReader rdr = await cmd.ExecuteReaderAsync();
            var columnSchema = await rdr.GetColumnSchemaAsync();

            return columnSchema.ToDictionary(o => o.ColumnName);
        }

        private static string BuildSql(FetchRecordsInput input, IDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
        {
            var sb = new StringBuilder();

            sb.Append(BuildSelect(input.Columns) + "\n");
            sb.Append($"FROM {PgQuote.Identifier(input.Schema, input.Table)}" + "\n");
            sb.Append(BuildWhere(input.Filters, columnSchema, parameters) is string where ? where + "\n" : string.Empty);
            sb.Append(BuildOrderBy(input.Columns) is string orderBy ? orderBy + "\n" : string.Empty);
            sb.Append($"LIMIT @Take OFFSET @Skip;");

            parameters.AddWithValue("Take", input.Take ?? 100);
            parameters.AddWithValue("Skip", input.Skip ?? 0);

            return sb.ToString();
        }

        private static string BuildSelect(IEnumerable<FetchRecordsInputColumn>? columns)
        {
            if (columns?.Any() == true)
            {
                return $"SELECT {columns
                    .Where(c => c.Visible != false)
                    .OrderBy(c => c.Position)
                    .ThenBy(c => c.Name)
                    .Select(c => PgQuote.Identifier(c.Name))
                    .Join(", ")}";
            }
            else
            {
                return $"SELECT *";
            }
        }

        private static string? BuildWhere(List<FetchRecordsInputRangeFilter>? filters, IDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
        {
            if (filters?.Any() == true)
            {
                return $"WHERE {filters
                    .Select(f => BuildWhereFilter(f, columnSchema, parameters))
                    .Join(" AND ")}";
            }
            else
            {
                return null;
            }
        }

        private static string BuildWhereFilter(FetchRecordsInputFilter filter, IDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
        {
            var dataTypeName = columnSchema[filter.Column].DataTypeName;

            switch (filter)
            {
                case FetchRecordsInputRangeFilter range:
                    string sql = $"{PgQuote.Identifier(filter.Column)} BETWEEN @p{parameters.Count}::{dataTypeName} AND @p{parameters.Count + 1}::{dataTypeName}";
                    parameters.AddWithValue($"p{parameters.Count}", range.LowerValue);
                    parameters.AddWithValue($"p{parameters.Count}", range.UpperValue);
                    return sql;

                default:
                    throw new NotImplementedException(filter.GetType().AssemblyQualifiedName);
            }
        }

        private static string? BuildOrderBy(IEnumerable<FetchRecordsInputColumn>? columns)
        {
            if (columns?.Any(c => c.Sort != Sort.Unsorted) == true)
            {
                return $"ORDER BY {columns
                    .Where(c => c.Sort != Sort.Unsorted)
                    .OrderBy(c => c.Position)
                    .ThenBy(c => c.Name)
                    .Select(c => PgQuote.Identifier(c.Name) + (c.Sort == Sort.Desc ? " DESC" : ""))
                    .Join(", ")}";
            }
            else
            {
                return null;
            }
        }
    }
}
