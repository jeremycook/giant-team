﻿using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Linq;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using GiantTeam.UserManagement.Services;
using Npgsql;
using Npgsql.Schema;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace GiantTeam.Organization.Services;

public class FetchRecordsService
{
    private readonly ILogger<FetchRecordsService> logger;
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataFactory;
    private readonly SessionService sessionService;

    public FetchRecordsService(
        ILogger<FetchRecordsService> logger,
        ValidationService validationService,
        UserDataServiceFactory userDataFactory,
        SessionService sessionService)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.userDataFactory = userDataFactory;
        this.sessionService = sessionService;
    }

    public async Task<FetchRecords> FetchRecordsAsync(FetchRecordsInput input)
    {
        validationService.Validate(input);

        var dataService = userDataFactory.NewDataService(input.OrganizationId, input.Schema);
        await using var dataSource = dataService.AcquireDataSource();
        await using var connection = await dataSource.OpenConnectionAsync();

        var (columnSchema, selectedColumns) = await GetColumnSchemaAsync(input, connection);

        using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = BuildSql(input, columnSchema, command.Parameters);

        logger.LogDebug("Fetching records as ({UserId},{UserRole},{LoginRole}) from {Database} with SQL: {Dql}",
            sessionService.User.UserId,
            sessionService.User.DbUser,
            sessionService.User.DbLogin,
            input.OrganizationId,
            command.CommandText);

        var output = new FetchRecords();

        if (input.Verbose == true)
        {
            output.Sql = command.CommandText;
        }

        output.Columns.AddRange(selectedColumns
            .Select(o => new FetchRecordsColumn()
            {
                Name = o.ColumnName,
                DataType = o.DataTypeName,
                Nullable = o.AllowDBNull == true,
            }));

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            object?[] record = new object?[selectedColumns.Count];
            for (int i = 0; i < selectedColumns.Count; i++)
            {
                object? value = reader[i];
                record[i] = value == DBNull.Value ? null : value;
            }
            output.Records.Add(record);
        }

        return output;
    }

    private static async Task<(IReadOnlyDictionary<string, NpgsqlDbColumn> columnSchema, IReadOnlyCollection<NpgsqlDbColumn> selectedColumns)> GetColumnSchemaAsync(FetchRecordsInput input, NpgsqlConnection openConnection)
    {
        var sb = new StringBuilder();

        using var cmd = new NpgsqlCommand();
        NpgsqlParameterCollection parameters = cmd.Parameters;

        var filteredColumns = input.Filters?.Select(f => f.Column).ToImmutableSortedSet() ?? ImmutableSortedSet.Create<string>();
        string select = "SELECT " + (input.Columns?.Any() == true ?
            input.Columns
               .Where(c => c.IsVisible() || filteredColumns.Contains(c.Name))
               .Select(c => PgQuote.Identifier(c.Name))
               .Join(", ") :
            "*");

        sb.Append($"""
{select}
FROM {PgQuote.Identifier(input.Schema, input.Table)}
LIMIT @Take OFFSET @Skip;
""");

        parameters.AddWithValue("Skip", 0);
        parameters.AddWithValue("Take", 1);

        cmd.CommandText = sb.ToString();
        cmd.Connection = openConnection;
        using NpgsqlDataReader rdr = await cmd.ExecuteReaderAsync();
        var columnSchema = await rdr.GetColumnSchemaAsync();

        var dictionary = columnSchema.ToImmutableDictionary(o => o.ColumnName);
        var selectedColumns = input.Columns?.Any() == true ?
            input.Columns
                .Where(o => o.IsVisible())
                .OrderBy(o => o.Position)
                .ThenBy(o => o.Name)
                .Select(o => dictionary[o.Name]) :
            dictionary.Values
                .OrderBy(o => o.ColumnOrdinal)
                .ThenBy(o => o.ColumnName);
        return (dictionary, selectedColumns.ToImmutableArray());
    }

    private static string BuildSql(FetchRecordsInput input, IReadOnlyDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
    {
        var sb = new StringBuilder();

        sb.Append(BuildSelect(input.Columns) + "\n");
        sb.Append($"FROM {PgQuote.Identifier(input.Schema, input.Table)}" + "\n");
        sb.Append(BuildWhere(input.Filters, columnSchema, parameters) is string where ? where + "\n" : string.Empty);
        sb.Append(BuildOrderBy(input.Columns) is string orderBy ? orderBy + "\n" : string.Empty);
        sb.Append($"LIMIT @Take OFFSET @Skip;");

        parameters.AddWithValue("Skip", input.Skip ?? 0);
        parameters.AddWithValue("Take", input.Take ?? 100);

        return sb.ToString();
    }

    private static string BuildSelect(IEnumerable<FetchRecordsInputColumn>? columns)
    {
        if (columns?.Any() == true)
        {
            return $"SELECT {columns
                .Where(c => c.IsVisible())
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

    private static string? BuildWhere(List<FetchRecordsInputRangeFilter>? filters, IReadOnlyDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
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

    private static string BuildWhereFilter(FetchRecordsInputFilter filter, IReadOnlyDictionary<string, NpgsqlDbColumn> columnSchema, NpgsqlParameterCollection parameters)
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

public class FetchRecordsInput
{
    /// <summary>
    /// Output generated SQL into <see cref="FetchRecords.Sql"/>.
    /// </summary>
    public bool? Verbose { get; set; } = false;

    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required, StringLength(50), PgIdentifier]
    public string Schema { get; set; } = null!;

    [Required, StringLength(50), PgIdentifier]
    public string Table { get; set; } = null!;

    public List<FetchRecordsInputColumn>? Columns { get; set; }

    public List<FetchRecordsInputRangeFilter>? Filters { get; set; }

    public int? Skip { get; set; } = 0;

    [Range(1, 10000)]
    public int? Take { get; set; } = 100;
}

public class FetchRecordsInputColumn
{
    [Required, StringLength(50), PgIdentifier]
    public string Name { get; set; } = null!;
    public int? Position { get; set; }
    public Sort Sort { get; set; }
    public bool? Visible { get; set; }

    public bool IsVisible() => Visible != false;
}

[JsonDerivedType(typeof(FetchRecordsInputRangeFilter), nameof(FetchRecordsInputRangeFilter))]
public abstract class FetchRecordsInputFilter
{
    protected FetchRecordsInputFilter(string type)
    {
        Type = type;
    }

    [JsonPropertyName("$type"), JsonPropertyOrder(-1)]
    public string Type { get; }

    [Required, StringLength(50), PgIdentifier]
    public string Column { get; set; } = null!;
}

public class FetchRecordsInputRangeFilter : FetchRecordsInputFilter
{
    public FetchRecordsInputRangeFilter()
        : base(nameof(FetchRecordsInputRangeFilter))
    {
    }

    [Required(AllowEmptyStrings = true)]
    public string LowerValue { get; set; } = null!;

    [Required]
    public string UpperValue { get; set; } = null!;
}

public class FetchRecordsInputOrder
{
    [Required, StringLength(50), PgIdentifier]
    public string Column { get; set; } = null!;

    public bool? Desc { get; set; } = false;

    public string ToSql()
    {
        return $"{PgQuote.Identifier(Column)}{(Desc == true ? " DESC" : string.Empty)}";
    }
}

public class FetchRecords
{
    public string? Sql { get; set; }
    public List<FetchRecordsColumn> Columns { get; } = new();
    public List<object?[]> Records { get; } = new();
}

public class FetchRecordsColumn
{
    [Required, StringLength(50), PgIdentifier]
    public string Name { get; set; } = null!;
    public string DataType { get; set; } = null!;
    public bool Nullable { get; set; }
}
