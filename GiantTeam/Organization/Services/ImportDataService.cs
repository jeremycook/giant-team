using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseDefinition;
using GiantTeam.DatabaseDefinition.Alterations;
using GiantTeam.DatabaseDefinition.Alterations.Models;
using GiantTeam.DatabaseDefinition.Models;
using GiantTeam.Postgres;
using GiantTeam.Text;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GiantTeam.Organization.Services;

public class ImportDataService
{
    private readonly ILogger<ImportDataService> logger;
    private readonly ValidationService validationService;
    private readonly UserDataFactory userDataFactory;

    public ImportDataService(
        ILogger<ImportDataService> logger,
        ValidationService validationService,
        UserDataFactory connectionService)
    {
        this.logger = logger;
        this.validationService = validationService;
        userDataFactory = connectionService;
    }

    public async Task<ImportDataOutput> ImportDataAsync(ImportDataInput input)
    {
        validationService.Validate(input);

        try
        {
            return await ImportCsvAsync(input);
        }
        catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
        {
            logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            throw new ValidationException($"Database error: {ex.MessageText.TrimEnd('.')}. {ex.Detail}");
        }
    }

    private async Task<ImportDataOutput> ImportCsvAsync(ImportDataInput input)
    {
        string databaseName = input.Database;
        string schemaName = input.Schema!;
        string tableName = input.Table!;
        bool createTable = input.CreateTableIfNotExists == true;

        using Stream csv = new MemoryStream(input.Data!);
        using StreamReader reader = new(csv, Encoding.UTF8);

        if (reader.EndOfStream)
        {
            throw new ValidationException($"The file was empty.");
        }
        else
        {
            var organizationDataService = userDataFactory.NewDataService(input.Database, input.Schema!);

            var fieldNames = await CsvHelper.ParseRecordAsync(reader);

            var records = new List<IReadOnlyList<string>>();
            while (!reader.EndOfStream)
            {
                try
                {
                    var record = await CsvHelper.ParseRecordAsync(reader);
                    records.Add(record);
                }
                catch (Exception ex)
                {
                    throw new ValidationException(ex.Message, ex);
                }
            }

            if (createTable)
            {
                Table table = new(tableName)
                {
                    //Owner = $"t:{WorkspaceId}:d",
                };
                if (fieldNames
                    .Select((fieldName, index) => (fieldName, index))
                    .FirstOrDefault(o => "id".Equals(o.fieldName, StringComparison.InvariantCultureIgnoreCase)) is var (idColumnName, index) &&
                    idColumnName is not null)
                {
                    if (records.Any() && Guid.TryParse(records[0].ElementAt(index) ?? string.Empty, out _))
                    {
                        table.Columns.Add(new(idColumnName, "uuid", isNullable: false, defaultValueSql: "gen_random_uuid()", computedColumnSql: null));
                    }
                    else if (records.Any() && int.TryParse(records[0].ElementAt(index) ?? string.Empty, out _))
                    {
                        // TODO: Support auto-incrementing integer identity column
                        table.Columns.Add(new(idColumnName, "int", isNullable: false, defaultValueSql: null, computedColumnSql: null));
                    }
                    else
                    {
                        table.Columns.Add(new(idColumnName, "text", isNullable: false, defaultValueSql: null, computedColumnSql: null));
                    }
                }
                else
                {
                    idColumnName = "Id";
                    table.Columns.Add(new(idColumnName, "uuid", isNullable: false, defaultValueSql: "gen_random_uuid()", computedColumnSql: null));
                }
                table.Indexes.GetOrAdd(new($"pk_{tableName}", TableIndexType.PrimaryKey)
                {
                    Columns = { idColumnName },
                });
                foreach (var fieldName in fieldNames)
                {
                    table.Columns.GetOrAdd(new(fieldName, "text", isNullable: true, defaultValueSql: null, computedColumnSql: null));
                }

                var diff = TableDiffer.DiffTables(schemaName, new Table(tableName), table);
                diff.Insert(0, new CreateTable(schemaName, tableName, Array.Empty<Column>()));

                PgDatabaseScripter scripter = new();
                string migrationScript = scripter.ScriptAlterations(diff);

                logger.LogInformation("Executing migration script: {CommandText}", migrationScript);

                await organizationDataService.ExecuteUnsanitizedAsync(migrationScript);
            }

            // Get column mapping
            Dictionary<string, (string column_name, string data_type, bool is_nullable)> columnMap;
            {
                var schema = await organizationDataService.ListAsync<(string column_name, string data_type, bool is_nullable)>(Sql.Format($"""
SELECT
    column_name,
    data_type,
    (CASE is_nullable WHEN 'YES' THEN true ELSE false END) AS is_nullable
FROM information_schema.columns
WHERE table_catalog = {databaseName}
AND table_schema = {schemaName}
AND table_name = {tableName}
"""));

                if (!schema.Any())
                {
                    throw new ValidationException($"The {schemaName}.{tableName} table does not exist, or you do not have permission to access it.");
                }

                columnMap = schema.ToDictionary(o => o.column_name);
            }

            // Insert records
            {
                string insertSql = $"""
INSERT INTO {PgQuote.Identifier(schemaName, tableName)} ({string.Join(",", fieldNames.Select(PgQuote.Identifier))})
SELECT {string.Join(",", fieldNames.Select((name, i) => "p" + i + "::" + columnMap[name].data_type + " AS " + PgQuote.Identifier(name)))} 
FROM unnest({string.Join(",", Enumerable.Range(0, fieldNames.Count).Select(i => "@p" + i))}) as data ({string.Join(",", Enumerable.Range(0, fieldNames.Count).Select(i => "p" + i))})
""";

                var command = new NpgsqlBatchCommand();
                command.CommandText = insertSql;
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    var name = fieldNames[i];
                    var (_, _, is_nullable) = columnMap[name];

                    string?[] value = records.Select(r => r[i].Length == 0 && is_nullable ? null : r[i]).ToArray();
                    command.Parameters.AddWithValue("p" + i, value);
                }

                logger.LogInformation("Inserting {Rows} rows into {Database}.{Schema}.{Table}: {CommandText}",
                    records.Count, databaseName, schemaName, tableName, insertSql);

                var rowsAffected = await organizationDataService.ExecuteAsync();

                logger.LogInformation("Inserted {Rows} rows into {Database}.{Schema}.{Table}",
                    rowsAffected, databaseName, schemaName, tableName);
            }

            return new ImportDataOutput()
            {
                Schema = schemaName,
                Table = tableName,
            };
        }
    }
}


public class ImportDataInput
{
    [Required, StringLength(50), PgIdentifier]
    public string Database { get; set; } = null!;

    [Required, StringLength(50), PgIdentifier]
    public string? Schema { get; set; }

    [Required, StringLength(50), PgIdentifier]
    public string? Table { get; set; }

    public bool? CreateTableIfNotExists { get; set; } = false;

    [Required]
    public byte[]? Data { get; set; }
}

public class ImportDataOutput
{
    public string Schema { get; init; } = null!;
    public string Table { get; init; } = null!;
}
