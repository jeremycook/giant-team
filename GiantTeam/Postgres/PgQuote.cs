using GiantTeam.Linq;

namespace GiantTeam.Postgres;

public static class PgQuote
{
    private const string quote = "'";
    private const string escapedQuote = "''";

    private const string backslash = @"\";
    private const string escapedBackslash = @"\\";

    private const string doubleQuote = "\"";
    private const string escapedDoubleQuote = "\"\"";

    public static string Identifier(string name)
    {
        return $"\"{name.Replace(doubleQuote, escapedDoubleQuote)}\"";
    }
    public static string Identifier(string name1, string name2)
    {
        return Identifier(name1) + "." + Identifier(name2);
    }
    /// <summary>
    /// Comma separated list of sanitized identifiers.
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    public static string IdentifierList(params string[] names)
    {
        return IdentifierList((IEnumerable<string>)names);
    }
    /// <summary>
    /// Comma separated list of sanitized identifiers.
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    public static string IdentifierList(IEnumerable<string> names)
    {
        return names.Select(Identifier).Join(',');
    }
    public static string Literal(string text)
    {
        return $"'{text.Replace(quote, escapedQuote).Replace(backslash, escapedBackslash)}'";
    }
    public static string Literal(DateTime moment)
    {
        if (moment.Kind != DateTimeKind.Utc)
            throw new ArgumentException($"The {moment} argument must be in UTC.", nameof(moment));

        return Literal(moment.ToString("o"));
    }
    public static string Literal(DateTimeOffset moment)
    {
        return Literal(moment.ToString("u"));
    }
}
