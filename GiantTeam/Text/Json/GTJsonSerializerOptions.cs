using System.Text.Json;

namespace GiantTeam.Text.Json
{
    public class GTJsonSerializerOptions
    {
        /// <summary>
        /// Relaxed options that allow trailing commas, camel and pascal case, and skips comments.
        /// </summary>
        public static JsonSerializerOptions Relaxed { get; } = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        /// <summary>
        /// <see cref="Relaxed"/> options with snake case naming policy.
        /// </summary>
        public static JsonSerializerOptions SnakeCase { get; } = new JsonSerializerOptions(Relaxed)
        {
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
        };

        /// <summary>
        /// Strict options.
        /// </summary>
        public static JsonSerializerOptions Strict { get; } = new JsonSerializerOptions();

        /// <summary>
        /// <see cref="Strict"/> options with snake case naming policy.
        /// </summary>
        public static JsonSerializerOptions StrictSnakeCase { get; } = new JsonSerializerOptions(Strict)
        {
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
        };
    }
}
