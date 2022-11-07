using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Postgres
{
    /// <summary>
    /// Requires that the field only contain letters, numbers, underscores, dashes and spaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PgLaxIdentifierAttribute : RegularExpressionAttribute
    {
        public PgLaxIdentifierAttribute() : base("^[a-zA-Z][a-zA-Z0-9-_ ]*$")
        {
            ErrorMessage = "The {0} field may only contain letters, numbers, underscores, dashes and spaces.";
        }
    }
}
