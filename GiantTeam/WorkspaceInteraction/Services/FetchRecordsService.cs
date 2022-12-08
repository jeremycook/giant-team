using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using Npgsql;
using Npgsql.Schema;
using NpgsqlTypes;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class FetchRecordsService
    {
        private readonly ILogger<FetchRecordsService> logger;
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;

        public class FetchRecordsInput
        {
            /// <summary>
            /// Output generated SQL into <see cref="FetchRecordsOutput.Sql"/>.
            /// </summary>
            public bool Verbose { get; set; } = false;

            [Required]
            [PgLaxIdentifier]
            [StringLength(50, MinimumLength = 3)]
            public string? Database { get; set; }

            [Required]
            [PgLaxIdentifier]
            [StringLength(100)]
            public string? Schema { get; set; }

            [Required]
            [PgLaxIdentifier]
            [StringLength(100)]
            public string? Table { get; set; }

            public List<string>? Columns { get; set; }

            public List<FetchRecordsInputRangeFilter>? Filters { get; set; }

            public List<FetchRecordsInputOrder>? OrderBy { get; set; }

            public int Skip { get; set; } = 0;

            public int Take { get; set; } = 100;

            public async Task<IDictionary<string, NpgsqlDbColumn>> GetColumnSchemaAsync(NpgsqlConnection openConnection)
            {
                var sb = new StringBuilder();

                if (Columns?.Any() == true)
                {
                    sb.AppendLine($"SELECT {string.Join(", ", Columns.Select(PgQuote.Identifier))}");
                }
                else
                {
                    sb.AppendLine($"SELECT *");
                }

                sb.AppendLine($"FROM {PgQuote.Identifier(Schema)}.{PgQuote.Identifier(Table)}");

                sb.AppendLine($"LIMIT 1 OFFSET 0;");

                using var cmd = new NpgsqlCommand(sb.ToString(), openConnection);
                using NpgsqlDataReader rdr = await cmd.ExecuteReaderAsync();
                var columnSchema = await rdr.GetColumnSchemaAsync();

                return columnSchema.ToDictionary(o => o.ColumnName);
            }

            public string ToSql(IDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
            {
                var sb = new StringBuilder();

                if (Columns?.Any() == true)
                {
                    sb.AppendLine($"SELECT {string.Join(", ", Columns.Select(PgQuote.Identifier))}");
                }
                else
                {
                    sb.AppendLine($"SELECT *");
                }

                sb.AppendLine($"FROM {PgQuote.Identifier(Schema)}.{PgQuote.Identifier(Table)}");

                if (Filters?.Any() == true)
                {
                    sb.AppendLine($"WHERE {string.Join(" AND ", Filters.Select(f => f.ToSql(columnSchema, parameters)))}");
                }

                if (OrderBy?.Any() == true)
                {
                    sb.AppendLine($"ORDER BY {string.Join(", ", OrderBy.Select(o => o.ToSql()))}");
                }
                else
                {
                    sb.AppendLine("ORDER BY 1");
                }

                sb.AppendLine($"LIMIT {Take} OFFSET {Skip};");

                return sb.ToString();
            }
        }

        public abstract class FetchRecordsInputFilter
        {
            public FetchRecordsInputFilter(string discriminator)
            {
                Discriminator = discriminator;
            }

            public string Discriminator { get; set; }

            [Required]
            [PgLaxIdentifier]
            [StringLength(100, MinimumLength = 3)]
            public string Column { get; set; } = null!;

            public abstract string ToSql(IDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters);
        }

        public class FetchRecordsInputRangeFilter : FetchRecordsInputFilter
        {
            public FetchRecordsInputRangeFilter() : base(nameof(FetchRecordsInputRangeFilter))
            {
            }

            [Required]
            public string LowerValue { get; set; } = null!;

            [Required]
            public string UpperValue { get; set; } = null!;

            public override string ToSql(IDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
            {
                var dataTypeName = columnSchema[Column].DataTypeName;

                string sql = $"{PgQuote.Identifier(Column)} BETWEEN @p{parameters.Count}::{dataTypeName} AND @p{parameters.Count + 1}::{dataTypeName}";
                parameters.AddWithValue($"p{parameters.Count}", LowerValue);
                parameters.AddWithValue($"p{parameters.Count}", UpperValue);
                return sql;
            }
        }

        public class FetchRecordsInputOrder
        {
            [Required]
            [PgLaxIdentifier]
            [StringLength(100, MinimumLength = 3)]
            public string Column { get; set; } = null!;

            public bool Asc { get; set; } = true;

            public string ToSql()
            {
                return $"{PgQuote.Identifier(Column)}{(Asc ? string.Empty : " DESC")}";
            }
        }

        public class FetchRecordsOutput
        {
            public string? Sql { get; set; }
            public List<FetchRecordsOutputColumn> Columns { get; } = new();
            public List<object?[]> Records { get; } = new();
        }

        public class FetchRecordsOutputColumn
        {
            public string Name { get; set; } = null!;
            public string DataType { get; set; } = null!;
            public bool Nullable { get; set; }
        }

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

            if (input.Verbose)
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
