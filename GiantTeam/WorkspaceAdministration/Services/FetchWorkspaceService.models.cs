using GiantTeam.ComponentModel;
using GiantTeam.DatabaseModeling.Models;
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
        public Schema[] Schemas { get; set; } = null!;
    }
}
