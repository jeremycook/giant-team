using GiantTeam.Postgres.Models;
using Npgsql;
using System.Collections.Immutable;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using static GiantTeam.Postgres.Sql;

namespace GiantTeam.Postgres
{
    public abstract partial class PgDataServiceBase
    {
        protected abstract ILogger Logger { get; }
        protected abstract string ConnectionString { get; }

        public virtual NpgsqlDataSource CreateDataSource()
        {
            return NpgsqlDataSource.Create(ConnectionString);
        }

        /// <summary>
        /// Returns the number of rows affected.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public async Task<int> ExecuteAsync(params FormattableString[] commands)
        {
            return await ExecuteAsync(commands.Select(Format));
        }

        /// <summary>
        /// Returns the number of rows affected.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public async Task<int> ExecuteAsync(IEnumerable<Sql> commands)
        {
            await using var batch = new NpgsqlBatch();
            foreach (var command in commands)
            {
                batch.BatchCommands.Add(command);
            }
            return await ExecuteAsync(batch);
        }

        /// <summary>
        /// Returns the number of rows affected.
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public async Task<int> ExecuteAsync(NpgsqlBatch batch)
        {
            await using var dataSource = CreateDataSource();
            await using var connection = await dataSource.OpenConnectionAsync();
            batch.Connection = connection;
            try
            {
                return await batch.ExecuteNonQueryAsync();
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing batch commands {CommandTextList}", batch.BatchCommands.OfType<NpgsqlBatchCommand>().Select(cmd => cmd.CommandText));
                throw;
            }
        }

        /// <summary>
        /// Use ExecuteAsync instead. Returns the number of rows affected.
        /// </summary>
        /// <param name="unsanitizedSql"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        [Obsolete("Use ExecuteAsync instead.")]
        public async Task<int> ExecuteUnsanitizedAsync(string unsanitizedSql)
        {
            await using var dataSource = CreateDataSource();
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = unsanitizedSql;
            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing unsanitized command {CommandText}", cmd.CommandText);
                throw;
            }
        }

        /// <summary>
        /// Returns a <see cref="QueryTable"/>.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DbException"></exception>
        public async Task<QueryTable> QueryTableAsync(Sql sql)
        {
            await using var dataSource = CreateDataSource();
            await using var batch = dataSource.CreateBatch();
            batch.BatchCommands.Add(sql);

            if (batch.BatchCommands[0].StatementType == StatementType.Select)
            {
                throw new ArgumentException($"The {nameof(sql)} argument must be a SELECT statement.", nameof(sql));
            }

            try
            {
                await using var reader = await batch.ExecuteReaderAsync();
                IReadOnlyCollection<Npgsql.Schema.NpgsqlDbColumn> columnSchema = await reader.GetColumnSchemaAsync();

                var cols = columnSchema.Select(o => o.ColumnName).ToArray();
                var rows = new List<object?[]>();
                while (await reader.ReadAsync())
                {
                    var row = new object?[cols.Length];
                    reader.GetValues(row!);
                    for (int i = 0; i < cols.Length; i++)
                    {
                        if (row[i] == DBNull.Value)
                            row[i] = null;
                    }
                    rows.Add(row);
                }

                return new()
                {
                    Columns = cols,
                    Rows = rows,
                };
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing query {QueryText}", batch.BatchCommands[0]);
                throw;
            }
        }

        /// <summary>
        /// Returns one <typeparamref name="T"/> or <c>null</c>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="DbException"></exception>
        public async Task<T?> SingleOrDefaultAsync<T>(Sql? sql = null)
            where T : new()
        {
            if (sql is null)
            {
                var type = typeof(T);
                sql = Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} LIMIT 2");
            }
            else if (WhereRegex().IsMatch(sql.Unsanitized))
            {
                // WHERE clause
                var type = typeof(T);
                sql = Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql} LIMIT 2");
            }
            else if (FromRegex().IsMatch(sql.Unsanitized))
            {
                // FROM clause
                var type = typeof(T);
                sql = Format($"SELECT {GetColumnIdentifiers(type)} {sql} LIMIT 2");
            }
            else
            {
                sql = Format($"SELECT * FROM ({sql}) query LIMIT 2");
            }

            try
            {
                int count = 0;
                T? lastItem = default;
                await foreach (var item in QueryAsync<T>(sql))
                {
                    lastItem = item;
                    count++;
                }

                if (count <= 1)
                {
                    return lastItem;
                }
                else
                {
                    throw new InvalidOperationException("More than one element.");
                }
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing query {QueryText}", sql.ToString());
                throw;
            }
        }

        /// <summary>
        /// Returns one <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="DbException"></exception>
        public async Task<T> SingleAsync<T>(Sql? sql = null)
            where T : new()
        {
            if (sql is null)
            {
                var type = typeof(T);
                sql = Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} LIMIT 2");
            }
            else if (WhereRegex().IsMatch(sql.Unsanitized))
            {
                // WHERE clause
                var type = typeof(T);
                sql = Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql} LIMIT 2");
            }
            else if (FromRegex().IsMatch(sql.Unsanitized))
            {
                // FROM clause
                var type = typeof(T);
                sql = Format($"SELECT {GetColumnIdentifiers(type)} {sql} LIMIT 2");
            }
            else
            {
                sql = Format($"SELECT * FROM ({sql}) query LIMIT 2");
            }

            try
            {
                int count = 0;
                T? lastItem = default;
                await foreach (var item in QueryAsync<T>(sql))
                {
                    lastItem = item;
                    count++;
                }

                if (count == 1)
                {
                    return lastItem!;
                }
                else if (count == 0)
                {
                    throw new InvalidOperationException("No elements.");
                }
                else
                {
                    throw new InvalidOperationException("More than one element.");
                }
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing query {QueryText}", sql.ToString());
                throw;
            }

        }

        /// <summary>
        /// Returns a list of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="itemFactory"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public async Task<List<T>> ListAsync<T>(Sql? sql = null)
            where T : new()
        {
            Type type = typeof(T);

            if (sql is null)
            {
                sql = Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)}");
            }
            else if (WhereRegex().IsMatch(sql.Unsanitized))
            {
                // WHERE clause
                sql = Format($"SELECT {GetColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql}");
            }
            else if (FromRegex().IsMatch(sql.Unsanitized))
            {
                // FROM clause
                sql = Format($"SELECT {GetColumnIdentifiers(type)} {sql}");
            }

            var list = new List<T>();
            try
            {
                await foreach (var item in QueryAsync<T>(sql))
                {
                    list.Add(item);
                }
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing query {QueryText}", sql.ToString());
                throw;
            }

            return list;
        }

        /// <summary>
        /// Asynchronously query and iterate through the rows converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="itemFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async IAsyncEnumerable<T> QueryAsync<T>(Sql sql)
            where T : new()
        {
            // TODO: LRU cache stuff like the column schema

            await using var dataSource = CreateDataSource();
            await using var batch = dataSource.CreateBatch();
            batch.BatchCommands.Add(sql);

            if (batch.BatchCommands[0].StatementType == StatementType.Select)
            {
                throw new ArgumentException($"The {nameof(sql)} argument must be a SELECT statement.", nameof(sql));
            }

            await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync();

            Type type = typeof(T);
            var columnSchema = await reader.GetColumnSchemaAsync();
            var columnProperties = columnSchema
                .Select(o =>
                    type.GetProperty(o.ColumnName.Replace("_", ""), BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) ??
                    throw new InvalidOperationException($"Failed to match a property of {type} to a column named {o.ColumnName}.")
                )
                .ToImmutableArray();

            while (await reader.ReadAsync())
            {
                var item = ItemFactory<T>(columnProperties, reader);
                yield return item;
            }
        }

        /// <summary>
        /// Asynchronously query and iterate through the rows converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="itemFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async IAsyncEnumerable<T> QueryAsync<T>(Sql sql, Func<NpgsqlDataReader, T> itemFactory)
        {
            await using var dataSource = CreateDataSource();
            await using var batch = dataSource.CreateBatch();
            batch.BatchCommands.Add(sql);

            if (batch.BatchCommands[0].StatementType == StatementType.Select)
            {
                throw new ArgumentException($"The {nameof(sql)} argument must be a SELECT statement.", nameof(sql));
            }

            await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = itemFactory(reader);
                yield return item;
            }
        }

        private static T ItemFactory<T>(IEnumerable<PropertyInfo> columnProperties, NpgsqlDataReader reader)
            where T : new()
        {
            T item = new();

            int i = -1;
            foreach (var property in columnProperties)
            {
                i++;
                var fieldType = reader.GetFieldType(i);

                object? value;
                if (property.PropertyType == typeof(DateTimeOffset) &&
                    fieldType == typeof(DateTime))
                {
                    value = reader.GetFieldValue<DateTimeOffset>(i);
                }
                else
                {
                    value = reader.GetValue(i);
                    if (value == DBNull.Value)
                    {
                        value = null;
                    }
                }

                property.SetValue(item, value);
            }

            return item;
        }

        [GeneratedRegex("^\\s+WHERE\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
        private static partial Regex WhereRegex();

        [GeneratedRegex("^\\s+FROM\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
        private static partial Regex FromRegex();
    }
}
