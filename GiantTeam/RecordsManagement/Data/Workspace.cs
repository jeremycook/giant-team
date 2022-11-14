using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    public class Workspace
    {
        [Key]
        public string WorkspaceId { get; set; } = null!;

        public DateTimeOffset Created { get; set; }

        public Guid OwnerId { get; set; }
        public User? Owner { get; private set; }
    }
}
