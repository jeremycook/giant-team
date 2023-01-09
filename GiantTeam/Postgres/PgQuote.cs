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

    public static string Identifier(string identifier)
    {
        return $"\"{identifier.Replace(doubleQuote, escapedDoubleQuote)}\"";
    }
    public static string Identifier(string identifier1, string identifier2)
    {
        return Identifier(identifier1) + "." + Identifier(identifier2);
    }
    /// <summary>
    /// Comma separated list of sanitized identifiers.
    /// </summary>
    /// <param name="identifiers"></param>
    /// <returns></returns>
    public static string IdentifierList(params string[] identifiers)
    {
        return IdentifierList((IEnumerable<string>)identifiers);
    }
    /// <summary>
    /// Comma separated list of sanitized identifiers.
    /// </summary>
    /// <param name="identifiers"></param>
    /// <returns></returns>
    public static string IdentifierList(IEnumerable<string> identifiers)
    {
        return identifiers.Select(Identifier).Join(',');
    }
    public static string Literal(string literal)
    {
        return $"'{literal.Replace(quote, escapedQuote).Replace(backslash, escapedBackslash)}'";
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
