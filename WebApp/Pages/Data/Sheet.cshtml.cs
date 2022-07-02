using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using Npgsql.Schema;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using WebApp.Charting;
using WebApp.Postgres;
using WebApp.Services;

namespace WebApp.Pages.Data
{
    [Authorize]
    public class SheetModel : PageModel
    {
        [FromRoute]
        public string DatabaseName { get; set; } = null!;

        [FromRoute]
        public string SchemaName { get; set; } = null!;

        [FromRoute]
        public string TableName { get; set; } = null!;

        public List<NpgsqlDbColumn> Columns { get; } = new();

        public List<object[]> Records { get; } = new();

        public async Task<ActionResult> OnGet(
            [FromServices] UserDatabaseConnectionService databaseConnectionService)
        {
            try
            {
                using NpgsqlConnection connection = databaseConnectionService.CreateConnection(DatabaseName);
                await connection.OpenAsync();

                string select = $@"SELECT * FROM {PgQuote.Identifier(SchemaName, TableName)}";

                using NpgsqlCommand command = new(select, connection);
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                Columns.AddRange(await reader.GetColumnSchemaAsync());

                while (await reader.ReadAsync())
                {
                    int i = -1;
                    var record = new object[Columns.Count];
                    foreach (var column in Columns)
                    {
                        i++;
                        object? value = reader[i];
                        record[i] = value;
                    }
                    Records.Add(record);
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return Page();
        }
    }
}
