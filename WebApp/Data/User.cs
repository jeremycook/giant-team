using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApp.Postgres;

namespace WebApp.Data
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(50, MinimumLength = 3)]
        public string DisplayUsername { get; set; } = null!;

        /// <summary>
        /// The lowercase form of <see cref="DisplayUsername"/>.
        /// </summary>
        [PgIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get => DisplayUsername?.ToLower()!; private set { } }

        public string PasswordDigest { get; set; } = null!;

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        public bool EmailVerified { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}