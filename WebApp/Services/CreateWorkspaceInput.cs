using System.ComponentModel.DataAnnotations;
using WebApp.Postgres;

namespace WebApp.Services
{
    public class CreateWorkspaceInput
    {
        [PgLaxIdentifier]
        [StringLength(25)]
        public string WorkspaceName { get; set; } = null!;
    }
}