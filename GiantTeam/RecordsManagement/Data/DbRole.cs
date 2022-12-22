using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    public class DbRole
    {
        [Key, StringLength(50), PgIdentifier]
        public string RoleId { get; set; } = null!;

        public DateTimeOffset Created { get; set; }
    }
}
