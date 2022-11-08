using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Asp.Services
{
    public class PasswordAuthenticationInput
    {
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [StringLength(100, MinimumLength = 10)]
        public string Password { get; set; } = default!;
    }
}
