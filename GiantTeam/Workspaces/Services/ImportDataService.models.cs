using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Workspaces.Services
{
    public class ImportDataInput
    {
        [Required]
        [Identifier]
        [StringLength(50, MinimumLength = 3)]
        public string Database { get; set; } = null!;

        [Required]
        [Identifier]
        [StringLength(100)]
        public string? Schema { get; set; }

        [Required]
        [Identifier]
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
