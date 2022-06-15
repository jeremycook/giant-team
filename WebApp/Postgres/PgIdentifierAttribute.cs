using System.ComponentModel.DataAnnotations;

namespace WebApp.Postgres
{
    /// <summary>
    /// Requires that the field starts with a lowercase letter followed by numbers and/or lowercase letters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PgIdentifierAttribute : RegularExpressionAttribute
    {
        public PgIdentifierAttribute() : base("^[a-z][a-z0-9_]*$")
        {
            ErrorMessage = "The {0} field must start with a lowercase letter followed by numbers and/or lowercase letters.";
        }
    }
}
