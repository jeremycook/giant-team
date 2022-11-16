using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    public class Workspace
    {
        [Key]
        [PgLaxIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string WorkspaceId { get; set; } = null!;

        [StringLength(50, MinimumLength = 3)]
        public string WorkspaceName { get; set; } = null!;

        public Guid OwnerId { get; set; }
        public Team? Owner { get; private set; }

        public bool Recycle { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
