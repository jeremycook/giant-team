using Npgsql;

namespace GiantTeam.Postgres
{
    public class Sql
    {
        public static implicit operator NpgsqlBatchCommand(Sql sql)
        {
            NpgsqlBatchCommand batchCommand = new(sql.ToParameterizedSql(out var parameterValues));
            foreach (var value in parameterValues)
            {
                batchCommand.Parameters.Add(value);
            }

            return batchCommand;
        }

        private readonly string format;
        private readonly object?[] arguments;

        public Sql(string format, object?[] arguments)
        {
            this.format = format;
            this.arguments = arguments;
        }

        public override string ToString()
        {
            return ToParameterizedSql(out _);
        }

        public string ToParameterizedSql(out List<object> parameterValues)
        {
            var tempValues = new List<object>();
            var formatArgs = new List<string>(arguments.Length);

            foreach (var arg in arguments)
            {
                switch (arg)
                {
                    case Sql sql:
                        formatArgs.Add(sql.GetParameterizedSql(ref tempValues));
                        break;

                    default:
                        formatArgs.Add($"${tempValues.Count}");
                        tempValues.Add(arg ?? DBNull.Value);
                        break;
                }
            }

            parameterValues = tempValues;
            return string.Format(format, args: formatArgs.ToArray());
        }

        private string GetParameterizedSql(ref List<object> parameterValues)
        {
            var formatArgs = new List<string>(arguments.Length);

            foreach (var arg in arguments)
            {
                switch (arg)
                {
                    case Sql sql:
                        formatArgs.Add(sql.GetParameterizedSql(ref parameterValues));
                        break;

                    default:
                        formatArgs.Add($"${parameterValues.Count}");
                        parameterValues.Add(arg ?? DBNull.Value);
                        break;
                }
            }

            return string.Format(format, args: formatArgs.ToArray());
        }

        public static Sql Format(FormattableString formattableString)
        {
            return new(formattableString.Format, formattableString.GetArguments());
        }

        public static Sql Raw(string text)
        {
            return new(text, Array.Empty<object?>());
        }

        public static Sql Identifier(string text)
        {
            return Raw(PgQuote.Identifier(text));
        }

        /// <summary>
        /// Comma separated list of sanitized identifiers.
        /// </summary>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static Sql IdentifierList(params string[] texts)
        {
            return IdentifierList((IEnumerable<string>)texts);
        }

        /// <summary>
        /// Comma separated list of sanitized identifiers.
        /// </summary>
        /// <param name="identifiers"></param>
        /// <returns></returns>
        public static Sql IdentifierList(IEnumerable<string> texts)
        {
            return Raw(PgQuote.IdentifierList(texts));
        }

        public static Sql Literal(string text)
        {
            return Raw(PgQuote.Literal(text));
        }

        public static Sql Literal(DateTimeOffset moment)
        {
            return Raw(PgQuote.Literal(moment));
        }
    }
}