using GiantTeam.Text;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace GiantTeam.Postgres
{
    public class Sql
    {
        public static implicit operator Sql(FormattableString sql)
        {
            return Format(sql);
        }

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

        public string Unsanitized => format;

        public override string ToString()
        {
            return ToParameterizedSql(out _);
        }

        public string ToParameterizedSql(out NpgsqlParameter[] parameterValues)
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
                        formatArgs.Add($"${tempValues.Count + 1}");
                        tempValues.Add(arg ?? DBNull.Value);
                        break;
                }
            }

            parameterValues = tempValues.Select((val, i) => new NpgsqlParameter() { Value = val }).ToArray();
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
                        formatArgs.Add($"${parameterValues.Count + 1}");
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
        /// Returns "<paramref name="prefix"/>"."<paramref name="text"/>".
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Sql Identifier(string prefix, string text)
        {
            return Raw(PgQuote.Identifier(prefix, text));
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

        public static Sql GetTableIdentifier<T>()
        {
            return GetTableIdentifier(typeof(T));
        }

        public static Sql GetTableIdentifier(Type type)
        {
            if (type.GetCustomAttribute<TableAttribute>() is TableAttribute table)
            {
                if (table.Schema is not null)
                {
                    return Identifier(table.Schema, table.Name);
                }
                else
                {
                    return Identifier(table.Name);
                }
            }
            else
            {
                return Identifier(TextTransformers.Snakify(type.Name));
            }
        }

        public static Sql GetColumnIdentifiers<T>()
        {
            return GetColumnIdentifiers(typeof(T));
        }

        public static Sql GetColumnIdentifiers(Type type)
        {
            return Raw(PgQuote.IdentifierList(type.GetProperties().Select(p => p.Name).Select(TextTransformers.Snakify)));
        }
    }
}