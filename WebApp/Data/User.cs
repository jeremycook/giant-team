using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Data
{
    [Index(nameof(UsernameLowercase), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [StringLength(100)]
        public string DisplayName { get; set; } = null!;

        [RegularExpression("^[A-Za-z][A-Za-z0-9]*$", ErrorMessage = "The {0} field must start with a letter. The first letter can be followed by letters and numbers.")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        /// <summary>
        /// The lowercase form of <see cref="Username"/>.
        /// </summary>
        public string UsernameLowercase { get => Username?.ToLower()!; private set { } }

        public string PasswordDigest { get; set; } = null!;

        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        public bool EmailConfirmed { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}