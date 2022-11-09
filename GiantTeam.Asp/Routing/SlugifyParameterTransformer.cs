using Microsoft.AspNetCore.Routing;
using System.Text;

namespace GiantTeam.Asp.Routing
{
    /// <summary>
    /// Transforms strings from "ThisStyle" to "this-style".
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        private const int caseDifference = 32;

        public string? TransformOutbound(object? value)
        {
            if (value is string text)
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
                }

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
