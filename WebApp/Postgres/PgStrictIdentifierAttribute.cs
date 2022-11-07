using System.ComponentModel.DataAnnotations;

namespace WebApp.Postgres
{
    /// <summary>
    /// Requires that the field starts with a lowercase letter, and may be followed by lowercase letters, numbers and underscores.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PgStrictIdentifierAttribute : RegularExpressionAttribute
    {
        public PgStrictIdentifierAttribute() : base("^[a-z][a-z0-9_]*$")
        {
            ErrorMessage = "The {0} field must start with a lowercase letter, and may be followed by lowercase letters, numbers and underscores.";
        }
    }
}
