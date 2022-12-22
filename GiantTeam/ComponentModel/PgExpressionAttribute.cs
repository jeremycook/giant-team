using GiantTeam.Postgres.Parser;
using GiantTeam.Postgres.Parser.Model;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.ComponentModel
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PgExpressionAttribute : ValidationAttribute
    {
        public PgExpressionAttribute() : base()
        {
            ErrorMessage = "The {0} is not a valid expression.";
        }

        protected virtual bool IsExpression(string input)
        {
            try
            {
                if (input.AsSpan().IgnoreWhitespace().IfExpression(out var result, out var expression) &&
                    result.IgnoreWhitespace().IsEmpty)
                {
                    while (expression is ParenExpression parenExpression)
                    {
                        if (parenExpression.Expression is null)
                        {
                            // Empty expressions are not allowed
                            return false;
                        }
                        else
                        {
                            expression = parenExpression.Expression;
                        }
                    }

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
                    return IsExpression(text);
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
