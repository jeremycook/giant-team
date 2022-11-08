using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;
using GiantTeam.DatabaseModel;
using GiantTeam.Services;
using GiantTeam.Postgres;

namespace WebApp.Pages.Data
{
    [Authorize]
    public class ImportModel : PageModel
    {
        private readonly DatabaseConnectionService databaseConnectionService;

        [FromRoute]
        public string WorkspaceId { get; set; } = null!;

        [BindProperty]
        public string? ExistingTable { get; set; }
        public List<SelectListItem> ExistingTableOptions { get; } = new();

        [BindProperty]
        public string? NewTableName { get; set; }

        [BindProperty]
        public List<IFormFile> DataFiles { get; set; } = new();

        public ImportModel(DatabaseConnectionService databaseConnectionService)
        {
            this.databaseConnectionService = databaseConnectionService;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await PopulateOptionsAsync();
            await base.OnPageHandlerExecutionAsync(context, next);
        }

        public void OnGet([FromQuery] string? existingTable = null)
        {
            ExistingTable = existingTable;
        }

        public async Task<ActionResult> OnPost(
            [FromServices] ILogger<ImportModel> logger)
        {
            bool createTable = false;
            string schemaName = string.Empty;
            string tableName = string.Empty;
            if (ExistingTable is not null)
            {
                createTable = false;
                var part = ExistingTable.Split('.');
                schemaName = part[0];
                tableName = part[1];
            }
            else if (NewTableName is not null)
            {
                createTable = true;
                var part = NewTableName.Split('.');
                schemaName = part[0];
                tableName = part[1];
            }
            else
            {
                ModelState.AddModelError("", "Please select an existing table, or enter the name of a new table to create and insert the data into.");
            }

            if (ModelState.IsValid)
            {
                foreach (IFormFile file in DataFiles)
                {
                    try
                    {
                        using Stream csv = file.OpenReadStream();
                        using StreamReader reader = new(csv, Encoding.UTF8);

                        if (reader.EndOfStream)
                        {
                            ModelState.AddModelError("", $"The \"{file.FileName}\" file was empty.");
                        }
                        else
                        {
                            IReadOnlyList<string> fieldNames = await CsvHelper.ParseRecordAsync(reader);

                            List<IReadOnlyList<string>> records = new();
                            while (!reader.EndOfStream)
                            {
                                try
                                {
                                    IReadOnlyList<string>? record = await CsvHelper.ParseRecordAsync(reader);
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
                                    Owner = $"t:{WorkspaceId}:d",
                                };
                                table.Columns.TryAdd("Id", new("Id", "uuid", isNullable: false, defaultValueSql: "gen_random_uuid()", computedColumnSql: null));
                                table.UniqueConstraints.TryAdd($"{tableName}_pkey", new($"{tableName}_pkey", isPrimaryKey: true)
                                {
                                    Columns = { "Id" },
                                });
                                foreach (var fieldName in fieldNames)
                                {
                                    Column column = new(fieldName, "text", isNullable: true, defaultValueSql: null, computedColumnSql: null);
                                    table.Columns.TryAdd(column.Name, column);
                                }

                                Schema schema = new(schemaName)
                                {
                                    Owner = $"t:{WorkspaceId}:d",
                                    Tables =
                                    {
                                        [tableName] = table
                                    },
                                    Privileges =
                                    {
                                        new("ALL", $"t:{WorkspaceId}:d"),
                                        new("USAGE", $"t:{WorkspaceId}:m"),
                                        new("USAGE", $"t:{WorkspaceId}:q"),
                                    },
                                    DefaultPrivileges =
                                    {
                                        new("ALL", "TABLES", $"t:{WorkspaceId}:d"),
                                        new("ALL", "TABLES", $"t:{WorkspaceId}:m"),
                                        new("SELECT", "TABLES", $"t:{WorkspaceId}:q"),

                                        new("ALL", "SEQUENCES", $"t:{WorkspaceId}:d"),
                                        new("SELECT", "SEQUENCES", $"t:{WorkspaceId}:m"),

                                        new("EXECUTE", "FUNCTIONS", $"t:{WorkspaceId}:d"),
                                        new("EXECUTE", "FUNCTIONS", $"t:{WorkspaceId}:m"),
                                    },
                                };

                                Database database = new()
                                {
                                    Schemas =
                                    {
                                        [schemaName] = schema
                                    }
                                };

                                PgDatabaseScripter scripter = new();
                                string migrationScript = scripter.Script(database);

                                logger.LogInformation("Execute migration script: {CommandText}", migrationScript);

                                using NpgsqlConnection designConnection = databaseConnectionService.CreateDesignConnection(WorkspaceId);
                                await designConnection.ExecuteAsync(migrationScript);
                            }

                            Dictionary<string, (string column_name, string data_type, bool is_nullable)> columnMap;
                            using (NpgsqlConnection queryConnection = databaseConnectionService.CreateQueryConnection(WorkspaceId))
                            {
                                var schema = await queryConnection.QueryAsync<(string column_name, string data_type, bool is_nullable)>(
@"SELECT
    column_name,
    data_type,
    (CASE is_nullable WHEN 'YES' THEN true ELSE false END) AS is_nullable
FROM information_schema.columns
WHERE table_catalog = @DatabaseName
AND table_schema = @SchemaName
AND table_name = @TableName",
new
{
    DatabaseName = WorkspaceId,
    SchemaName = schemaName,
    TableName = tableName,
});

                                columnMap = schema.ToDictionary(o => o.column_name);
                            }

                            using (NpgsqlConnection manipulateConnection = databaseConnectionService.CreateManipulateConnection(WorkspaceId))
                            {
                                string insertSql =
$@"INSERT INTO {PgQuote.Identifier(schemaName, tableName)} ({string.Join(",", fieldNames.Select(PgQuote.Identifier))})
SELECT {string.Join(",", fieldNames.Select((name, i) => "p" + i + "::" + columnMap[name].data_type + " AS " + PgQuote.Identifier(name)))} 
FROM unnest({string.Join(",", Enumerable.Range(0, fieldNames.Count).Select(i => "@p" + i))}) as data ({string.Join(",", Enumerable.Range(0, fieldNames.Count).Select(i => "p" + i))})";

                                NpgsqlCommand command = new(insertSql, manipulateConnection);
                                for (int i = 0; i < fieldNames.Count; i++)
                                {
                                    var name = fieldNames[i];
                                    var (_, _, is_nullable) = columnMap[name];

                                    string?[] value = records.Select(r => r[i].Length == 0 && is_nullable ? null : r[i]).ToArray();
                                    command.Parameters.AddWithValue("p" + i, value);
                                }

                                logger.LogInformation("Execute non-query: {CommandText}", insertSql);

                                await manipulateConnection.OpenAsync();
                                await command.ExecuteNonQueryAsync();
                            }

                            return RedirectToPage("Import", new
                            {
                                WorkspaceId,
                                ExistingTable = $"{schemaName}.{tableName}"
                            });
                        }
                    }
                    catch (ValidationException ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }

            return Page();
        }

        private async Task PopulateOptionsAsync()
        {
            NpgsqlConnection queryConnection = databaseConnectionService.CreateQueryConnection(WorkspaceId);

            ExistingTableOptions.AddRange(await queryConnection.QueryAsync<SelectListItem>($@"
SELECT
    TABLE_SCHEMA || '.' || TABLE_NAME AS ""Value"",
    TABLE_SCHEMA || '.' || TABLE_NAME AS ""Text""
FROM INFORMATION_SCHEMA.TABLES
WHERE table_schema NOT IN('information_schema', 'pg_catalog')
ORDER BY 2;
"));
        }
    }
}
