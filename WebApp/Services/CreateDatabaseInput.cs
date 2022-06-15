using System.ComponentModel.DataAnnotations;
using WebApp.Postgres;

namespace WebApp.Services
{
    public class CreateDatabaseInput
    {
        [PgIdentifier]
        [StringLength(25)]
        public string DatabaseName { get; set; } = null!;
    }
}