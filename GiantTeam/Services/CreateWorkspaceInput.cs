using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Services
{
    public class CreateWorkspaceInput
    {
        [PgLaxIdentifier]
        [StringLength(25)]
        public string WorkspaceName { get; set; } = null!;
    }
}