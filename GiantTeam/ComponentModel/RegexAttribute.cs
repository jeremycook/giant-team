using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// Requires the value match <see cref="DatabaseNamePattern"/>.
    /// </summary>
    public class RegexAttribute : ValidationAttribute
    {
        public RegexAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
            : this(pattern, RegexOptions.None) { }

        public RegexAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, RegexOptions options)
        {
            Regex = new Regex('^' + pattern + '$', options);
            ErrorMessage = "The {0} must start with a lowercase letter, and may be followed by lowercase letters, numbers or the underscore.";
        }

        public Regex Regex { get; }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }
            else if (value is string text)
            {
                var valid = Regex.IsMatch(text);
                return valid;
            }
            else
            {
                throw new ArgumentException($"The {nameof(value)} argument must be a string or null.", nameof(value));
            }
        }
    }
}
