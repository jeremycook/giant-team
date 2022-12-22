using GiantTeam.Postgres.Parser.Model;

namespace GiantTeam.Postgres.Parser
{
    public static class PostgresParserExtensions
    {
        public static bool IfExpression(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out Expression expression)
        {
            if (source.IfOpenParen())
            {
                var tempResult = source
                    .OpenParen();

                if (tempResult.IfExpression(out tempResult, out var innerExpression))
                {
                    expression = new ParenExpression(innerExpression);
                }
                else
                {
                    expression = new ParenExpression(null);
                }

                tempResult = tempResult
                    .IgnoreWhitespace()
                    .CloseParen();

                result = tempResult;
                return true;
            }
            else if (source.IfFunction(out var functionResult, out var function))
            {
                result = functionResult;
                expression = function;
                return true;
            }
            else if (source.IfStatement(out var specialResult, out var special))
            {
                result = specialResult;
                expression = special;
                return true;
            }
            else if (source.IfIdentifier(out var identifierResult, out var identifier))
            {
                result = identifierResult;
                expression = identifier;
                return true;
            }
            else if (source.IfLiteral(out var literalResult, out var literal))
            {
                result = literalResult;
                expression = literal;
                return true;
            }

            result = source;
            expression = null!;
            return false;
        }

        public static bool IfFunction(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out FunctionExpression function)
        {
            if (source.IfUnquotedIdentifier(out var tempResult, out var identifier) &&
                tempResult.IgnoreWhitespace().StartsWith("("))
            {
                var name = identifier.Name;

                tempResult = tempResult
                    .IgnoreWhitespace()
                    .OpenParen();

                List<Expression> args = new();
                while (tempResult.IfExpression(out var argResult, out var argExpression))
                {
                    args.Add(argExpression);

                    tempResult = argResult
                        .IgnoreWhitespace();
                    if (tempResult.StartsWith(","))
                    {
                        tempResult = tempResult
                            .Char(',')
                            .IgnoreWhitespace();
                    }
                }

                tempResult = tempResult
                    .CloseParen();

                result = tempResult;
                function = new FunctionExpression(name, args);
                return true;
            }

            result = source;
            function = null!;
            return false;
        }

        public static bool IfIdentifier(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out IdentifierExpression identifier)
        {
            if (source.IfQuotedIdentifier(out result, out var quotedIdentifier))
            {
                identifier = quotedIdentifier;
                return true;
            }

            if (source.IfUnquotedIdentifier(out result, out var unquotedIdentifier))
            {
                identifier = unquotedIdentifier;
                return true;
            }

            result = source;
            identifier = null!;
            return false;
        }

        public static bool IfQuotedIdentifier(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out QuotedIdentifierExpression identifier)
        {
            if (PostgresParserConstants.QuotedIdentifier.IsMatch(source))
            {
                var text = source[0..Math.Min(128, source.Length)].ToString();
                var match = PostgresParserConstants.QuotedIdentifier.Match(text);
                if (match.Success)
                {
                    result = source[match.ValueSpan.Length..];
                    identifier = new QuotedIdentifierExpression(match.Groups[1].Value);
                    return true;
                }
                throw new PostgresParserException("a quoted identifier that is no more than 128 characters long", 0);
            }

            result = source;
            identifier = null!;
            return false;
        }

        public static bool IfUnquotedIdentifier(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out UnquotedIdentifierExpression identifier)
        {
            if (PostgresParserConstants.UnquotedIdentifier.IsMatch(source))
            {
                var text = source[0..Math.Min(128, source.Length)].ToString();
                var match = PostgresParserConstants.UnquotedIdentifier.Match(text);
                if (match.Success)
                {
                    result = source[match.ValueSpan.Length..];
                    identifier = new UnquotedIdentifierExpression(match.Groups[0].Value);
                    return true;
                }
                throw new PostgresParserException("an unquoted identifier that is no more than 128 characters long", 0);
            }

            result = source;
            identifier = null!;
            return false;
        }

        public static bool IfStatement(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out StatementExpression statement)
        {
            if (source.IfString("CURRENT_TIMESTAMP AT TIME ZONE", out result, out var name))
            {
                result = result.IgnoreWhitespace();

                if (!result.IfLiteral(out var literalResult, out var literal))
                {
                    throw new PostgresParserException("a string literal", result.Length);
                }

                result = literalResult;
                statement = new(name.ToString(), new[] { literal });
                return true;
            }

            result = source;
            statement = null!;
            return false;
        }

        public static bool IfString(this ReadOnlySpan<char> source, ReadOnlySpan<char> search, out ReadOnlySpan<char> result, out ReadOnlySpan<char> found)
        {
            if (source.StartsWith(search, StringComparison.InvariantCultureIgnoreCase))
            {
                found = source[..search.Length];
                result = source[search.Length..];
                return true;
            }

            result = source;
            found = null!;
            return false;
        }


        public static bool IfLiteral(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out LiteralExpression identifier)
        {
            if (!source.IsEmpty && source[0] == PostgresParserConstants.SingleQuote)
            {
                result = source
                    .Char(PostgresParserConstants.SingleQuote);

                var index = result.IndexOf(PostgresParserConstants.SingleQuote);

                if (index > -1)
                {
                    identifier = new LiteralExpression(result[..index].ToString());

                    result = result[index..]
                        .Char(PostgresParserConstants.SingleQuote);

                    return true;
                }

                throw new PostgresParserException("a literal string terminated by a single quote", index);
            }

            result = source;
            identifier = null!;
            return false;
        }

        public static bool IfOpenParen(this ReadOnlySpan<char> source)
        {
            return !source.IsEmpty && source[0] == PostgresParserConstants.OpenParen;
        }

        public static ReadOnlySpan<char> OpenParen(this ReadOnlySpan<char> source)
        {
            return source.Char(PostgresParserConstants.OpenParen);
        }

        public static ReadOnlySpan<char> CloseParen(this ReadOnlySpan<char> source)
        {
            return source.Char(PostgresParserConstants.CloseParen);
        }

        public static ReadOnlySpan<char> Char(this ReadOnlySpan<char> source, char match)
        {
            if (source.IsEmpty || source[0] != match)
            {
                throw new PostgresParserException($"'{match}'", source.Length);
            }

            return source[1..];
        }

        public static ReadOnlySpan<char> IgnoreWhitespace(this ReadOnlySpan<char> source)
        {
            int i = 0;
            foreach (char ch in source)
            {
                if (!PostgresParserConstants.Whitespace.Contains(ch))
                    break;

                i++;
            }

            return source[i..];
        }
    }
}
