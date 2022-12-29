using System.Globalization;
using System.Text;

namespace GiantTeam.Text
{
    public static class TextTransformers
    {
        private const int caseDifference = 32;
        private const char dash = '-';
        private static readonly Rune underscore = new('_');

        public static string Slugify(string text)
        {
            var sb = new StringBuilder(text.Length);

            char last = '\0';
            foreach (var ch in text)
            {
                if (char.IsUpper(ch))
                {
                    if (char.IsLower(last))
                    {
                        sb.Append(dash);
                    }
                    sb.Append((char)(ch + caseDifference));
                }
                else
                {
                    sb.Append(ch);
                }

                last = ch;
            }

            return sb.ToString();
        }

        public static string Snakify(string text)
        {
            var sb = new List<Rune>();

            Rune last = new('\0');
            foreach (var rune in text.EnumerateRunes())
            {
                if (Rune.IsUpper(rune))
                {
                    if (Rune.IsLower(last))
                    {
                        sb.Add(underscore);
                    }
                    sb.Add(Rune.ToLowerInvariant(rune));
                }
                else
                {
                    sb.Add(rune);
                }

                last = rune;
            }

            return string.Concat(sb);
        }
    }
}
