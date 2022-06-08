using System.ComponentModel.DataAnnotations;

namespace WebApp.Services
{
    public class LoginDataModel
    {
        [RegularExpression("^[A-Za-z][A-Za-z0-9]*$")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [StringLength(100, MinimumLength = 10)]
        public string Password { get; set; } = default!;
    }
}
