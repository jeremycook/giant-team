using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class ImportDataInput
    {
        [Required]
        [PgLaxIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string Database { get; set; } = null!;

        [Required]
        [PgLaxIdentifier]
        [StringLength(100)]
        public string? Schema { get; set; }

        [Required]
        [PgLaxIdentifier]
        [StringLength(100)]
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
