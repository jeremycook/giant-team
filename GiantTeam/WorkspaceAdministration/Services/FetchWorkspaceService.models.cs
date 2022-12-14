using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class FetchWorkspaceInput
    {
        [Required]
        [Identifier]
        [StringLength(50, MinimumLength = 3)]
        public string? WorkspaceName { get; set; }
    }

    public class FetchWorkspaceOutput
    {
        public string WorkspaceName { get; set; } = null!;
        public string WorkspaceOwner { get; set; } = null!;
        public List<FetchWorkspaceSchema> Schemas { get; } = new();
    }

    public class FetchWorkspaceSchema
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public List<FetchWorkspaceTable> Tables { get; set; } = new();
    }

    public class FetchWorkspaceTable
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
    }
}
