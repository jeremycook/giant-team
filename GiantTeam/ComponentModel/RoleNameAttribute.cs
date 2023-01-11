using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="RoleNamePattern"/>.
    /// </summary>
    public class RoleNameAttribute : RegularExpressionAttribute
    {
        public const string RoleNamePattern = "^[^<>:\"/\\|?*]+$";

        public RoleNameAttribute() : base(RoleNamePattern)
        {
            ErrorMessage = "The {0} cannot contain ^ < > : \" / \\ | ? * ] + or $ characters.";
        }
    }
}
