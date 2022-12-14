using GiantTeam.ComponentModel;
using GiantTeam.Postgres;
using Npgsql;
using Npgsql.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class FetchRecordsInput
    {
        /// <summary>
        /// Output generated SQL into <see cref="FetchRecordsOutput.Sql"/>.
        /// </summary>
        public bool? Verbose { get; set; } = false;

        [Required]
        [Identifier]
        [StringLength(50, MinimumLength = 3)]
        public string Database { get; set; } = null!;

        [Required]
        [Identifier]
        [StringLength(100)]
        public string Schema { get; set; } = null!;

        [Required]
        [Identifier]
        [StringLength(100)]
        public string Table { get; set; } = null!;

        public List<string>? Columns { get; set; }

        public List<FetchRecordsInputRangeFilter>? Filters { get; set; }

        public List<FetchRecordsInputOrder>? OrderBy { get; set; }

        public int? Skip { get; set; } = 0;

        public int? Take { get; set; } = 1000;

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

    public class FetchRecordsInputColumn
    {
        [StringLength(100)]
        public string Column { get; set; } = null!;
        [StringLength(100)]
        public string? Alias { get; set; }

        public string? ToSql()
        {
            return
                PgQuote.Identifier(Column) +
                (Alias is not null ? " " + PgQuote.Identifier(Alias) : string.Empty);
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
        [Identifier]
        [StringLength(100)]
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
        [Identifier]
        [StringLength(100)]
        public string Column { get; set; } = null!;

        public bool? Desc { get; set; } = false;

        public string ToSql()
        {
            return $"{PgQuote.Identifier(Column)}{(Desc == true ? " DESC" : string.Empty)}";
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

}
