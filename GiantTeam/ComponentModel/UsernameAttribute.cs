using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="UsernamePattern"/>.
    /// </summary>
    public class UsernameAttribute : RegularExpressionAttribute
    {
        public const string UsernamePattern = "[a-z][a-z0-9_]*";

        public UsernameAttribute() : base("^" + UsernamePattern + "$")
        {
            ErrorMessage = "The {0} must start with a lowercase letter, and may be followed by lowercase letters, numbers or the underscore.";
        }
    }
}
