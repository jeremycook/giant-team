using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
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
}
