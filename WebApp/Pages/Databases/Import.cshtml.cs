using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;
using WebApp.Helpers;
using WebApp.Postgres;

namespace WebApp.Pages.Databases
{
    public class ImportModel : PageModel
    {
        [FromRoute]
        public string DatabaseName { get; set; } = null!;

        [FromRoute]
        public string SchemaName { get; set; } = null!;

        [FromRoute]
        public string TableName { get; set; } = null!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Display(Name = "Data File")]
            public IFormFile DataFile { get; set; } = null!;
        }

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost([FromServices] ILogger<ImportModel> logger)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using Stream csv = Input.DataFile.OpenReadStream();
                    using StreamReader reader = new(csv, Encoding.UTF8);

                    if (reader.EndOfStream)
                    {
                        ModelState.AddModelError("", "The file is empty.");
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

                        using NpgsqlConnection connection = new($"Server=localhost;Port=5432;Database={DatabaseName};Integrated Security=True;Max Auto Prepare=500;Include Error Detail=True;CommandTimeout=15");
                        await connection.OpenAsync();

                        var schema = await connection.QueryAsync<(string column_name, string data_type, bool is_nullable)>(
@"SELECT
    column_name,
    data_type,
    (CASE is_nullable WHEN 'YES' THEN true ELSE false END) AS is_nullable
from information_schema.columns
where table_catalog = @DatabaseName
and table_schema = @SchemaName
and table_name = @TableName", new { DatabaseName, SchemaName, TableName });

                        var columnMap = schema.ToDictionary(o => o.column_name);

                        string sql =
$@"INSERT INTO {PgQuote.Identifier(SchemaName, TableName)} ({string.Join(",", fieldNames.Select(PgQuote.Identifier))})
SELECT {string.Join(",", fieldNames.Select((name, i) => "p" + i + "::" + columnMap[name].data_type + " AS " + PgQuote.Identifier(name)))} 
FROM unnest({string.Join(",", Enumerable.Range(0, fieldNames.Count).Select(i => "@p" + i))}) as data ({string.Join(",", Enumerable.Range(0, fieldNames.Count).Select(i => "p" + i))})";

                        NpgsqlCommand command = new(sql, connection);
                        for (int i = 0; i < fieldNames.Count; i++)
                        {
                            var name = fieldNames[i];
                            var (_, _, is_nullable) = columnMap[name];

                            string?[] value = records.Select(r => r[i].Length == 0 && is_nullable ? null : r[i]).ToArray();
                            command.Parameters.AddWithValue("p" + i, value);
                        }

                        logger.LogInformation("Execute non-query: {CommandText}", sql);
                        await command.ExecuteNonQueryAsync();

                        return RedirectToPage("Import", new { DatabaseName, SchemaName, TableName });
                    }
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return Page();
        }
    }
}
