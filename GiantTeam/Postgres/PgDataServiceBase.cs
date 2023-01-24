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

        private NpgsqlDataSource? _dataSource;

        public async ValueTask<PgDataServiceScope> BeginScopeAsync(CancellationToken cancellationToken = default)
        {
            if (_dataSource is not null)
            {
                throw new InvalidOperationException($"A scope has already been started.");
            }

            _dataSource = NpgsqlDataSource.Create(ConnectionString);
            var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            return new(connection, transaction, () => { _dataSource = null; return ValueTask.CompletedTask; });
        }

        public virtual NpgsqlDataSource AcquireDataSource()
        {
            return _dataSource ?? NpgsqlDataSource.Create(ConnectionString);
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
        public async Task<int> ExecuteAsync(params Sql[] commands)
        {
            return await ExecuteAsync((IEnumerable<Sql>)commands);
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
            await using var dataSource = AcquireDataSource();
            await using var connection = await dataSource.OpenConnectionAsync();
            batch.Connection = connection;
            try
            {
                return await batch.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
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
        public async Task<int> ExecuteUnsanitizedAsync(string unsanitizedSql)
        {
            await using var dataSource = AcquireDataSource();
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
        /// Returns a <see cref="TabularData"/>.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DbException"></exception>
        public async Task<TabularData> TabularQueryAsync(Sql sql)
        {
            await using var dataSource = AcquireDataSource();
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
        /// Returns first column of the first row in the result set, or a null reference if the result set is empty.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DbException"></exception>
        public async Task<object?> ScalarAsync(FormattableString sql)
        {
            return await ScalarAsync(Format(sql));
        }

        /// <summary>
        /// Returns first column of the first row in the result set, or a null reference if the result set is empty.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DbException"></exception>
        public async Task<object?> ScalarAsync(NpgsqlBatchCommand sql)
        {
            try
            {
                await using var dataSource = AcquireDataSource();
                await using var connection = await dataSource.OpenConnectionAsync();
                await using var batch = connection.CreateBatch();
                batch.BatchCommands.Add(sql);

                if (batch.BatchCommands[0].StatementType == StatementType.Select)
                {
                    throw new ArgumentException($"The {nameof(sql)} argument must be a SELECT statement.", nameof(sql));
                }

                try
                {
                    return await batch.ExecuteScalarAsync();
                }
                catch (DbException ex)
                {
                    Logger.LogError(ex, "Error executing batch scalar query command {CommandText}", batch.BatchCommands[0].CommandText);
                    throw;
                }
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "Error executing scalar query {QueryText}", sql.ToString());
                throw;
            }

        }

        public async Task<T?> SingleOrDefaultAsync<T>(FormattableString sql)
            where T : new()
        {
            return await SingleOrDefaultAsync<T>(Format(sql));
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
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} LIMIT 2");
            }
            else if (WhereRegex().IsMatch(sql.Unsanitized))
            {
                // WHERE clause
                var type = typeof(T);
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql} LIMIT 2");
            }
            else if (FromRegex().IsMatch(sql.Unsanitized))
            {
                // FROM clause
                var type = typeof(T);
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} {sql} LIMIT 2");
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

        public async Task<T> SingleAsync<T>(FormattableString sql)
            where T : new()
        {
            return await SingleAsync<T>(Format(sql));
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
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} LIMIT 2");
            }
            else if (WhereRegex().IsMatch(sql.Unsanitized))
            {
                // WHERE clause
                var type = typeof(T);
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql} LIMIT 2");
            }
            else if (FromRegex().IsMatch(sql.Unsanitized))
            {
                // FROM clause
                var type = typeof(T);
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} {sql} LIMIT 2");
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

        public async Task<List<T>> ListAsync<T>(FormattableString sql)
            where T : new()
        {
            return await ListAsync<T>(Format(sql));
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
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)}");
            }
            else if (FromRegex().IsMatch(sql.Unsanitized))
            {
                // FROM clause
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} {sql}");
            }
            else if (WhereRegex().IsMatch(sql.Unsanitized))
            {
                // WHERE clause
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql}");
            }
            else if (OrderByRegex().IsMatch(sql.Unsanitized))
            {
                // ORDER BY clause
                sql = Format($"SELECT {GetSelectColumnIdentifiers(type)} FROM {GetTableIdentifier(type)} {sql}");
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

            await using var dataSource = AcquireDataSource();
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
            await using var dataSource = AcquireDataSource();
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

        [GeneratedRegex("^\\s*FROM\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
        private static partial Regex FromRegex();

        [GeneratedRegex("^\\s*WHERE\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
        private static partial Regex WhereRegex();

        [GeneratedRegex("^\\s*ORDER BY\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
        private static partial Regex OrderByRegex();
    }
}
