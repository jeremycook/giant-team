using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="DatabaseNamePattern"/>.
    /// </summary>
    public class DatabaseNameAttribute : RegularExpressionAttribute
    {
        public const string DatabaseNamePattern = "^[a-z][a-z0-9_]*$";

        public DatabaseNameAttribute() : base(DatabaseNamePattern)
        {
            ErrorMessage = "The {0} must start with a lowercase letter, and may be followed by lowercase letters, numbers or the underscore.";
        }
    }
}
