using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    [Index(nameof(DbRoleId), IsUnique = true)]
    public class Team
    {
        [Key]
        public Guid TeamId { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        public DateTimeOffset Created { get; set; }

        public string DbRoleId { get; set; } = null!;
        public DbRole? DbRole { get; private set; }

        public List<TeamUser>? Users { get; set; }
    }
}