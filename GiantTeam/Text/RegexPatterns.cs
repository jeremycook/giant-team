using System.Text.RegularExpressions;

namespace GiantTeam.Text
{
    public static partial class RegexPatterns
    {
        /// <summary>
        /// Matches if entire line is empty or English letters.
        /// </summary>
        public static Regex Alpha { get; } = alpha();

        [GeneratedRegex("^[A-Za-z]*$")]
        private static partial Regex alpha();
    }
}
