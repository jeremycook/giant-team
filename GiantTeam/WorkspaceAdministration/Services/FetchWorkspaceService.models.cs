using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class FetchWorkspaceInput
    {
        [Required, StringLength(50), PgIdentifier]
        public string? WorkspaceName { get; set; }
    }

    public class FetchWorkspaceOutput
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public FetchWorkspaceSchema[] Schemas { get; set; } = null!;
    }

    public class FetchWorkspaceSchema
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public FetchWorkspaceTable[] Tables { get; set; } = null!;
    }

    public class FetchWorkspaceTable
    {
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
    }
}
