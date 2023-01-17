﻿using GiantTeam.ComponentModel;
using GiantTeam.Linq;
using GiantTeam.Text;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Reflection;
using System.Windows.Markup;

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

            parameterValues = tempValues
                .Select(val => val switch
                {
                    char[] charArray => new NpgsqlParameter() { Value = charArray, NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.InternalChar },
                    _ => new NpgsqlParameter() { Value = val },
                })
                .ToArray();
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

        public static Sql Literal(DateTime moment)
        {
            return Raw(PgQuote.Literal(moment));
        }

        public static Sql Literal(DateTimeOffset moment)
        {
            return Raw(PgQuote.Literal(moment));
        }

        private static readonly Dictionary<Assembly, SqlMetadata[]> _assemblySqlMetadatasCache = new();
        private static readonly Dictionary<Type, SqlMetadata> _sqlmetadataCache = new();
        private static readonly Dictionary<Type, Sql> _tableNameCache = new();
        private static readonly Dictionary<Type, Sql> _columnNameCache = new();
        private static readonly Dictionary<Type, PropertyInfo[]> _insertPropertiesCache = new();

        public static SqlMetadata GetSqlMetadata<TTable>()
        {
            return GetSqlMetadata(typeof(TTable));
        }

        public static SqlMetadata GetSqlMetadata(Type tableType)
        {
            if (!_sqlmetadataCache.TryGetValue(tableType, out var sqlMetadata))
            {
                if (!_assemblySqlMetadatasCache.TryGetValue(tableType.Assembly, out var candidates))
                {
                    _assemblySqlMetadatasCache[tableType.Assembly] =
                    candidates =
                        tableType.Assembly.GetTypes()
                        .Where(t => t.IsAssignableTo(typeof(SqlMetadata)))
                        .OrderByDescending(t => t.FullName) // sort longest name to shortest
                        .Select(t => (SqlMetadata)Activator.CreateInstance(t)!)
                        .ToArray();
                }

                _sqlmetadataCache[tableType] =
                sqlMetadata =
                    candidates.FirstOrDefault(c => c.GetType().Namespace!.StartsWith(tableType.Namespace!))
                    ?? new SqlMetadata();
            }

            return sqlMetadata;
        }

        public static Sql GetTableIdentifier<TTable>()
        {
            return GetTableIdentifier(typeof(TTable));
        }

        public static Sql GetTableIdentifier(Type type)
        {
            if (!_tableNameCache.TryGetValue(type, out var sql))
            {
                var sqlMetadata = GetSqlMetadata(type);

                _tableNameCache[type] =
                sql =
                    sqlMetadata.GetFullyQualifiedTableIdentifier(type);
            }

            return sql;
        }

        public static Sql GetColumnIdentifiers<TTable>()
        {
            return GetColumnIdentifiers(typeof(TTable));
        }

        public static Sql GetColumnIdentifiers(Type type)
        {
            if (!_columnNameCache.TryGetValue(type, out var sql))
            {
                var sqlMetadata = GetSqlMetadata(type);

                _columnNameCache[type] =
                sql =
                    Raw(PgQuote.IdentifierList(type.GetProperties().Select(sqlMetadata.GetColumnName)));
            }

            return sql;
        }

        public static Sql Insert<TTable>(TTable row, SqlMetadata? sqlMetadata = null)
        {
            var type = typeof(TTable);
            sqlMetadata ??= GetSqlMetadata(type);

            if (!_insertPropertiesCache.TryGetValue(type, out var properties))
            {
                properties = type
                    .GetProperties()
                    .Where(p =>
                        p.GetSetMethod() != null &&
                        (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
                    )
                    .ToArray();
                _insertPropertiesCache[type] = properties;
            }

            var columns = properties
                .Select(p => (Name: sqlMetadata.GetColumnName(p), Value: p.GetValue(row)))
                .ToDictionary(o => o.Name, o => o.Value);

            var arguments = columns.Values
                .Prepend(IdentifierList(columns.Keys))
                .Prepend(GetTableIdentifier(type))
                .ToArray();

            var sql = new Sql("INSERT INTO {0} ({1}) VALUES (" + Enumerable.Range(2, columns.Count).Select(i => "{" + i + "}").Join(',') + ")", arguments);

            return sql;
        }
    }
}