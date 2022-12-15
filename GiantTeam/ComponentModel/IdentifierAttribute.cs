using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace GiantTeam.ComponentModel
{
    /// <summary>
    /// An identifier is made up of printable characters and would be a valid filename.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class IdentifierAttribute : ValidationAttribute
    {
        private const string asciiSymbols = " !$%'()+,-.@[]_{}~^`&/";

        static readonly SortedSet<Rune> allowedAsciiRunes = new((
            asciiSymbols +
            "0123456789" +
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        ).EnumerateRunes());

        static readonly SortedSet<UnicodeCategory> allowedUnicodeCategories = new() {
            UnicodeCategory.UppercaseLetter,
            UnicodeCategory.LowercaseLetter,
            UnicodeCategory.TitlecaseLetter,
            UnicodeCategory.DecimalDigitNumber,

            UnicodeCategory.OtherLetter,
            UnicodeCategory.SpaceSeparator,
            UnicodeCategory.OtherPunctuation,
            UnicodeCategory.CurrencySymbol,
            UnicodeCategory.OpenPunctuation,
            UnicodeCategory.ClosePunctuation,
            UnicodeCategory.MathSymbol,
            UnicodeCategory.DashPunctuation,
            UnicodeCategory.ConnectorPunctuation,
            UnicodeCategory.ModifierSymbol,
            UnicodeCategory.OtherSymbol,
            UnicodeCategory.NonSpacingMark,

	        // Need to research
	        //UnicodeCategory.ModifierLetter,
	        //UnicodeCategory.SpacingCombiningMark,
	        //UnicodeCategory.EnclosingMark,
	        //UnicodeCategory.LetterNumber,
	        //UnicodeCategory.OtherNumber,
	        //UnicodeCategory.LineSeparator,
	        //UnicodeCategory.ParagraphSeparator,
	        //UnicodeCategory.Surrogate,
	        //UnicodeCategory.InitialQuotePunctuation,
	        //UnicodeCategory.FinalQuotePunctuation,

	        // Banned
	        //UnicodeCategory.Control,
	        //UnicodeCategory.Format,
	        //UnicodeCategory.PrivateUse,
	        //UnicodeCategory.OtherNotAssigned,
        };

        public IdentifierAttribute() : base()
        {
            ErrorMessage = $"The {{0}} field contains invalid text ({{1}}).";
        }

        protected virtual bool IsInvalidString(string text, out Rune? firstInvalidRune)
        {
            foreach (var rune in text.EnumerateRunes())
            {
                if (rune.IsAscii)
                {
                    if (!allowedAsciiRunes.Contains(rune))
                    {
                        firstInvalidRune = rune;
                        return true;
                    }
                }
                else
                {
                    if (!allowedUnicodeCategories.Contains(Rune.GetUnicodeCategory(rune)))
                    {
                        firstInvalidRune = rune;
                        return true;
                    }
                }
            }

            firstInvalidRune = null;
            return false;
        }

        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }
            else if (value is string text)
            {
                if (IsInvalidString(text, out _))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException($"The {nameof(value)} argument must be a string but is of type {value.GetType()}.");
            }
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return null;
            }
            else if (value is string text)
            {
                if (IsInvalidString(text, out var invalidRune))
                {
                    var memberNames = string.IsNullOrEmpty(validationContext.MemberName) ?
                        Array.Empty<string>() :
                        new[] { validationContext.MemberName };
                    return new(string.Format(ErrorMessage!, validationContext.DisplayName, invalidRune!.Value), memberNames);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new ArgumentException($"The {nameof(value)} argument must be a string but is of type {value.GetType()}.");
            }
        }
    }
}
