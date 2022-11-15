using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    public class Workspace
    {
        private string _workspaceId = null!;

        [Key]
        public string WorkspaceId { get => _workspaceId; set => _workspaceId = value?.ToLowerInvariant()!; }

        public string? WorkspaceName { get; set; }

        public bool Recycle { get; set; }

        public DateTimeOffset Created { get; set; }

        public Guid OwnerId { get; set; }
        public User? Owner { get; private set; }
    }
}
