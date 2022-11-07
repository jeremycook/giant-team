using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Data
{
    [Index(nameof(UsernameNormalized), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [PgLaxIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        /// <summary>
        /// The lowercase form of <see cref="Username"/>.
        /// </summary>
        [PgStrictIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string UsernameNormalized { get => Username?.ToLowerInvariant()!; private set { } }

        public string PasswordDigest { get; set; } = null!;

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        public bool EmailVerified { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}