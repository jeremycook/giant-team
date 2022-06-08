namespace WebApp.Postgres;

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
    public static string Literal(string literal)
    {
        return $"'{literal.Replace(quote, escapedQuote).Replace(backslash, escapedBackslash)}'";
    }
}
