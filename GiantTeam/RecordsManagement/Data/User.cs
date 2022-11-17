using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.RecordsManagement.Data
{
    [Index(nameof(InvariantUsername), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [PgLaxIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = null!;
        public string InvariantUsername { get => Username?.ToLowerInvariant()!; private set { } }

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        public bool EmailVerified { get; set; }

        public DateTimeOffset Created { get; set; }

        public string DbRoleId { get; set; } = null!;
        public DbRole? DbRole { get; private set; }

        public List<TeamUser>? Teams { get; set; }
    }
}