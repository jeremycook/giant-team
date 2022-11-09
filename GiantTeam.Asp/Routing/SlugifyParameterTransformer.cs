using GiantTeam.Text;
using Microsoft.AspNetCore.Routing;

namespace GiantTeam.Asp.Routing
{
    /// <summary>
    /// Transforms strings from "ThisStyle" to "this-style".
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {

        public string? TransformOutbound(object? value)
        {
            if (value is string text)
            {
                return TextTransformers.Slugify(text);
            }
            else
            {
                return null;
            }
        }
    }
}
