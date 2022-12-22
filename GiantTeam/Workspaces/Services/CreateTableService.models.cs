using GiantTeam.ComponentModel;
using GiantTeam.DatabaseModeling.Models;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class CreateTableInput
    {
        [Required, StringLength(50), PgIdentifier]
        public string DatabaseName { get; set; } = null!;

        [Required, StringLength(50), PgIdentifier]
        public string SchemaName { get; set; } = null!;

        [Required]
        public Table Table { get; set; } = null!;
    }

    public class CreateTable
    {
    }
}
