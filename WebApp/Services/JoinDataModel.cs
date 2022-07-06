using System.ComponentModel.DataAnnotations;

namespace WebApp.Services
{
    public class JoinDataModel
    {
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = default!;

        [EmailAddress]
        [StringLength(200, MinimumLength = 3)]
        public string Email { get; set; } = default!;

        [RegularExpression("^[A-Za-z][A-Za-z0-9]*$")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [StringLength(100, MinimumLength = 10)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
    }
}
