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

namespace WebApp.Pages.Databases
{
    [Authorize]
    public class ChartModel : PageModel
    {
        [FromRoute]
        public string DatabaseName { get; set; } = null!;

        [FromRoute]
        public string SchemaName { get; set; } = null!;

        [FromRoute]
        public string TableName { get; set; } = null!;

        public DataModel Data { get; } = new();

        public class DataModel
        {
            public List<object> Labels { get; } = new();
            public List<ChartDataset> Datasets { get; } = new();
        }

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

                ReadOnlyCollection<NpgsqlDbColumn> schema = await reader.GetColumnSchemaAsync();

                IEnumerable<string> colors = ColorMaker.HslRange(schema.Count - 1);

                Data.Datasets.AddRange(schema
                    .Skip(1)
                    .Zip(colors)
                    .Select(o => new ChartDataset()
                    {
                        Label = o.First.ColumnName,
                        BackgroundColor = o.Second,
                        BorderColor = o.Second,
                    })
                );

                while (await reader.ReadAsync())
                {
                    Data.Labels.Add(reader[0]);

                    // Skip the first field since it is the label and not data.
                    int field = 1;
                    foreach (ChartDataset dataset in Data.Datasets)
                    {
                        object? value = reader[field];
                        dataset.Data.Add(value);
                        field++;
                    }
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
