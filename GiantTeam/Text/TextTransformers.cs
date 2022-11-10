using System.Text;

namespace GiantTeam.Text
{
    public static class TextTransformers
    {
        private const int caseDifference = 32;

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
                        sb.Append('-');
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
    }
}
