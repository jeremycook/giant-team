using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Services
{
    public class CreateWorkspaceInput
    {
        [PgLaxIdentifier]
        [StringLength(25)]
        public string WorkspaceName { get; set; } = null!;
    }
}