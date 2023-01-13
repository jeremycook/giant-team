using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="InodeNamePattern"/>.
    /// </summary>
    public class InodeNameAttribute : RegularExpressionAttribute
    {
        public const string InodeNamePattern = "^[^<>:\"/\\|?*]+$";

        public InodeNameAttribute() : base(InodeNamePattern)
        {
            ErrorMessage = "The {0} cannot contain ^ < > : \" / \\ | ? * ] + or $ characters.";
        }
    }
}
