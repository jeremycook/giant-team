using Npgsql;

namespace GiantTeam.Postgres
{
    public class Sql
    {
        public static implicit operator NpgsqlBatchCommand(Sql sql)
        {
            var parameterValues = new List<object>();

            NpgsqlBatchCommand batchCommand = new(sql.ToParameterizedSql(ref parameterValues));
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

        public string ToParameterizedSql(ref List<object> parameterValues)
        {
            var formatArgs = new List<string>(arguments.Length);

            foreach (var arg in arguments)
            {
                switch (arg)
                {
                    case Sql sql:
                        formatArgs.Add(sql.ToParameterizedSql(ref parameterValues));
                        break;

                    default:
                        formatArgs.Add($"${parameterValues.Count}");
                        parameterValues.Add(arg ?? DBNull.Value);
                        break;
                }
            }

            try
            {
                return string.Format(format, args: formatArgs.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
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

        internal static Sql Literal(string text)
        {
            return Raw(PgQuote.Literal(text));
        }

        internal static Sql Literal(DateTimeOffset moment)
        {
            return Raw(PgQuote.Literal(moment));
        }
    }
}