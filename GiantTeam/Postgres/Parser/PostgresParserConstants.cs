using System.Text.RegularExpressions;

namespace GiantTeam.Postgres.Parser
{
    public static class PostgresParserConstants
    {
        /// <summary>
        /// Matches a space, tab, carriage return, line feed, form feed, or vertical tab.
        /// </summary>
        public const string Whitespace = " \n\r\t\f\v";

        public const char SingleQuote = '\'';

        public const char OpenParen = '(';
        public const char CloseParen = ')';

        public static Regex OpenFunction { get; } = new(@"^([a-zA-Z][a-zA-Z0-9_]*)\s*\(");

        public static Regex UnquotedIdentifier { get; } = new(@"^[a-zA-Z][a-zA-Z0-9_]*");
        public static Regex QuotedIdentifier { get; } = new(@"^""([a-zA-Z][\w0-9 ._-]*)""");
    }
}