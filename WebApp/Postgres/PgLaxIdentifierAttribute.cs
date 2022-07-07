using System.ComponentModel.DataAnnotations;

namespace WebApp.Postgres
{
    /// <summary>
    /// Requires that the field only contain letters, numbers, underscores, dashes and spaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PgLaxIdentifierAttribute : RegularExpressionAttribute
    {
        public PgLaxIdentifierAttribute() : base("^[A-Za-z0-9-_ ]*$")
        {
            ErrorMessage = "The {0} field may only contain letters, numbers, dashes, underscores and spaces.";
        }
    }
}
