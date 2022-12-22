using GiantTeam.Postgres.Parser;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PgIdentifierAttribute : ValidationAttribute
    {
        public PgIdentifierAttribute() : base()
        {
            ErrorMessage = "The {0} field is not a valid identifier.";
        }

        protected virtual bool IsIdentifier(string input)
        {
            try
            {
                if (input.AsSpan().IfIdentifier(out var result, out _) &&
                    result.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (PostgresParserException)
            {
                return false;
            }
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }
            else if (value is string text)
            {
                if (text == string.Empty)
                {
                    return true;
                }
                else
                {
                    text = '"' + text + '"';
                    return IsIdentifier(text);
                }
            }
            else
            {
                throw new ArgumentException($"The {nameof(value)} argument must be a string but is of type {value.GetType()}.");
            }
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (IsValid(value))
            {
                return null;
            }
            else
            {
                var memberNames = string.IsNullOrEmpty(validationContext.MemberName) ?
                    Array.Empty<string>() :
                    new[] { validationContext.MemberName };
                return new(string.Format(ErrorMessage!, validationContext.DisplayName), memberNames);
            }
        }
    }
}
