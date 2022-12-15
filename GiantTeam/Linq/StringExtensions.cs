namespace GiantTeam.Linq
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string?> values, string separator)
        {
            return string.Join(separator, values);
        }
    }
}
