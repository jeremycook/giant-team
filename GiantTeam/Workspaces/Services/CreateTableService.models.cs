using GiantTeam.ComponentModel;
using GiantTeam.DatabaseDefinition.Models;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class CreateTableInput
    {
        [Required, StringLength(50), PgIdentifier]
        public string DatabaseName { get; set; } = null!;

        [Required, StringLength(50), PgIdentifier]
        public string SchemaName { get; set; } = null!;

        [Required, StringLength(50), PgIdentifier]
        public string TableName { get; set; } = null!;

        [Required]
        public Column[] Columns { get; set; } = null!;

        [Required]
        public TableIndex[] Indexes { get; set; } = null!;
    }

    public class CreateTableOutput
    {
    }
}
