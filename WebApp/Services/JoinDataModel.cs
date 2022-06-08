using System.ComponentModel.DataAnnotations;

namespace WebApp.Services
{
    public class JoinDataModel
    {
        [StringLength(50, MinimumLength = 3)]
        public string DisplayName { get; set; } = default!;

        [EmailAddress]
        [StringLength(150, MinimumLength = 3)]
        public string Email { get; set; } = default!;

        [RegularExpression("^[A-Za-z][A-Za-z0-9]*$")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [StringLength(100, MinimumLength = 10)]
        public string Password { get; set; } = default!;
    }
}
