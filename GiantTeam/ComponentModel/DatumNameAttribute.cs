using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="DatumNamePattern"/>.
    /// </summary>
    public class DatumNameAttribute : RegularExpressionAttribute
    {
        public const string DatumNamePattern = "^[^<>:\"/\\|?*]+$";

        public DatumNameAttribute() : base(DatumNamePattern)
        {
            ErrorMessage = "The {0} cannot contain ^ < > : \" / \\ | ? * ] + or $ characters.";
        }
    }
}
