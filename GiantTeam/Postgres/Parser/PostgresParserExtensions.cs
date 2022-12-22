using GiantTeam.Postgres.Parser.Model;

namespace GiantTeam.Postgres.Parser
{
    public static class PostgresParserExtensions
    {
        public static bool IfExpression(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out Expression expression)
        {
            if (source.IfOpenParen())
            {
                var parenResult = source
                    .OpenParen();

                ParenExpression parenExpression;
                if (parenResult.IfExpression(out parenResult, out var innerExpression))
                {
                    parenExpression = new ParenExpression(innerExpression);
                }
                else
                {
                    parenExpression = new ParenExpression(null);
                }

                parenResult = parenResult
                    .IgnoreWhitespace()
                    .CloseParen();

                if (parenResult.IfCast(out parenResult, out string typeName))
                {
                    result = parenResult;
                    expression = new CastExpression(parenExpression, typeName);
                    return true;
                }
                else
                {
                    result = parenResult;
                    expression = parenExpression;
                    return true;
                }
            }
            else if (source.IfFunction(out var functionResult, out var function))
            {
                if (functionResult.IfCast(out functionResult, out string typeName))
                {
                    result = functionResult;
                    expression = new CastExpression(function, typeName);
                    return true;
                }
                else
                {
                    result = functionResult;
                    expression = function;
                    return true;
                }
            }
            else if (source.IfStatement(out var statementResult, out var special))
            {
                result = statementResult;
                expression = special;
                return true;
            }
            else if (source.IfIdentifier(out var identifierResult, out var identifier))
            {
                if (identifierResult.IfCast(out identifierResult, out string typeName))
                {
                    result = identifierResult;
                    expression = new CastExpression(identifier, typeName);
                    return true;
                }
                else
                {
                    result = identifierResult;
                    expression = identifier;
                    return true;
                }
            }
            else if (source.IfLiteral(out var literalResult, out var literal))
            {
                if (literalResult.IfCast(out literalResult, out string typeName))
                {
                    result = literalResult;
                    expression = new CastExpression(literal, typeName);
                    return true;
                }
                else
                {
                    result = literalResult;
                    expression = literal;
                    return true;
                }
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
            if (source.IfString("CURRENT_TIMESTAMP AT TIME ZONE", out var temp, out var name))
            {
                temp = temp.IgnoreWhitespace();

                if (temp.IfExpression(out var temp2, out var expression) &&
                    (
                        expression is LiteralExpression ||
                        (expression is CastExpression cast && cast.Expression is LiteralExpression)
                    ))
                {
                    // OK
                    temp = temp2;
                }
                else
                {
                    throw new PostgresParserException("a string literal", temp.Length);
                }

                result = temp;
                statement = new(name.ToString(), new[] { expression });
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
                var tmpResult = source
                    .Char(PostgresParserConstants.SingleQuote);

                var index = tmpResult.IndexOf(PostgresParserConstants.SingleQuote);

                if (index < 0)
                {
                    throw new PostgresParserException("a literal string terminated by a single quote", index);
                }

                identifier = new LiteralExpression(tmpResult[..index].ToString());

                tmpResult = tmpResult[index..]
                    .Char(PostgresParserConstants.SingleQuote);

                result = tmpResult;
                return true;

            }

            result = source;
            identifier = null!;
            return false;
        }

        public static bool IfCast(this ReadOnlySpan<char> source, out ReadOnlySpan<char> result, out string typeName)
        {
            var temp = source;

            if (temp.IfString("::", out temp, out _))
            {
                if (!temp.IfUnquotedIdentifier(out temp, out var identifier))
                {
                    throw new PostgresParserException("a data type to cast to", temp.Length);
                }

                result = temp;
                typeName = identifier.Name;
                return true;

            }

            result = source;
            typeName = null!;
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
