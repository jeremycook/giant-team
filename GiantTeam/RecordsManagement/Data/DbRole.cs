using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    public class DbRole
    {
        [Key]
        [PgLaxIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string RoleId { get; set; } = null!;

        public DateTimeOffset Created { get; set; }
    }
}
